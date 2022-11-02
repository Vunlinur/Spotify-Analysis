using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data {
	public class FullTracks : SpotifyCache<FullTrack> {
		protected override string GetKeyForItem(FullTrack item) {
			return item.Id;
		}
	}

	public class FullArtists : SpotifyCache<FullArtist> {
		protected override string GetKeyForItem(FullArtist item) {
			return item.Id;
		}
	}

	public abstract class SpotifyCache<T> : KeyedCollection<string, T> {
		new public void Add(T item) {
			if (!Contains(GetKeyForItem(item)))
				base.Add(item);
		}
	}

	public static class Extensions {
		public static FullTracks ToKeyedCollection(this IEnumerable<FullTrack> allTracks) {
			var fullTracks = new FullTracks();
			foreach (var track in allTracks)
				if (!fullTracks.Contains(track.Id))
					fullTracks.Add(track);
			return fullTracks;
		}

		public static FullArtists ToKeyedCollection(this IEnumerable<FullArtist> allArtists) {
			var fullTracks = new FullArtists();
			foreach (var track in allArtists)
				if (!fullTracks.Contains(track.Id))
					fullTracks.Add(track);
			return fullTracks;
		}
	}
}
