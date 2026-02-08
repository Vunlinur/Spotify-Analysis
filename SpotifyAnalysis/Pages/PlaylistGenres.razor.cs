using Fernandezja.ColorHashSharp;
using SpotifyAnalysis.Data.DTO;
using System;
using System.Linq;
using System.Collections.Generic;
using static SpotifyAnalysis.Pages.PlaylistGenres;
using System.Diagnostics;

namespace SpotifyAnalysis.Pages {
	public partial class PlaylistGenres {
		[DebuggerDisplay("{playlistDTO.Name}|{genre1.name}|{genre2.name}|{genre3.name}")]
		public class Playlist(PlaylistDTO playlistDTO, List<Genre> topGenres) {
			public PlaylistDTO playlistDTO = playlistDTO;
			public List<Genre> topGenres = topGenres ??= [];
			public Genre genre1 = topGenres.ElementAtOrDefault(0) ?? Genre.empty;
			public Genre genre2 = topGenres.ElementAtOrDefault(1) ?? Genre.empty;
			public Genre genre3 = topGenres.ElementAtOrDefault(2) ?? Genre.empty;
		}

		public class Genre(string name, int count) {
			public string name = name;
			public int count = count;
			// Not calculated initially for performance, run Color():
			public string color = "";
			public string root = "";  // e.g. "metal" from "power metal"

			public static readonly Genre empty = new("", 0);
		}
	}

	public static class GenreExtensions {
		public static readonly char[] splitOn = [' ', '-'];
		private static readonly ColorHash colorHash = new();

		public static Genre Color(this Genre genre) {
			var tokens = genre.name
				.ToLower()
				.Replace("music", "")
				.Split(splitOn, StringSplitOptions.RemoveEmptyEntries);
			genre.root = tokens.Last(); // e.g. "rock" from "classic rock"
			genre.color = '#' + colorHash.Hex(genre.root);
			return genre;
		}

		public static void Color(this IEnumerable<Genre> genres, Dictionary<string, Genre> genreSamples = null) {
			genreSamples ??= new(StringComparer.OrdinalIgnoreCase);

			foreach (var genre in genres) {
				if (!genreSamples.ContainsKey(genre.name))
					genreSamples[genre.name] = genre.Color();
				genre.root = genreSamples[genre.name].root;
				genre.color = genreSamples[genre.name].color;
			}
		}
	}
}
