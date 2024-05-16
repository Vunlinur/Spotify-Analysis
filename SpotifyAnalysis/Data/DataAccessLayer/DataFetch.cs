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
            await GetMultiplePlaylistsTracksAsync(db, playlistsToUpdate, user);
            
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
        private async Task GetMultiplePlaylistsTracksAsync(SpotifyContext db, IEnumerable<PlaylistDTO> playlistsIds, UserDTO user) {
            // TODO cancellation token?
            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            var dbContextSemaphore = new SemaphoreSlim(1, 1);
            var tasks = new List<Task>();

            string[] playlistsToUpdateIds = playlistsIds.Select(p => p.ID).ToArray();

            DTOAggregate dtoAggregate = new() {
                User = user,
                Playlists = new ConcurrentDictionary<string, PlaylistDTO>(await db.Playlists.Include(p => p.Tracks).Where(p => playlistsToUpdateIds.Contains(p.ID)).ToDictionaryAsync(t => t.ID, t => t)),
                Tracks = new ConcurrentDictionary<string, TrackDTO>(await db.Tracks.ToDictionaryAsync(t => t.ID, t => t)),
                Albums = new ConcurrentDictionary<string, AlbumDTO>(await db.Albums.ToDictionaryAsync(t => t.ID, t => t)),
                Artists = new ConcurrentDictionary<string, ArtistDTO>(await db.Artists.ToDictionaryAsync(t => t.ID, t => t))
            };

            // Create tasks to process each playlist ID asynchronously
            foreach (PlaylistDTO playlist in playlistsIds) {
                async Task GetTracks() {
                    try {
                        FullPlaylist fullPlaylist = await getPlaylistAsync(playlist);
                        List<FullTrack> fullTracks = [];
                        if (fullPlaylist.SnapshotId != playlist.SnapshotID)
                            fullTracks = await getTracksAsync(fullPlaylist.Tracks);

                        ProcessTracks(dtoAggregate, fullPlaylist, fullTracks);
                        updateProgressBar?.Invoke(95, "Saving results");
                        await dbContextSemaphore.WaitAsync();
                        await db.SaveChangesAsync();
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                    }
                    finally { // Release the semaphore slot when done
                        dbContextSemaphore.Release();
                        semaphore.Release();
                    }
                }

                await semaphore.WaitAsync(); // Wait for a semaphore slot to limit concurrency
                tasks.Add(Task.Run(GetTracks));
            }
            await Task.WhenAll(tasks);
        }

        private class DTOAggregate {
            public UserDTO User;
            public ConcurrentDictionary<string, PlaylistDTO> Playlists;
            public ConcurrentDictionary<string, TrackDTO> Tracks;
            public ConcurrentDictionary<string, AlbumDTO> Albums;
            public ConcurrentDictionary<string, ArtistDTO> Artists;
        }

        private static void ProcessTracks(DTOAggregate dtos, FullPlaylist fullPlaylist, List<FullTrack> fullTracks) {
            dtos.Playlists.UpdateOrAdd(fullPlaylist, out PlaylistDTO playlist);

            if (!dtos.User.Playlists.Any(p => p.ID == playlist.ID))
                dtos.User.Playlists.Add(playlist);

            foreach (var fullTrack in fullTracks) {
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
