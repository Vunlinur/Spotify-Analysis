using Microsoft.EntityFrameworkCore;
using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
using SpotifyAPI.Web;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;


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
                await AddNewPlaylistsAsync(db, user, allUserPlaylists);
                await RemoveOldPlaylistsAsync(db, user, allUserPlaylists);
                await db.SaveChangesAsync();

                var playlistsToUpdate = user.Playlists.Where(p => snapshotIDs[p.ID] != p.SnapshotID);
                var dtoAggregate = await DTOAggregate.AggregateAsync(db, playlistsToUpdate, user);
                updateProgressBar?.Invoke(20, "Processing tracks");
                await ProcessPlaylistDataTreesAsync(playlistsToUpdate, dtoAggregate);

                updateProgressBar?.Invoke(60, "Processing artists");
                await GetNewArtistsAsync(db, dtoAggregate);

                updateProgressBar?.Invoke(90, "Saving data");
                await db.SaveChangesAsync();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }

        #region USER

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
            return user;
        }

        #endregion USER

        #region PLAYLISTS

        /**
         * Assigns new Playlists to the User and readies them for Track update later.
         */
        private static async Task AddNewPlaylistsAsync(SpotifyContext db, UserDTO user, IList<PlaylistDTO> allUserPlaylists) {
            var newPlaylists = db.Playlists.FindNewEntities(allUserPlaylists, p => p.ID);
            foreach (var playlist in newPlaylists) playlist.SnapshotID = ""; // Don't save the snapshotID so that it gets eligible for an update later
            user.Playlists.AddRange(newPlaylists);
        }

        /**
         * Removes orphaned Playlists.
         */
        private static async Task RemoveOldPlaylistsAsync(SpotifyContext db, UserDTO user, IList<PlaylistDTO> allUserPlaylists) {
            // TODO playlist can be referenced by other users, check if the playlist has no other users first
            // Remove orphan playlists from db
            var stalePlaylists = user.Playlists.Where(p => !allUserPlaylists.Any(aup => aup.ID == p.ID)).ToList();
            if (stalePlaylists.Count == 0)
                return;
            
            db.RemoveRange(stalePlaylists);
        }

        #endregion PLAYLISTS

        #region DATA TREE

        private async Task ProcessPlaylistDataTreesAsync(IEnumerable<PlaylistDTO> playlistsToUpdate, DTOAggregate dtoAggregate) {
            float progressBase = 20, progressDelta = (60 - progressBase) / playlistsToUpdate.Count();
            var tasks = playlistsToUpdate.Select(playlist =>
                Task.Run(() => GetAndProcessPlaylistDataTreeAsync(playlist, dtoAggregate))
                .ContinueWith(_ => updateProgressBar?.Invoke(progressBase += progressDelta, null))
            );
            await Task.WhenAll(tasks);
        }

        /**
		 * Get full details of the items of multiple playlists with given IDs.
		 * https://developer.spotify.com/documentation/web-api/reference/get-playlists-tracks
		 */
        private async Task GetAndProcessPlaylistDataTreeAsync(PlaylistDTO playlist, DTOAggregate dtoAggregate) {
            FullPlaylist fullPlaylist = await getPlaylistAsync(playlist.ID);
            List<FullTrack> fullTracks = [];
            if (fullPlaylist.SnapshotId != playlist.SnapshotID)
                fullTracks = await getTracksAsync(fullPlaylist.Tracks);
            ProcessDataTree(fullPlaylist, fullTracks, dtoAggregate);
        }

        /**
         * <summary>
         * Processes a hierarchical "data tree" of Spotify data from leaves to the root, adding
         * Playlists and Artists -> Albums -> Tracks to the DTOAggregate structure while maintaining dependencies.
         * </summary>
         * 
         * <param name="fullPlaylist">The root playlist containing metadata and track references.</param>
         * <param name="fullTracks">Tracks within the playlist, each referencing album and artists.</param>
         * <param name="dtos">An in-memory repository for deduplicating and linking playlists, tracks, albums, and artists.</param>
         * 
         * <remarks>
         * Entities are processed in dependency order:
         * 1. **Playlist**: Added to the User.
         * 2. **Artists**: Added first to allow albums and tracks to reference them correctly later.
         * 3. **Albums**: Added next, linked to artists.
         * 4. **Tracks**: Added next, referencing albums and artists, then linked to the playlist.
         * This ensures proper relationships between entities and avoids dependency issues during processing.
         * DTOAggregate helps with de-duplication.
         * </remarks>
         */
        private static void ProcessDataTree(FullPlaylist fullPlaylist, List<FullTrack> fullTracks, DTOAggregate dtos) {
            if (dtos.GetOrAddPlaylist(fullPlaylist, out PlaylistDTO playlist))
                playlist.Update(fullPlaylist);

            AddPlaylistToUser(dtos.User, playlist);

            foreach (var fullTrack in fullTracks) {
                UpdateOrAddArtists(fullTrack.Artists, dtos);
                UpdateOrAddArtists(fullTrack.Album.Artists, dtos);

                if (dtos.GetOrAddAlbum(fullTrack.Album, out AlbumDTO album))
                    album.Update(fullTrack.Album);
                else
                    album.Artists = dtos.GetArtists(fullTrack.Album.Artists);

                if (dtos.GetOrAddTrack(fullTrack, out TrackDTO track))
                    track.Update(fullTrack);
                else {
                    track.Album = album;
                    track.Artists = dtos.GetArtists(fullTrack.Artists);
                }

                AddTrackToPlaylist(track, playlist);
            }
        }

        public static void AddPlaylistToUser(UserDTO user, PlaylistDTO playlist) {
            if (!user.Playlists.Any(p => p.ID == playlist.ID))
                user.Playlists.Add(playlist);
        }

        private static void UpdateOrAddArtists(List<SimpleArtist> artists, DTOAggregate dtos) {
            foreach (var artist in artists ?? [])
                if (dtos.GetOrAddArtist(artist, out ArtistDTO artistDTO))
                    artistDTO.Update(artist);
        }

        public static void AddTrackToPlaylist(TrackDTO track, PlaylistDTO playlist) {
            if (!playlist.Tracks.Any(t => t.ID == track.ID))
                playlist.Tracks.Add(track);
        }

        #endregion DATA TREE

        #region ARTISTS

        private async Task GetNewArtistsAsync(SpotifyContext db, DTOAggregate dtoAggregate) {
            var newArtistsIds = db.Artists
                .FindNewEntities(dtoAggregate.Artists.Values, p => p.ID)
                .Select(a => a.ID)
                .ToList();

            var chunks = DivideArtistsRequests(newArtistsIds);
            float progressBase = 60, progressDelta = (90 - progressBase) / chunks.Count();
            var tasks = chunks.Select(chunk =>
                Task.Run(
                    () => getArtistsAsync(chunk)
                    .ContinueWith(async fullArtists => UpdateOrAddArtists(await fullArtists, dtoAggregate))
                    .ContinueWith(_ => updateProgressBar?.Invoke(progressBase += progressDelta, null))
            ));

            await Task.WhenAll(tasks);
        }

        private static IEnumerable<List<string>> DivideArtistsRequests(List<string> newArtistsIds) {
            ushort chunkSize = 50;  // TODO replace with something that doesn't create empty partitions
            return newArtistsIds.Select((s, i) => newArtistsIds.Skip(i * chunkSize).Take(chunkSize).ToList()).Where(a => a.Count != 0);
        }

        private static void UpdateOrAddArtists(List<FullArtist> fullArtists, DTOAggregate dtos) {
            foreach (var artist in fullArtists)
                if (dtos.GetOrAddArtist(artist, out ArtistDTO artistDTO))
                    artistDTO.Update(artist);
        }

        #endregion ARTISTS
    }
}
