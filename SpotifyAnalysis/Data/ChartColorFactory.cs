using ChartJs.Blazor.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyAnalysis.Data {
	public interface ChartColorFactory {
		public string Next();
	}

	public class RandomColor : ChartColorFactory {
		public string Next() {
			return ColorUtil.RandomColorString();
		}
	}

	public class Pastel : ChartColorFactory {
		private static readonly Random random = new Random();
		private const int min = 100;
		private const int max = 255;

		public string Next() {
			return ColorUtil.ColorHexString(
				(byte)random.Next(min, max),
				(byte)random.Next(min, max),
				(byte)random.Next(min, max)
			);
		}
	}

	public class Rainbow : ChartColorFactory {
		private static readonly string[] colors = { "#9400D3", "#4B0082", "#0000FF", "#00FF00", "#FFFF00", "#FF7F00", "#FF0000" };
		private int step = 0;

		public string Next() {
			return colors[step++ % colors.Length];
		}
	}

	public class RotateHue : ChartColorFactory {
		private const double thirdCycle = Math.PI / 3 * 2;
		private const double twoThirdsCycle = thirdCycle * 2;
		private double offset = 0;
		private readonly double step;
		private readonly byte magnitude;

		public RotateHue(double step = 0.2, byte magnitude = 255) {
			this.step = step;
			this.magnitude = magnitude;
		}

		public string Next() {
			offset += step;
			return ColorUtil.ColorHexString(
				Sin(offset),
				Sin(offset + thirdCycle),
				Sin(offset + twoThirdsCycle)
			);
		}

		internal byte Sin(double x) {
			return (byte)((
				Math.Min(0, Math.Sin(x)) // take only range -1 to 0
				+ 1 // lift above 0
				) * magnitude); // increase magnitude, default 0-255
		}
	}
}