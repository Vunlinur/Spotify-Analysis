using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpotifyAnalysis.Data {
	public class Elements : KeyedCollection<string, Element> {
		protected override string GetKeyForItem(Element item) => item.Label;

        public void Increase(Element item) {
			if (Contains(item.Label))
				this[item.Label].Quantity += item.Quantity;
			else
				Add(item);
		}

		public Element Extract(string label) {
			if (Contains(label)) {
				var element = this[label];
				Remove(label);
				return element;
			}
			else
				return null;
		}
    }

    public static class ElementsExtensions {
        public static IEnumerable<Element> Increase(this IEnumerable<Element> elements, Element element) {
            var existingElement = elements.FirstOrDefault(e => e.Label == element.Label);
            if (existingElement is not null)
                existingElement.Quantity += element.Quantity;
            else
                elements = elements.Append(element);
			return elements;
        }
    }

    public class Element {
		public string Label { get; set; }
		public int Quantity { get; set; }
		public string Color { get; set; }
	}
}
