using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Globalization;

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

		public static DateTime ParseReleaseDate(string input) {
			string format = input.Length switch {
				4 => "yyyy",
				7 => "yyyy-MM",
				10 => "yyyy-MM-dd",
				_ => throw new ArgumentException($"Unexpected ReleaseDate format: {input}")
			};
			return DateTime.ParseExact(input, format, CultureInfo.InvariantCulture);
		}
	}

    // The relationship between the artist and the album
    public enum AlbumType { album, single, compilation, appears_on }

    public static class AlbumTypeExtensions {
        public static AlbumType ToAlbumType(this string from) {
            _ = Enum.TryParse(from, out AlbumType result);
            return result;
        }

        public static string ToColor(this AlbumType albumType) {
            return albumType switch {
				AlbumType.album => "#B9541D",
				AlbumType.single => "#541DB9", // #337db5 alternative
				AlbumType.compilation => "#B91D82", // #326544 alternative
				AlbumType.appears_on => "#A0A0A0", // rare
				_ => "#FF00FF" // Unknown
			};
		}
	}
}
