using Microsoft.Extensions.Configuration;
using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Threading.Tasks;
using static SpotifyAPI.Web.Scopes;


namespace SpotifyAnalysis.Data.SpotifyAPI {
    /// <summary>
    /// Manages a scoped SpotifyClient instance for user-specific API interactions.
    /// This client is initialized using Spotify's Authorization Code Flow (OAuth), enabling access to the user's
    /// private playlists, libraries, and other user-specific data.
    /// </summary>
    /// <remarks>
    /// This class also maintains the authenticated user's profile information (`UserDTO`) and triggers 
    /// a `UserChanged` event whenever the authenticated user context is updated.
    /// </remarks>
    public class SpotifyClientScoped : IUserContainer {
        public UserDTO UserDTO {
            get => user;
            set => UserChanged?.Invoke(user = value);
        }
        public event Action<UserDTO> UserChanged;
        public SpotifyClient SpotifyClient { get; set; }

        private UserDTO user;
        private EmbedIOAuthServer server;


        public async void InitializeSpotifyClient() {
            server = new EmbedIOAuthServer(
                new Uri(Program.Config.GetValue<string>("OAuthServerUri")),
                5543
            );
            server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
            await server.Start();

            var request = new LoginRequest(server.BaseUri, Program.Config.GetValue<string>("ClientId"), LoginRequest.ResponseType.Code) {
                Scope = [
                    UserLibraryRead,
                    PlaylistReadPrivate,
                    PlaylistReadCollaborative,
                    //UserTopRead,
                    //UserFollowRead,
                ]
            };

            var uri = request.ToUri();
            try {
                BrowserUtil.Open(uri);
            }
            catch (Exception) {
                Console.WriteLine("Unable to open URL, manually open: {0}", uri);
            }
        }

        private async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response) {
            await server.Stop();

            var token = await new OAuthClient().RequestToken(new AuthorizationCodeTokenRequest(
                Program.Config.GetValue<string>("ClientId"),
                Program.Config.GetValue<string>("ClientSecret"),
                response.Code,
                server.BaseUri)
            );

            var config = SpotifyClientConfig.CreateDefault().WithToken(token.AccessToken, token.TokenType);
            SpotifyClient = new SpotifyClient(config);

            UserDTO = (await SpotifyClient.UserProfile.Current()).ToUserDTO();
        }
    }
}
