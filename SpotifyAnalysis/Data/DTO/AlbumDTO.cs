using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpotifyAnalysis.Data.DTO {
    [Table("Albums")]
    public class AlbumDTO {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        public string Name { get; set; }

        public string ReleaseDate { get; set; }

        public int TotalTracks { get; set; }

        public int Popularity { get; set; }

        public string Label { get; set; }

		public string ImageS { get; set; }

		public string ImageL { get; set; }

		public virtual List<ArtistDTO> Artists { get; set; }

        public virtual List<TrackDTO> Tracks { get; set; }
	}
}
