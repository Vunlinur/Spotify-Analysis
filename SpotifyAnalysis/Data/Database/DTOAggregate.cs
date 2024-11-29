using Microsoft.EntityFrameworkCore;
using SpotifyAnalysis.Data.DTO;
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
            Playlists.UpdateOrAdd(fullPlaylist, out PlaylistDTO playlist);
            return playlist;
        }

        public ArtistDTO UpdateArtist(SimpleArtist sourceArtist) {
            Artists.UpdateOrAdd(sourceArtist, out ArtistDTO artist);
            return artist;
        }

        public AlbumDTO UpdateAlbum(SimpleAlbum album) {
            if (!Albums.UpdateOrAdd(album, out AlbumDTO dtoAlbum)) {
                var artistIds = album.Artists?.Select(a => a.Id) ?? [];
                dtoAlbum.Artists = Artists
                    .Where(a => artistIds.Contains(a.Key))
                    .Select(a => a.Value)
                    .ToList();
            }
            return dtoAlbum;
        }

        public TrackDTO UpdateTrack(FullTrack track, AlbumDTO album, PlaylistDTO playlist) {
            if (!Tracks.UpdateOrAdd(track, out TrackDTO dtoTrack)) {
                dtoTrack.Album = album;
                var artistIds = track.Artists.Select(a => a.Id);
                dtoTrack.Artists = Artists
                    .Where(a => artistIds.Contains(a.Key))
                    .Select(a => a.Value)
                    .ToList();
            }

            if (!playlist.Tracks.Any(t => t.ID == dtoTrack.ID)) {
                playlist.Tracks.Add(dtoTrack);
            }
            return dtoTrack;
        }
    }
}
