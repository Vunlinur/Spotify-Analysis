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
using static SpotifyAnalysis.Data.SpotifyAPI.SpotifyModule;


namespace SpotifyAnalysis.Data.DataAccessLayer {
    public delegate Task<UserDTO> GetUserProfileDelegate(string userID);
    public delegate Task<IList<PlaylistDTO>> GetUsersPublicPlaylistsDelegate(string userID);
    public delegate Task<List<FullPlaylistAndTracks>> GetMultiplePlaylistsTracksDelegate(IEnumerable<PlaylistDTO> playlists);
    public delegate void UpdateProgressBarDelegate(ushort progress, string message);

    public class DataFetch(UpdateProgressBarDelegate updateProgressBar,
            GetUserProfileDelegate getUserProfile,
            GetUsersPublicPlaylistsDelegate getUsersPublicPlaylists,
            GetMultiplePlaylistsTracksDelegate getMultiplePlaylistsTracks) {

        private readonly UpdateProgressBarDelegate updateProgressBar = updateProgressBar;
        private readonly GetUserProfileDelegate getUserProfileAsync = getUserProfile;
        private readonly GetUsersPublicPlaylistsDelegate getUsersPublicPlaylistsAsync = getUsersPublicPlaylists;
        private readonly GetMultiplePlaylistsTracksDelegate getMultiplePlaylistsTracksAsync = getMultiplePlaylistsTracks;


        public async Task GetData(string userID) {
            updateProgressBar(5, "Getting user's playlists");
            var allUserPlaylists = await getUsersPublicPlaylistsAsync(userID);
            var snapshotIDs = allUserPlaylists.ToDictionary(p => p.ID, p => p.SnapshotID);

            updateProgressBar(10, "Getting user's details");
            using var db = new SpotifyContext();
            var user = await GetOrAddUser(db, userID);

            updateProgressBar(20, "Processing playlists");
            await ProcessPlaylists(db, user, allUserPlaylists);

            updateProgressBar(30, "Getting tracks");
            var playlistsToUpdate = user.Playlists.Where(p => snapshotIDs[p.ID] != p.SnapshotID);
            var getPlaylistsAndTracksTask = getMultiplePlaylistsTracksAsync(playlistsToUpdate);
            string[] selectedPlaylistsIds = playlistsToUpdate.Select(p => p.ID).ToArray();

            var dtoAggregate = new DTOAggregate {
                Playlists = await db.Playlists.Include(p => p.Tracks).Where(p => selectedPlaylistsIds.Contains(p.ID)).ToDictionaryAsync(t => t.ID, t => t),
                Tracks = await db.Tracks.ToDictionaryAsync(t => t.ID, t => t),
                Albums = await db.Albums.ToDictionaryAsync(t => t.ID, t => t),
                Artists = await db.Artists.ToDictionaryAsync(t => t.ID, t => t)
            };

            updateProgressBar(40, "Processing tracks");
            ProcessTracks(user, dtoAggregate, await getPlaylistsAndTracksTask);

            updateProgressBar(95, "Saving results");
            await db.SaveChangesAsync();
            updateProgressBar(0, null);
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

        private static void ProcessTracks(UserDTO user, DTOAggregate dtos, List<FullPlaylistAndTracks> playlistsAndTracks) {
            foreach (var data in playlistsAndTracks) {
                dtos.Playlists.UpdateOrAdd(data.Playlist, out PlaylistDTO playlist);

                if (!user.Playlists.Any(p => p.ID == playlist.ID))
                    user.Playlists.Add(playlist);

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

    public class DTOAggregate {
        public Dictionary<string, PlaylistDTO> Playlists;
        public Dictionary<string, TrackDTO> Tracks;
        public Dictionary<string, AlbumDTO> Albums;
        public Dictionary<string, ArtistDTO> Artists;
    }


    public class DataFetchBuilder {
        UpdateProgressBarDelegate updateProgressBar;
        GetUserProfileDelegate getUserProfile;
        GetUsersPublicPlaylistsDelegate getUsersPublicPlaylistsAsync;
        GetMultiplePlaylistsTracksDelegate getMultiplePlaylistsTracksAsync;

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

        public DataFetchBuilder SetGetMultiplePlaylistsTracksAsync(GetMultiplePlaylistsTracksDelegate getMultiplePlaylistsTracksAsync) {
            this.getMultiplePlaylistsTracksAsync = getMultiplePlaylistsTracksAsync;
            return this;
        }

        public DataFetch Build() {
            if (getUserProfile == null || getUsersPublicPlaylistsAsync == null || getMultiplePlaylistsTracksAsync == null)
                throw new InvalidOperationException("All dependencies must be provided");
            return new DataFetch(updateProgressBar, getUserProfile, getUsersPublicPlaylistsAsync, getMultiplePlaylistsTracksAsync);
        }
    }
}
