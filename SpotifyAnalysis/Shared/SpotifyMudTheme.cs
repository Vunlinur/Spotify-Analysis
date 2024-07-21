using MudBlazor;

namespace SpotifyAnalysis.Shared {
	public class SpotifyMudTheme : MudTheme {
        public SpotifyMudTheme() {
			PaletteDark = new PaletteDark() {
				Primary = new MudBlazor.Utilities.MudColor("#1DB954"),
				Secondary = new MudBlazor.Utilities.MudColor("#569bd7"),
				Background = Colors.Shades.Black,
				Surface = new MudBlazor.Utilities.MudColor("#242424")
			};
		}
    }
}
