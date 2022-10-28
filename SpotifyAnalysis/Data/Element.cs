using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data {
	public class Elements : List<Element> { }

	public class Element {
		public string Label { get; set; }
		public int Quantity { get; set; }
		public string Color { get; set; }
	}
}
