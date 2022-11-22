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
		public FullPlaylists(IEnumerable<SimplePlaylist> simplePlaylists) {
			foreach (var simplePlaylist in simplePlaylists)
				Add(new Playlist(simplePlaylist));
		}

		protected override string GetKeyForItem(Playlist item) {
			return item.Id;
		}
	}

	public class Playlist : SimplePlaylist {
		public FullTracks FullTracks { get; }

		public Playlist(SimplePlaylist copy) {
			FullTracks = new FullTracks();

			Collaborative = copy.Collaborative;
			Description = copy.Description;
			ExternalUrls = copy.ExternalUrls;
			Href = copy.Href;
			Id = copy.Id;
			Images = copy.Images;
			Name = copy.Name;
			Owner = copy.Owner;
			Public = copy.Public;
			SnapshotId = copy.SnapshotId;
			Tracks = copy.Tracks;
			Type = copy.Type;
			Uri = copy.Uri;
		}
	}

	public abstract class SpotifyCache<T> : KeyedCollection<string, T> {
		new public void Add(T item) {
			lock(this)
				if (!Contains(GetKeyForItem(item)))
					base.Add(item);
		}
	}
}
