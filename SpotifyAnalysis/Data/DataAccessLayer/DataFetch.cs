using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotifyAnalysis.Data;
using SpotifyAPI.Web;
using System.Threading;
using System.Linq;
using System;
using System.Collections.Generic;


namespace SpotifyAnalysis.Data.DataAccessLayer {
    public delegate Task<UserDTO> GetUserProfileDelegate(string userID);
    public delegate Task<IList<PlaylistDTO>> GetUsersPublicPlaylistsDelegate(string userID);
    public delegate Task<FullPlaylist> GetPlaylistAsyncDelegate(PlaylistDTO playlist);
    public delegate Task<List<FullTrack>> GetTracksAsyncDelegate(Paging<PlaylistTrack<IPlayableItem>> paging);
    public delegate void UpdateProgressBarDelegate(ushort progress, string message);

    public class DataFetch(GetUserProfileDelegate getUserProfile,
            GetUsersPublicPlaylistsDelegate getUsersPublicPlaylists,
            GetPlaylistAsyncDelegate getPlaylistAsync,
            GetTracksAsyncDelegate getTracksAsync,
            UpdateProgressBarDelegate updateProgressBar = null) {
        private const int maxDegreeOfParallelism = 3; // Adjust based on Spotify API capacity

        private readonly GetUserProfileDelegate getUserProfileAsync = getUserProfile;
        private readonly GetUsersPublicPlaylistsDelegate getUsersPublicPlaylistsAsync = getUsersPublicPlaylists;
        private readonly GetPlaylistAsyncDelegate getPlaylistAsync = getPlaylistAsync;
        private readonly GetTracksAsyncDelegate getTracksAsync = getTracksAsync;
        private readonly UpdateProgressBarDelegate updateProgressBar = updateProgressBar;


        public async Task GetData(string userID) {
            updateProgressBar?.Invoke(5, "Getting user's playlists");
            var allUserPlaylists = await getUsersPublicPlaylistsAsync(userID);
            var snapshotIDs = allUserPlaylists.ToDictionary(p => p.ID, p => p.SnapshotID);

            updateProgressBar?.Invoke(10, "Getting user's details");
            using var db = new SpotifyContext();
            UserDTO user = await GetOrAddUser(db, userID);
            updateProgressBar?.Invoke(20, "Processing playlists");
            await ProcessPlaylists(db, user, allUserPlaylists);

            updateProgressBar?.Invoke(30, "Getting tracks");
            var playlistsToUpdate = user.Playlists.Where(p => snapshotIDs[p.ID] != p.SnapshotID);
            var getPlaylistsAndTracksTask = GetMultiplePlaylistsTracksAsync(playlistsToUpdate);
            string[] playlistsToUpdateIds = playlistsToUpdate.Select(p => p.ID).ToArray();

            DTOAggregate dtoAggregate = new() {
                User = user,
                Playlists = await db.Playlists.Include(p => p.Tracks).Where(p => playlistsToUpdateIds.Contains(p.ID)).ToDictionaryAsync(t => t.ID, t => t),
                Tracks = await db.Tracks.ToDictionaryAsync(t => t.ID, t => t),
                Albums = await db.Albums.ToDictionaryAsync(t => t.ID, t => t),
                Artists = await db.Artists.ToDictionaryAsync(t => t.ID, t => t)
            };

            updateProgressBar?.Invoke(40, "Processing tracks");
            ProcessTracks(dtoAggregate, await getPlaylistsAndTracksTask);
            updateProgressBar?.Invoke(95, "Saving results");
            await db.SaveChangesAsync();
            updateProgressBar?.Invoke(0, null);
        }

        private async Task<UserDTO> GetOrAddUser(SpotifyContext db, string userID) {
            UserDTO user = await db.Users.Include(u => u.Playlists).FirstOrDefaultAsync(u => u.ID == userID);
            if (user is null) {
                user = await getUserProfileAsync(userID);
                await db.AddAsync(user);
                await db.SaveChangesAsync();
            }
            return user;
        }

        private static async Task ProcessPlaylists(SpotifyContext db, UserDTO user, IList<PlaylistDTO> allUserPlaylists) {
            var newPlaylists = db.Playlists.FindNewEntities(allUserPlaylists, p => p.ID);
            foreach (var playlist in newPlaylists) playlist.SnapshotID = ""; // Don't save the snapshotID so that it gets eligible for an update later
            user.Playlists.AddRange(newPlaylists);
            await db.SaveChangesAsync();

            var stalePlaylists = user.Playlists.Where(p => !allUserPlaylists.Any(aup => aup.ID == p.ID)).ToList();
            if (stalePlaylists.Count != 0) {
                db.RemoveRange(stalePlaylists);
                await db.SaveChangesAsync();
            }
        }

        class FullPlaylistAndTracks {
            public FullPlaylist Playlist;
            public IList<FullTrack> Tracks = [];
        }

        /**
		 * Get full details of the items of multiple playlists with given IDs.
		 * https://developer.spotify.com/documentation/web-api/reference/get-playlists-tracks
		 */
        private async Task<List<FullPlaylistAndTracks>> GetMultiplePlaylistsTracksAsync(IEnumerable<PlaylistDTO> playlistsIds) {
            // TODO cancellation token?
            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            var tasks = new List<Task<FullPlaylistAndTracks>>();

            // Create tasks to process each playlist ID asynchronously
            foreach (PlaylistDTO playlist in playlistsIds) {
                async Task<FullPlaylistAndTracks> GetTracks() {
                    try {
                        var data = new FullPlaylistAndTracks() {
                            Playlist = await getPlaylistAsync(playlist)
                        };
                        if (data.Playlist.SnapshotId != playlist.SnapshotID)
                            data.Tracks = await getTracksAsync(data.Playlist.Tracks);
                        return data;
                    }
                    finally {
                        semaphore.Release(); // Release the semaphore slot when done
                    }
                }

                await semaphore.WaitAsync(); // Wait for a semaphore slot to limit concurrency
                tasks.Add(Task.Run(GetTracks));
            }
            await Task.WhenAll(tasks);
            return tasks.Select(t => t.Result).ToList();
        }

        private class DTOAggregate {
            public UserDTO User;
            public Dictionary<string, PlaylistDTO> Playlists;
            public Dictionary<string, TrackDTO> Tracks;
            public Dictionary<string, AlbumDTO> Albums;
            public Dictionary<string, ArtistDTO> Artists;
        }

        private static void ProcessTracks(DTOAggregate dtos, List<FullPlaylistAndTracks> playlistsAndTracks) {
            foreach (var data in playlistsAndTracks) {
                dtos.Playlists.UpdateOrAdd(data.Playlist, out PlaylistDTO playlist);

                if (!dtos.User.Playlists.Any(p => p.ID == playlist.ID))
                    dtos.User.Playlists.Add(playlist);

                foreach (var fullTrack in data.Tracks) {
                    foreach (var simpleArtist in fullTrack.Artists)
                        dtos.Artists.UpdateOrAdd(simpleArtist, out _);

                    foreach (var simpleArtist in fullTrack.Album.Artists)
                        dtos.Artists.UpdateOrAdd(simpleArtist, out _);

                    if (!dtos.Albums.UpdateOrAdd(fullTrack.Album, out AlbumDTO album)) {
                        var artistIds = fullTrack.Album.Artists.Select(a => a.Id);
                        album.Artists = dtos.Artists.Where(a => artistIds.Contains(a.Key)).Select(p => p.Value).ToList();
                    }

                    if (!dtos.Tracks.UpdateOrAdd(fullTrack, out TrackDTO track)) {
                        track.Album = album;
                        var artistIds = fullTrack.Artists.Select(a => a.Id);
                        track.Artists = dtos.Artists.Where(a => artistIds.Contains(a.Key)).Select(p => p.Value).ToList();
                    }

                    if (!playlist.Tracks.Any(t => t.ID == track.ID))
                        playlist.Tracks.Add(track);
                }
            }
        }
    }


    public class DataFetchBuilder {
        GetUserProfileDelegate getUserProfile;
        GetUsersPublicPlaylistsDelegate getUsersPublicPlaylistsAsync;
        GetPlaylistAsyncDelegate getPlaylistAsync;
        GetTracksAsyncDelegate getTracksAsync;
        UpdateProgressBarDelegate updateProgressBar;

        public DataFetchBuilder SetUpdateProgressBar(UpdateProgressBarDelegate updateProgressBar) {
            this.updateProgressBar = updateProgressBar;
            return this;
        }

        public DataFetchBuilder SetGetUserProfile(GetUserProfileDelegate getUserProfile) {
            this.getUserProfile = getUserProfile;
            return this;
        }

        public DataFetchBuilder SetGetUsersPublicPlaylistsAsync(GetUsersPublicPlaylistsDelegate getUsersPublicPlaylistsAsync) {
            this.getUsersPublicPlaylistsAsync = getUsersPublicPlaylistsAsync;
            return this;
        }

        public DataFetchBuilder SetGetPlaylistAsync(GetPlaylistAsyncDelegate getPlaylistAsync) {
            this.getPlaylistAsync = getPlaylistAsync;
            return this;
        }

        public DataFetchBuilder SetGetTracksAsync(GetTracksAsyncDelegate getTracksAsync) {
            this.getTracksAsync = getTracksAsync;
            return this;
        }

        public DataFetch Build() {
            if (getUserProfile == null || getUsersPublicPlaylistsAsync == null || getPlaylistAsync == null || getTracksAsync == null)
                throw new InvalidOperationException("All dependencies must be provided");
            return new DataFetch(getUserProfile, getUsersPublicPlaylistsAsync, getPlaylistAsync, getTracksAsync, updateProgressBar);
        }
    }
}
