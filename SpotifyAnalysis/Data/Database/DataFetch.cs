using Microsoft.EntityFrameworkCore;
using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
using SpotifyAPI.Web;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;


namespace SpotifyAnalysis.Data.Database {
    public delegate Task<PublicUser> GetUserProfileDelegate(string userID);
    public delegate Task<IList<FullPlaylist>> GetUsersPublicPlaylistsDelegate(string userID);
    public delegate Task<FullPlaylist> GetPlaylistAsyncDelegate(string playlistId);
    public delegate Task<List<FullTrack>> GetTracksAsyncDelegate(Paging<PlaylistTrack<IPlayableItem>> paging);
    public delegate Task<List<FullArtist>> GetArtistsAsyncDelegate(IList<string> ids);
    public delegate void UpdateProgressBarDelegate(float progress, string message);

    public class DataFetch(GetUserProfileDelegate getUserProfile,
            GetUsersPublicPlaylistsDelegate getUsersPublicPlaylists,
            GetPlaylistAsyncDelegate getPlaylistAsync,
            GetTracksAsyncDelegate getTracksAsync,
            GetArtistsAsyncDelegate getArtistsAsync,
            UpdateProgressBarDelegate updateProgressBar = null) {
        readonly GetUserProfileDelegate getUserProfileAsync = getUserProfile;
        readonly GetUsersPublicPlaylistsDelegate getUsersPublicPlaylistsAsync = getUsersPublicPlaylists;
        readonly GetPlaylistAsyncDelegate getPlaylistAsync = getPlaylistAsync;
        readonly GetTracksAsyncDelegate getTracksAsync = getTracksAsync;
        readonly GetArtistsAsyncDelegate getArtistsAsync = getArtistsAsync;
        readonly UpdateProgressBarDelegate updateProgressBar = updateProgressBar;
        
        /**
         * Fetches User data from Spotify API and inserts to the DB.
         * User Data include:
         * User's profile, User's playlists, playlists' tracks, tracks' artists
         */
        public async Task GetData(string userID) {
            try {
                // TODO optimize await order
                updateProgressBar?.Invoke(5, "Processing user data");
                var allUserPlaylistsTask = getUsersPublicPlaylistsAsync(userID);
                using var db = new SpotifyContext();
                UserDTO user = await GetOrAddUserAsync(db, userID);
                var allUserPlaylists = (await allUserPlaylistsTask).ToPlaylistDTOs();
                var snapshotIDs = allUserPlaylists.ToDictionary(p => p.ID, p => p.SnapshotID);

                updateProgressBar?.Invoke(10, "Processing playlists");
                await ProcessPlaylistsAsync(db, user, allUserPlaylists);
                var playlistsToUpdate = user.Playlists.Where(p => snapshotIDs[p.ID] != p.SnapshotID);

                // Create tasks to process each playlist ID asynchronously
                var dtoAggregate = await AggregateDTOsAsync(db, playlistsToUpdate, user);
                updateProgressBar?.Invoke(20, "Processing tracks");
                await ProcessPlaylistsTracksAsync(playlistsToUpdate, dtoAggregate);

                updateProgressBar?.Invoke(60, "Processing artists");
                await ProcessArtistsAsync(db, dtoAggregate);

                updateProgressBar?.Invoke(90, "Saving data");
                await db.SaveChangesAsync();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }

        /**
         * Gets a User from the DB by ID, or creates a new one and inserts.
         */
        private async Task<UserDTO> GetOrAddUserAsync(SpotifyContext db, string userID) {
            UserDTO user = await db.Users.Include(u => u.Playlists).FirstOrDefaultAsync(u => u.ID == userID);
            if (user is null) {
                user = (await getUserProfileAsync(userID)).ToUserDTO();
                await db.AddAsync(user);
            }
            user.Updated = DateTime.Now;
            await db.SaveChangesAsync();
            return user;
        }

        /**
         * Assigns new Playlists to the User and readies them for Track update later. Removes orphaned Playlists.
         */
        private static async Task ProcessPlaylistsAsync(SpotifyContext db, UserDTO user, IList<PlaylistDTO> allUserPlaylists) {
            var newPlaylists = db.Playlists.FindNewEntities(allUserPlaylists, p => p.ID);
            foreach (var playlist in newPlaylists) playlist.SnapshotID = ""; // Don't save the snapshotID so that it gets eligible for an update later
            user.Playlists.AddRange(newPlaylists);
            await db.SaveChangesAsync();

            // TODO playlist can be referenced by other users, check if the playlist has no other users first
            // Remove orphan playlists from db
            var stalePlaylists = user.Playlists.Where(p => !allUserPlaylists.Any(aup => aup.ID == p.ID)).ToList();
            if (stalePlaylists.Count > 0) {
                db.RemoveRange(stalePlaylists);
                await db.SaveChangesAsync();
            }
        }

        private async Task ProcessPlaylistsTracksAsync(IEnumerable<PlaylistDTO> playlistsToUpdate, DTOAggregate dtoAggregate) {
            float progressBase = 20, progressDelta = (60 - progressBase) / playlistsToUpdate.Count();
            var tasks = playlistsToUpdate.Select(playlist =>
                Task.Run(() => GetAndProcessPlaylistDataAsync(playlist, dtoAggregate))
                .ContinueWith(_ => updateProgressBar?.Invoke(progressBase += progressDelta, null))
            );
            await Task.WhenAll(tasks);
        }

        private async Task ProcessArtistsAsync(SpotifyContext db, DTOAggregate dtoAggregate) {
            var newArtistsIds = db.Artists
                .FindNewEntities(dtoAggregate.Artists.Values, p => p.ID)
                .Select(a => a.ID)
                .ToList();

            var chunks = DivideArtistsRequests(newArtistsIds);
            float progressBase = 60, progressDelta = (90 - progressBase) / chunks.Count();
            var tasks = chunks.Select(chunk =>
                Task.Run(() => GetAndProcessArtistsAsync(chunk, dtoAggregate))
                .ContinueWith(_ => updateProgressBar?.Invoke(progressBase += progressDelta, null))
            );

            await Task.WhenAll(tasks);
        }

        private class DTOAggregate {
            public UserDTO User;
            public ConcurrentDictionary<string, PlaylistDTO> Playlists;
            public ConcurrentDictionary<string, TrackDTO> Tracks;
            public ConcurrentDictionary<string, AlbumDTO> Albums;
            public ConcurrentDictionary<string, ArtistDTO> Artists;
        }

        /**
         * Get all relevant data from the DB
         */
        private async Task<DTOAggregate> AggregateDTOsAsync(SpotifyContext db, IEnumerable<PlaylistDTO> playlistsIds, UserDTO user) {
            string[] playlistsToUpdateIds = playlistsIds.Select(p => p.ID).ToArray();
            return new DTOAggregate() {
                User = user,
                Playlists = new ConcurrentDictionary<string, PlaylistDTO>(await db.Playlists.Include(p => p.Tracks).Where(p => playlistsToUpdateIds.Contains(p.ID)).ToDictionaryAsync(t => t.ID, t => t)),
                Tracks = new ConcurrentDictionary<string, TrackDTO>(await db.Tracks.ToDictionaryAsync(t => t.ID, t => t)),
                Albums = new ConcurrentDictionary<string, AlbumDTO>(await db.Albums.ToDictionaryAsync(t => t.ID, t => t)),
                Artists = new ConcurrentDictionary<string, ArtistDTO>(await db.Artists.ToDictionaryAsync(t => t.ID, t => t))
            };
        }

        /**
		 * Get full details of the items of multiple playlists with given IDs.
		 * https://developer.spotify.com/documentation/web-api/reference/get-playlists-tracks
		 */
        private async Task GetAndProcessPlaylistDataAsync(PlaylistDTO playlist, DTOAggregate dtoAggregate) {
            FullPlaylist fullPlaylist = await getPlaylistAsync(playlist.ID);
            List<FullTrack> fullTracks = [];
            if (fullPlaylist.SnapshotId != playlist.SnapshotID)
                fullTracks = await getTracksAsync(fullPlaylist.Tracks);
            ProcessTracks(fullPlaylist, fullTracks, dtoAggregate);
        }

        private async Task GetAndProcessArtistsAsync(List<string> ids, DTOAggregate dtoAggregate) {
            var fullArtists = await getArtistsAsync(ids);
            foreach (var artist in fullArtists)
                dtoAggregate.Artists[artist.Id].Update(artist);
        }

        private static void ProcessTracks(FullPlaylist fullPlaylist, List<FullTrack> fullTracks, DTOAggregate dtos) {
            dtos.Playlists.UpdateOrAdd(fullPlaylist, out PlaylistDTO playlist);

            if (!dtos.User.Playlists.Any(p => p.ID == playlist.ID)) {
                dtos.User.Playlists.Add(playlist);
            }

            foreach (var fullTrack in fullTracks) {
                ProcessTrackArtists(fullTrack, dtos);
                ProcessTrackAlbum(fullTrack, dtos, out AlbumDTO album);
                ProcessSingleTrack(fullTrack, playlist, dtos, album);
            }
        }

        private static void ProcessTrackArtists(FullTrack fullTrack, DTOAggregate dtos) {
            foreach (var simpleArtist in fullTrack.Artists)
                dtos.Artists.UpdateOrAdd(simpleArtist, out _);

                foreach (var simpleArtist in fullTrack.Album.Artists) // TODO Artists here sometimes happen to be null!
                dtos.Artists.UpdateOrAdd(simpleArtist, out _);
        }

        private static void ProcessTrackAlbum(FullTrack fullTrack, DTOAggregate dtos, out AlbumDTO album) {
            if (!dtos.Albums.UpdateOrAdd(fullTrack.Album, out album)) {
                var artistIds = fullTrack.Album.Artists.Select(a => a.Id);
                album.Artists = dtos.Artists
                    .Where(a => artistIds.Contains(a.Key))
                    .Select(a => a.Value)
                    .ToList();
            }
        }

        private static void ProcessSingleTrack(FullTrack fullTrack, PlaylistDTO playlist, DTOAggregate dtos, AlbumDTO album) {
            if (!dtos.Tracks.UpdateOrAdd(fullTrack, out TrackDTO track)) {
                track.Album = album;
                var artistIds = fullTrack.Artists.Select(a => a.Id);
                track.Artists = dtos.Artists
                    .Where(a => artistIds.Contains(a.Key))
                    .Select(a => a.Value)
                    .ToList();
            }
            
            // TODO remove tracks which have been removed
            if (!playlist.Tracks.Any(t => t.ID == track.ID))
                playlist.Tracks.Add(track);
        }

        private static IEnumerable<List<string>> DivideArtistsRequests(List<string> newArtistsIds) {
            ushort chunkSize = 50;  // TODO replace with something that doesn't create empty partitions
            return newArtistsIds.Select((s, i) => newArtistsIds.Skip(i * chunkSize).Take(chunkSize).ToList()).Where(a => a.Count != 0);
        }
    }
}
