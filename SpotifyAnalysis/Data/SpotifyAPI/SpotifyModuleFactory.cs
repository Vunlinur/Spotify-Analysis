namespace SpotifyAnalysis.Data.SpotifyAPI {
    /// <summary>
    /// Creates a SpotifyModule using a SpotifyClient object.
    /// Prefers SpotifyClientStatic (user logged in), falls back to SpotifyClientScoped (user not logged int)
    /// This factory is injected at page load. Then the User can log in, which initializes spotifyClientScoped.SpotifyClient.
    /// </summary>
    public class SpotifyModuleFactory(SpotifyClientStatic clientStatic, SpotifyClientScoped clientScoped) {
        readonly SpotifyClientStatic spotifyClientStatic = clientStatic;
        readonly SpotifyClientScoped spotifyClientScoped = clientScoped;

        public SpotifyModule GetModule()
            => new(spotifyClientScoped.SpotifyClient ?? spotifyClientStatic.SpotifyClient);
    }
}
