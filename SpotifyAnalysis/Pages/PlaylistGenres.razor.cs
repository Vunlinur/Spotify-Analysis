using SpotifyAnalysis.Data.DTO;
using System;
using System.Collections.Generic;

namespace SpotifyAnalysis.Pages {
	public partial class PlaylistGenres {
		public class Playlist(PlaylistDTO playlistDTO, List<Genre> topGenres) {
			public PlaylistDTO playlistDTO = playlistDTO;
			public List<Genre> topGenres = topGenres;
		}

		public class Genre(string name, int count) {
			public string name = name;
			public int count = count;
			public string color;
		}
	}
}
