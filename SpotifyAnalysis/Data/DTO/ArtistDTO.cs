using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpotifyAnalysis.Data.DTO {
    [Table("Artists")]
    public class ArtistDTO {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        public string Name { get; set; }

        public int? Popularity { get; set; }

        public List<string> Genres { get; set; }

        public virtual List<AlbumDTO> Albums { get; set; }

        public virtual List<ImageDTO> Images { get; set; }
    }
}
