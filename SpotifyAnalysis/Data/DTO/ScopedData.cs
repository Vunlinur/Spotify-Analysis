using System;

namespace SpotifyAnalysis.Data.DTO {
	public class ScopedData : IUserContainer {
        public event Action<UserDTO> UserChanged;

        public UserDTO UserDTO {
			get => _user;
			set => UserChanged?.Invoke(_user = value);
        }
        private UserDTO _user;
    }
}
