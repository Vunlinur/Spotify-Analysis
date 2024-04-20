using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpotifyAnalysis.Data.DTO {
    [Table("Tracks")]
    public class TrackDTO {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }
        
        public string Name { get; set; }

        public int DurationMs { get; set; }

        public int Popularity { get; set; }

        public virtual AlbumDTO Album { get; set; }

        public virtual List<ArtistDTO> Artists { get; set; }
    }
}
