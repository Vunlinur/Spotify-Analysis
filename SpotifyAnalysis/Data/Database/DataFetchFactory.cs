using SpotifyAnalysis.Data.SpotifyAPI;


namespace SpotifyAnalysis.Data.Database {
    public static class DataFetchFactory {
        public static DataFetch GetDefault(SpotifyModule spotifyModule, UpdateProgressBarDelegate updateProgressBar)
            => new(
                spotifyModule.GetUserProfile,
                spotifyModule.GetUsersPublicPlaylistsAsync,
                spotifyModule.GetPlaylistAsync,
                spotifyModule.GetTracksAsync,
                spotifyModule.GetArtistsAsync,
                spotifyModule.GetAlbumsAsync,
                updateProgressBar
            );
    }
}
