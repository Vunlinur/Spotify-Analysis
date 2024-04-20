using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace SpotifyAnalysis.Data.DTO {
    [Table("Images")]
    public class ImageDTO {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        public string Url { get; set; }

        public int Resolution { get; set; }
    }
}
