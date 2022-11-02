using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

	public class FullPlaylists : SpotifyCache<Playlist> {
		protected override string GetKeyForItem(Playlist item) {
			return item.GetPlaylist.Id;
		}
	}

	public class Playlist {
		public SimplePlaylist GetPlaylist { get; }
		public FullTracks Tracks { get; }

		public Playlist(SimplePlaylist playlist) {
			GetPlaylist = playlist;
			Tracks = new FullTracks();
		}
	}

	public abstract class SpotifyCache<T> : KeyedCollection<string, T> {
		new public void Add(T item) {
			if (!Contains(GetKeyForItem(item)))
				base.Add(item);
		}
	}
}
