using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Threading.Tasks;
using static SpotifyAPI.Web.Scopes;
using SpotifyAnalysis.Data.Common;
using Microsoft.JSInterop;


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
    public class SpotifyClientScoped(ProtectedLocalStorage protectedLocalStorage, IJSRuntime JSRuntime) : IUserContainer {
        private readonly Storage<AuthorizationCodeTokenResponse> accessTokenStorage = new(nameof(accessTokenStorage), protectedLocalStorage);
        private readonly IJSRuntime JSRuntime = JSRuntime;

        public UserDTO UserDTO {
            get => user;
            set => UserChanged?.Invoke(user = value);
        }
        public event Action<UserDTO> UserChanged;

        public SpotifyClient SpotifyClient { get; set; }

        private UserDTO user;
        private EmbedIOAuthServer server;

        public async void InitializeSpotifyClient() {
            if (await CheckClientInitialized())
                await DestroyClient();
            else
                await AuthenticateUser();
        }

        public async Task<bool> CheckClientInitialized() {
            var accessToken = await accessTokenStorage.Get();
            bool validTokenFound = accessToken?.IsExpired == false;
            if (validTokenFound) 
                await CreateClient(accessToken);
            return validTokenFound;
        }

        private async Task AuthenticateUser() {
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
            await JSRuntime.InvokeVoidAsync("open", uri.AbsoluteUri, "_blank");
        }

        private async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response) {
            await server.Stop();

            var token = await new OAuthClient().RequestToken(new AuthorizationCodeTokenRequest(
                Program.Config.GetValue<string>("ClientId"),
                Program.Config.GetValue<string>("ClientSecret"),
                response.Code,
                server.BaseUri)
            );
            await accessTokenStorage.Set(token);
            await CreateClient(token);
        }

        private async Task DestroyClient() {
            SpotifyClient = null;
            UserDTO = new UserDTO();
            await accessTokenStorage.Set(null);
        }

        private async Task CreateClient(AuthorizationCodeTokenResponse token) {
            var config = SpotifyClientConfig.CreateDefault().WithToken(token.AccessToken, token.TokenType);
            SpotifyClient = new SpotifyClient(config);

            UserDTO = (await SpotifyClient.UserProfile.Current()).ToUserDTO();
        }
    }
}
