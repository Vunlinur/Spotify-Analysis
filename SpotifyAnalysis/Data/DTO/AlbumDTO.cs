using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace SpotifyAnalysis.Data.DTO {
    [Table("Albums")]
    public class AlbumDTO {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        public string Name { get; set; }

        public AlbumType Type { get; set; }

        public string ReleaseDate { get; set; }

        public int TotalTracks { get; set; }

        public int Popularity { get; set; }

        public string Label { get; set; }

        public string ImageS { get; set; }

        public string ImageL { get; set; }

        public virtual List<ArtistDTO> Artists { get; set; }

        public virtual List<TrackDTO> Tracks { get; set; }

        // Meta
        public DateTime LastUpdated { get; set; }
    }

    // The relationship between the artist and the album
    public enum AlbumType { album, single, compilation, appears_on }

    public static class AlbumTypeExtensions {
        public static AlbumType FromString(this AlbumType type, string from) {
            Enum.TryParse(from, out AlbumType result);
            return result;
        }
    }
}
