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
    public class DataFetch {
        readonly Action<int, string> updateProgressBar;
        readonly Func<string, Task<UserDTO>> getUserProfile;
        readonly Func<string, Task<IList<PlaylistDTO>>> GetUsersPublicPlaylistsAsync;
        readonly Func<IEnumerable<PlaylistDTO>, Task<List<FullPlaylistAndTracks>>> GetMultiplePlaylistsTracksAsync;

        /*
        *  Get UserDTO from DB, create new from API if not present
        */
        private async Task<UserDTO> GetOrAddUser(SpotifyContext db, string userID) {
            UserDTO user = await db.Users
            .Include(u => u.Playlists)
                    .FirstOrDefaultAsync(u => u.ID == userID);
            if (user is null) {
                user = await getUserProfile(userID);
                await db.AddAsync(user);
                await db.SaveChangesAsync();
            }
            return user;
        }

        public async void GetData(string userID) {
            updateProgressBar(5, "Getting user's playlists");
            // TODO optimize await order
            var allUserPlaylists = await GetUsersPublicPlaylistsAsync(userID);
            var snapshotIDs = allUserPlaylists.ToDictionary(p => p.ID, p => p.SnapshotID);

            updateProgressBar(10, "Getting user's details");
            UserDTO user;
            using (var db = new SpotifyContext()) {
                user = await GetOrAddUser(db, userID);

                updateProgressBar(20, "Processing playlists");
                // Add new playlists
                var newPlaylists = db.Playlists.FindNewEntities(allUserPlaylists, p => p.ID);
                foreach (var playlist in newPlaylists) playlist.SnapshotID = ""; // Don't save the snapshodID so that it gets eligible for an update later
                user.Playlists.AddRange(newPlaylists);
                await db.SaveChangesAsync();

                // TODO playlist can be referenced by other users, check if the playlist has no other users first
                // Remove the playlist we don't have anymore
                var stalePlaylists = user.Playlists.Where(p => !allUserPlaylists.Any(aup => aup.ID == p.ID));
                if (stalePlaylists.Any())
                    db.RemoveRange(stalePlaylists);

                await db.SaveChangesAsync();
            }

            updateProgressBar(30, "Getting tracks");
            var playlistsToUpdate = user.Playlists.Where(p => snapshotIDs[p.ID] != p.SnapshotID);
            var getPlaylistsAndTracksTask = GetMultiplePlaylistsTracksAsync(playlistsToUpdate);
            string[] selectedPlaylistsIds = playlistsToUpdate.Select(p => p.ID).ToArray();

            using (var db = new SpotifyContext()) {
                var playlists = await db.Playlists
                    .Include(p => p.Tracks)
                    .Where(p => selectedPlaylistsIds.Contains(p.ID))
                    .ToDictionaryAsync(t => t.ID, t => t);
                var tracks = await db.Tracks.ToDictionaryAsync(t => t.ID, t => t);
                var albums = await db.Albums.ToDictionaryAsync(t => t.ID, t => t);
                var artists = await db.Artists.ToDictionaryAsync(t => t.ID, t => t);

                updateProgressBar(40, "Processing tracks");
                foreach (var data in await getPlaylistsAndTracksTask) {
                    playlists.UpdateOrAdd(data.Playlist, out PlaylistDTO playlist);
                    // Attach the playlist to the user if not done previously. It might have been in the db from another user.
                    if (!user.Playlists.Any(p => p.ID == playlist.ID))
                        user.Playlists.Add(playlist);

                    foreach (var fullTrack in data.Tracks) {
                        foreach (var simpleArtist in fullTrack.Artists)
                            artists.UpdateOrAdd(simpleArtist, out _);

                        foreach (var simpleArtist in fullTrack.Album.Artists)
                            artists.UpdateOrAdd(simpleArtist, out _);

                        if (!albums.UpdateOrAdd(fullTrack.Album, out AlbumDTO album)) {
                            var artistIds = fullTrack.Album.Artists.Select(a => a.Id);
                            album.Artists = artists.Where(a => artistIds.Contains(a.Key)).Select(p => p.Value).ToList();
                        }

                        if (!tracks.UpdateOrAdd(fullTrack, out TrackDTO track)) {
                            track.Album = album;
                            var artistIds = fullTrack.Artists.Select(a => a.Id);
                            track.Artists = artists.Where(a => artistIds.Contains(a.Key)).Select(p => p.Value).ToList();
                        }

                        // TODO remove old tracks
                        if (!playlist.Tracks.Any(t => t.ID == track.ID))
                            playlist.Tracks.Add(track);
                    }
                }
                updateProgressBar(95, "Saving results");
                await db.SaveChangesAsync();
            }
            updateProgressBar(0, null);
        }
    }
}
