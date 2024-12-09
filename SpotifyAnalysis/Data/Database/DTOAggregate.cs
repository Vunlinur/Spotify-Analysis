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

        public PlaylistDTO UpdatePlaylist(FullPlaylist fullPlaylist) {
            PlaylistDTO dto = Playlists.AddOrUpdate(
                fullPlaylist.Id,
                id => {
                    // this shouldn't really happen as we should already have
                    // all the playlists in the db before adding their details
                    return fullPlaylist.ToPlaylistDTO();
                },
                (id, existing) => {
                    existing.Update(fullPlaylist);
                    return existing;
                }
            );
            return dto;
        }

        public ArtistDTO UpdateArtist(SimpleArtist simpleArtist) {
            ArtistDTO dto = Artists.AddOrUpdate(
                simpleArtist.Id,
                id => {
                    return simpleArtist.ToArtistDTO();
                },
                (id, existing) => {
                    existing.Update(simpleArtist);
                    return existing;
                }
            );
            return dto;
        }

        public AlbumDTO UpdateAlbum(SimpleAlbum album) {
            bool created = false;
            AlbumDTO dto = Albums.AddOrUpdate(
                album.Id,
                id => {
                    created = true;
                    return album.ToAlbumDTO();
                },
                (id, existing) => {
                    existing.Update(album);
                    return existing;
                }
            );

            if (created) {
                var artistIds = album.Artists?.Select(a => a.Id) ?? [];
                dto.Artists = Artists
                    .Where(a => artistIds.Contains(a.Key))
                    .Select(a => a.Value)
                    .ToList();
            }
            return dto;
        }

        public TrackDTO UpdateTrack(FullTrack track, AlbumDTO album, PlaylistDTO playlist) {
            bool created = false;
            TrackDTO dto = Tracks.AddOrUpdate(
                track.Id,
                id => {
                    created = true;
                    return track.ToTrackDTO();
                },
                (id, existing) => {
                    existing.Update(track);
                    return existing;
                }
            );

            if (created) {
                dto.Album = album;
                var artistIds = track.Artists.Select(a => a.Id);
                dto.Artists = Artists
                    .Where(a => artistIds.Contains(a.Key))
                    .Select(a => a.Value)
                    .ToList();
            }

            if (!playlist.Tracks.Any(t => t.ID == dto.ID)) {
                playlist.Tracks.Add(dto);
            }
            return dto;
        }
    }
}
