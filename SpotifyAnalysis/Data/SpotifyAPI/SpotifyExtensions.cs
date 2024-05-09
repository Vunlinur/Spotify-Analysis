using SpotifyAnalysis.Data.DTO;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyAnalysis.Data.SpotifyAPI {
    public static class SpotifyExtensions {
		public static IEnumerable<FullTrack> ToFullTracks(this IEnumerable<PlaylistTrack<IPlayableItem>> playableItems) {
			foreach (PlaylistTrack<IPlayableItem> item in playableItems)
				if (item.Track is FullTrack track)
					yield return track;
		}

		public static IEnumerable<FullEpisode> ToFullEpisodes(this IEnumerable<PlaylistTrack<IPlayableItem>> playableItems) {
			foreach (PlaylistTrack<IPlayableItem> item in playableItems)
				if (item.Track is FullEpisode track)
					yield return track;
		}
	}
}
