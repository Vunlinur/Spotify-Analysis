using Microsoft.EntityFrameworkCore;
using SpotifyAnalysis.Data.DTO;
using SpotifyAnalysis.Data.SpotifyAPI;
using SpotifyAPI.Web;
using System;
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
                Albums = new ConcurrentDictionary<string, AlbumDTO>(await db.Albums.Include(a => a.Artists).ToDictionaryAsync(t => t.ID, t => t)),
                Artists = new ConcurrentDictionary<string, ArtistDTO>(await db.Artists.ToDictionaryAsync(t => t.ID, t => t))
            };
        }

        public bool GetOrAddPlaylist(FullPlaylist fullPlaylist, out PlaylistDTO entity) =>
            GetOrAddEntity(
                Playlists,
                fullPlaylist.Id,
                id => fullPlaylist.ToPlaylistDTO(),
                out entity
            );

        public bool GetOrAddArtist(SimpleArtist simpleArtist, out ArtistDTO entity) =>
            GetOrAddEntity(
                Artists,
                simpleArtist.Id,
                id => simpleArtist.ToArtistDTO(),
                out entity
            );

        public bool GetOrAddArtist(FullArtist simpleArtist, out ArtistDTO entity) =>
            GetOrAddEntity(
                Artists,
                simpleArtist.Id,
                id => simpleArtist.ToArtistDTO(),
                out entity
            );

        public bool GetOrAddAlbum(SimpleAlbum album, out AlbumDTO entity) =>
            GetOrAddEntity(
                Albums,
                album.Id,
                id => album.ToAlbumDTO(),
                out entity
            );

        public bool GetOrAddAlbum(FullAlbum album, out AlbumDTO entity) =>
            GetOrAddEntity(
                Albums,
                album.Id,
                id => album.ToAlbumDTO(),
                out entity
            );

        public bool GetOrAddTrack(FullTrack track, out TrackDTO entity) =>
            GetOrAddEntity(
                Tracks,
                track.Id,
                id => track.ToTrackDTO(),
                out entity
            );

        public List<ArtistDTO> GetArtists(List<SimpleArtist> simpleArtists) {
            var artistIds = simpleArtists?.Select(a => a.Id) ?? [];
                return Artists
                .Where(a => artistIds.Contains(a.Key))
                .Select(a => a.Value)
                .ToList();
        }

        private static bool GetOrAddEntity<TEntity>(ConcurrentDictionary<string, TEntity> dictionary, 
            string key, Func<string, TEntity> createEntity, out TEntity entity)
        {
            bool found = false;
            entity = dictionary.AddOrUpdate(
                key,
                id => createEntity(id),
                (id, existing) => {
                    found = true;
                    return existing;
                }
            );
            return found;
        }
    }
}
