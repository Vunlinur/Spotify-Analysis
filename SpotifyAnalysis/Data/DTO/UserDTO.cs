using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpotifyAnalysis.Data.DTO {
    [Table("Users")]
    public class UserDTO {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        public string Name { get; set; }

        public virtual List<PlaylistDTO> Playlists { get; set; }
    }
}
