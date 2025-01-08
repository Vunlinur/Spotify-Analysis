namespace SpotifyAnalysis.Data.SpotifyAPI {
    /// <summary>
    /// Creates a SpotifyModule instance using the appropriate SpotifyClient.
    /// This factory first attempts to use the scoped client (`SpotifyClientScoped`) for logged-in users.
    /// If the user has not logged in yet, it falls back to a singleton client (`SpotifyClientStatic`),
    /// which can be initialized during runtime after user authentication.
    /// 
    /// This factory is injected at page load. When the User logs in,
    /// initializes SpotifyClientScoped.SpotifyClient, which can then be used here.
    /// </summary>
    public class SpotifyModuleFactory(SpotifyClientStatic clientStatic, SpotifyClientScoped clientScoped) {
        readonly SpotifyClientStatic spotifyClientStatic = clientStatic;
        readonly SpotifyClientScoped spotifyClientScoped = clientScoped;

        public SpotifyModule GetModule()
            => new(spotifyClientScoped.SpotifyClient ?? spotifyClientStatic.SpotifyClient);
    }
}
