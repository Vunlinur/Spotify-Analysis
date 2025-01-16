using System;
using SpotifyAnalysis.Data.DTO;

namespace SpotifyAnalysis.Data.Common {
    public class ScopedData : IUserContainer {

        public event Action<UserDTO> UserChanged;

        public UserDTO UserDTO {
            get => _user;
            set => UserChanged?.Invoke(_user = value);
        }
        private UserDTO _user;
    }
}
