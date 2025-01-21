using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace SpotifyAnalysis.Data.DTO {
    [Table("Images")]
    public class ImageDTO {
        [Key]
        public string Url { get; set; }

        public int Resolution { get; set; }
    }
}
