using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;
using static SpotifyAPI.Web.Scopes;
using SpotifyAnalysis.Data.Common;
using Microsoft.AspNetCore.Components;
using System.Threading;


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
    public class SpotifyClientScoped(ProtectedLocalStorage protectedLocalStorage, NavigationManager navigation, SpotifyHttpClientProvider httpClientProvider) : IUserContainer {
        private readonly SpotifyHttpClientProvider httpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
        private readonly Storage<AuthorizationCodeTokenResponse> accessTokenStorage = new(nameof(accessTokenStorage), protectedLocalStorage);
        private readonly NavigationManager navigation = navigation;

        public UserDTO UserDTO {
            get => user;
            set => UserChanged?.Invoke(user = value);
        }

        public SpotifyClient SpotifyClient { get; set; }

        private Timer refreshTimer;
        private UserDTO user;
        public event Action<UserDTO> UserChanged;

        public async Task InitializeSpotifyClient() {
            // TODO add timed refresh? expiry can happen for a user
            if (await CheckClientInitialized())
                await DestroyClient();
            else
                AuthenticateUser();
        }

        public async Task<bool> CheckClientInitialized() {
            var accessToken = await accessTokenStorage.Get();
            bool validTokenFound = accessToken?.IsExpired == false;
            if (validTokenFound)
                await CreateClient(accessToken);
            return validTokenFound;
        }

        private void AuthenticateUser() {
            var request = new LoginRequest(
                new Uri(Program.Config.GetValue<string>("OAuthServerUri")),
                Program.Config.GetValue<string>("ClientId"),
                LoginRequest.ResponseType.Code) {
                Scope = [
                    UserLibraryRead,
                    PlaylistReadPrivate,
                    PlaylistReadCollaborative,
                    UserTopRead,
                    //UserFollowRead,
                ]
            };

            var uri = request.ToUri();
            navigation.NavigateTo(uri.AbsoluteUri);
        }

        public async Task ExchangeCodeForTokenAsync(Uri uri) {
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var authorizationCode = queryParams["code"];
            if (authorizationCode is null)
                return;  // Logging
            else
                await RequestTokenAsync(authorizationCode);
        }

        private async Task RequestTokenAsync(string authorizationCode) {
            int refreshTime;
            string refreshToken = null;
            try {
                var token = await new OAuthClient().RequestToken(new AuthorizationCodeTokenRequest(
                Program.Config.GetValue<string>("ClientId"),
                Program.Config.GetValue<string>("ClientSecret"),
                authorizationCode,
                new Uri(Program.Config.GetValue<string>("OAuthServerUri")))
            );
                await accessTokenStorage.Set(token);
                await CreateClient(token);

                refreshTime = token.ExpiresIn - 30; // Refresh 30 seconds before expiry
                refreshToken = token.RefreshToken;
            }
            catch (APIException e) {
                if (e.Message != "invalid_grant")  // invalid_grant is OK, happens when we request token with an already used code
                    throw;
                refreshTime = 30;  // retry in 30 sec
            }

            refreshTimer?.Dispose();
            refreshTimer = new Timer(async _ => await RequestTokenAsync(refreshToken), null, TimeSpan.FromSeconds(refreshTime), Timeout.InfiniteTimeSpan);
        }

        private async Task CreateClient(AuthorizationCodeTokenResponse token) {
            var config = SpotifyClientConfig.CreateDefault()
                .WithHTTPClient(httpClientProvider.HttpClient)
                .WithToken(token.AccessToken, token.TokenType);
            SpotifyClient = new SpotifyClient(config);
            UserDTO = (await SpotifyClient.UserProfile.Current()).ToUserDTO();
        }

        private async Task DestroyClient() {
            SpotifyClient = null;
            UserDTO = null;
            await accessTokenStorage.Set(null);
        }
    }
}
