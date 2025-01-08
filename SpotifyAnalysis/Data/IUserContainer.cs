using SpotifyAnalysis.Data.DTO;
using System;

namespace SpotifyAnalysis.Data {
    public interface IUserContainer {
        public UserDTO UserDTO { get; set; }
        public event Action<UserDTO> UserChanged;
    }
}
