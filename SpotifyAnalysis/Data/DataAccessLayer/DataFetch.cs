﻿using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotifyAnalysis.Data;
using SpotifyAPI.Web;
using System.Threading;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;


namespace SpotifyAnalysis.Data.DataAccessLayer {
    // TODO change the delegates to NOT depend on DTOs
    public delegate Task<UserDTO> GetUserProfileDelegate(string userID);
    public delegate Task<IList<PlaylistDTO>> GetUsersPublicPlaylistsDelegate(string userID);
    public delegate Task<FullPlaylist> GetPlaylistAsyncDelegate(PlaylistDTO playlist);
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

        public async Task GetData(string userID) {
            // TODO optimize await order
            updateProgressBar?.Invoke(5, "Processing user data");
            var allUserPlaylists = await getUsersPublicPlaylistsAsync(userID);
            var snapshotIDs = allUserPlaylists.ToDictionary(p => p.ID, p => p.SnapshotID);
            using var db = new SpotifyContext();
            UserDTO user = await GetOrAddUser(db, userID);

            updateProgressBar?.Invoke(10, "Processing playlists");
            await ProcessPlaylists(db, user, allUserPlaylists);
            var playlistsToUpdate = user.Playlists.Where(p => snapshotIDs[p.ID] != p.SnapshotID);
            var dtoAggregate = await AggregateDTOs(db, playlistsToUpdate, user);

            // TODO cancellation token?
            // Create tasks to process each playlist ID asynchronously
            updateProgressBar?.Invoke(20, "Processing tracks");
            float progressBase = 20, progressDelta = (60 - progressBase) / playlistsToUpdate.Count();
            List<Task> tasks = [];
            foreach (PlaylistDTO playlist in playlistsToUpdate)
                tasks.Add(
                    Task.Run(() => GetAndProcessPlaylistData(playlist, dtoAggregate))
                    .ContinueWith(t => updateProgressBar?.Invoke(progressBase += progressDelta, null))
                );
            await Task.WhenAll(tasks);

            updateProgressBar?.Invoke(60, "Processing artists");
            var newArtistsIds = db.Artists.FindNewEntities(dtoAggregate.Artists.Values, p => p.ID).Select(a => a.ID).ToList();
            await GetAndProcessArtists(newArtistsIds, dtoAggregate);

            updateProgressBar?.Invoke(95, "Saving data");
            await db.SaveChangesAsync();

            updateProgressBar?.Invoke(100, "Finished!");
            await Task.Delay(2000);
            updateProgressBar?.Invoke(0, null);
        }

        /**
         * Gets a User from the DB by ID, or creates a new one and inserts.
         */
        private async Task<UserDTO> GetOrAddUser(SpotifyContext db, string userID) {
            UserDTO user = await db.Users.Include(u => u.Playlists).FirstOrDefaultAsync(u => u.ID == userID);
            if (user is null) {
                user = await getUserProfileAsync(userID);
                await db.AddAsync(user);
                await db.SaveChangesAsync();
            }
            return user;
        }

        /**
         * Assigns new Playlists to the User and readies them for Track update later. Removes orphaned Playlists.
         */
        private static async Task ProcessPlaylists(SpotifyContext db, UserDTO user, IList<PlaylistDTO> allUserPlaylists) {
            var newPlaylists = db.Playlists.FindNewEntities(allUserPlaylists, p => p.ID);
            foreach (var playlist in newPlaylists) playlist.SnapshotID = ""; // Don't save the snapshotID so that it gets eligible for an update later
            user.Playlists.AddRange(newPlaylists);
            await db.SaveChangesAsync();

            // TODO playlist can be referenced by other users, check if the playlist has no other users first
            // Remove orphan playlists from db
            var stalePlaylists = user.Playlists.Where(p => !allUserPlaylists.Any(aup => aup.ID == p.ID)).ToList();
            if (stalePlaylists.Count != 0) {
                db.RemoveRange(stalePlaylists);
                await db.SaveChangesAsync();
            }
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
        private async Task<DTOAggregate> AggregateDTOs(SpotifyContext db, IEnumerable<PlaylistDTO> playlistsIds, UserDTO user) {
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
        private async Task GetAndProcessPlaylistData(PlaylistDTO playlist, DTOAggregate dtoAggregate) {
            FullPlaylist fullPlaylist = await getPlaylistAsync(playlist);
            List<FullTrack> fullTracks = [];
            if (fullPlaylist.SnapshotId != playlist.SnapshotID)
                fullTracks = await getTracksAsync(fullPlaylist.Tracks);
            ProcessTracks(fullPlaylist, fullTracks, dtoAggregate);
        }

        /**
		 * Get full details of the items of multiple playlists with given IDs.
		 * https://developer.spotify.com/documentation/web-api/reference/get-playlists-tracks
		 */
        private async Task GetAndProcessArtists(List<string> newArtistsIds, DTOAggregate dtoAggregate) {
            if (newArtistsIds.Count == 0)
                return;

            IEnumerable<List<string>> chunks = DivideArtistsRequests(newArtistsIds);
            float progressBase = 60, progressDelta = (95 - progressBase) / chunks.Count(); // TODO replace with something that doesn't create empty partitions

            foreach (var chunk in chunks) {
                updateProgressBar?.Invoke(progressBase += progressDelta, null);
                var artistsData = await getArtistsAsync(chunk);
                foreach (var artist in artistsData)
                    UpdateArtist(dtoAggregate.Artists[artist.Id], artist);
            }
        }

        private static IEnumerable<List<string>> DivideArtistsRequests(List<string> newArtistsIds) {
            ushort chunkSize = 50;
            return newArtistsIds.Select((s, i) => newArtistsIds.Skip(i * chunkSize).Take(chunkSize).ToList()).Where(a => a.Count != 0);
        }

        private static void ProcessTracks(FullPlaylist fullPlaylist, List<FullTrack> fullTracks, DTOAggregate dtos) {
            dtos.Playlists.UpdateOrAdd(fullPlaylist, out PlaylistDTO playlist);

            if (!dtos.User.Playlists.Any(p => p.ID == playlist.ID))
                dtos.User.Playlists.Add(playlist);

            foreach (var fullTrack in fullTracks) {
                foreach (var simpleArtist in fullTrack.Artists)
                    dtos.Artists.UpdateOrAdd(simpleArtist, out _);

                foreach (var simpleArtist in fullTrack.Album.Artists) // TODO Artists here sometimes happen to be null!
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

                // TODO remove tracks which have been removed
                if (!playlist.Tracks.Any(t => t.ID == track.ID))
                    playlist.Tracks.Add(track);
            }
        }

        // TODO move to extensions and extract more UpdateX from DbExtensions
        private static void UpdateArtist(ArtistDTO artist, FullArtist fullArtist) {
            artist.Genres = fullArtist.Genres;
            artist.Popularity = fullArtist.Popularity;
            artist.Images = fullArtist.Images.Select(i => i.ToImageDTO()).ToList();
        }
    }
}
