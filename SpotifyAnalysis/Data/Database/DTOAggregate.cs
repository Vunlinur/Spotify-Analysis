using Microsoft.EntityFrameworkCore;
using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
using SpotifyAPI.Web;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data.Database {
    public class DTOAggregate {
        public UserDTO User { get; set; }
        public ConcurrentDictionary<string, PlaylistDTO> Playlists { get; private set; }
        public ConcurrentDictionary<string, TrackDTO> Tracks { get; private set; }
        public ConcurrentDictionary<string, AlbumDTO> Albums { get; private set; }
        public ConcurrentDictionary<string, ArtistDTO> Artists { get; private set; }

        private DTOAggregate() { }

        public static async Task<DTOAggregate> AggregateAsync(SpotifyContext db, IEnumerable<PlaylistDTO> playlistsIds, UserDTO user) {
            string[] playlistsToUpdateIds = playlistsIds.Select(p => p.ID).ToArray();
            return new DTOAggregate() {
                User = user,
                Playlists = new ConcurrentDictionary<string, PlaylistDTO>(await db.Playlists.Include(p => p.Tracks).Where(p => playlistsToUpdateIds.Contains(p.ID)).ToDictionaryAsync(t => t.ID, t => t)),
                Tracks = new ConcurrentDictionary<string, TrackDTO>(await db.Tracks.ToDictionaryAsync(t => t.ID, t => t)),
                Albums = new ConcurrentDictionary<string, AlbumDTO>(await db.Albums.ToDictionaryAsync(t => t.ID, t => t)),
                Artists = new ConcurrentDictionary<string, ArtistDTO>(await db.Artists.ToDictionaryAsync(t => t.ID, t => t))
            };
        }

        public bool UpdatePlaylist(FullPlaylist fullPlaylist, out PlaylistDTO entity) {
            bool found = false;
            entity = Playlists.AddOrUpdate(
                fullPlaylist.Id,
                id => {
                    // this shouldn't really happen as we should already have
                    // all the playlists in the db before adding their details
                    return fullPlaylist.ToPlaylistDTO();
                },
                (id, existing) => {
                    found = true;
                    existing.Update(fullPlaylist);
                    return existing;
                }
            );
            return found;
        }

        public bool UpdateArtist(SimpleArtist simpleArtist, out ArtistDTO entity) {
            bool found = false;
            entity = Artists.AddOrUpdate(
                simpleArtist.Id,
                id => {
                    return simpleArtist.ToArtistDTO();
                },
                (id, existing) => {
                    found = true;
                    existing.Update(simpleArtist);
                    return existing;
                }
            );
            return found;
        }

        public bool UpdateAlbum(SimpleAlbum album, out AlbumDTO entity) {
            bool found = false;
            entity = Albums.AddOrUpdate(
                album.Id,
                id => {
                    return album.ToAlbumDTO();
                },
                (id, existing) => {
                    found = true;
                    existing.Update(album);
                    return existing;
                }
            );

            if (!found) {
                var artistIds = album.Artists?.Select(a => a.Id) ?? [];
                entity.Artists = Artists
                    .Where(a => artistIds.Contains(a.Key))
                    .Select(a => a.Value)
                    .ToList();
            }
            return found;
        }

        public bool UpdateTrack(FullTrack track, AlbumDTO album, PlaylistDTO playlist, out TrackDTO entity) {
            bool found = false;
            entity = Tracks.AddOrUpdate(
                track.Id,
                id => {
                    return track.ToTrackDTO();
                },
                (id, existing) => {
                    found = true;
                    existing.Update(track);
                    return existing;
                }
            );

            if (!found) {
                entity.Album = album;
                var artistIds = track.Artists.Select(a => a.Id);
                entity.Artists = Artists
                    .Where(a => artistIds.Contains(a.Key))
                    .Select(a => a.Value)
                    .ToList();
            }

            var entityID = entity.ID;
            if (!playlist.Tracks.Any(t => t.ID == entityID)) {
                playlist.Tracks.Add(entity);
            }
            return found;
        }
    }
}
