using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data {
	public class Elements : KeyedCollection<string, Element> {
		protected override string GetKeyForItem(Element item) {
			return item.Label;
		}

		public void Increase(Element item) {
			if (Contains(item.Label))
				this[item.Label].Quantity += item.Quantity;
			else
				Add(item);
		}
	}

	public class Element {
		public string Label { get; set; }
		public int Quantity { get; set; }
		public string Color { get; set; }
	}
}
