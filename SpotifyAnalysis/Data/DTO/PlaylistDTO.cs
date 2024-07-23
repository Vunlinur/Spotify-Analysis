using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpotifyAnalysis.Data.DTO {
    [Table("Playlists")]
    public class PlaylistDTO {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        public string Name { get; set; }

        public string OwnerID { get; set; }

        public string OwnerName { get; set; }

        public int Followers { get; set; }
        
        public string SnapshotID { get; set; }

        public int? TracksTotal { get; set; }

        public virtual List<TrackDTO> Tracks { get; set; }

        public virtual List<ImageDTO> Images { get; set; }
    }
}
