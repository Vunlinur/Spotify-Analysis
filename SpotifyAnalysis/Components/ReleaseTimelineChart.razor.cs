using ApexCharts;
using SpotifyAnalysis.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SpotifyAnalysis.Components {
	public class ArtistWithReleases {
		public ArtistDTO Artist { get; set; }
		public List<AlbumDTO> RecentReleases { get; set; } = [];
	}

	public partial class ReleaseTimelineChart {

		ApexChartOptions<AlbumDTO> chartOptions => BuildChartOptions(ArtistsWithReleases);

		private ApexChartOptions<AlbumDTO> BuildChartOptions(IEnumerable<ArtistWithReleases> artistsWithReleases) {
			string[] alternatingLabelColors = ["#1aa14b", "#cccccc"];
			var count = artistsWithReleases?.Count() ?? 0;

			var artistNames = artistsWithReleases.Select(a => a.Artist?.Name ?? "Unknown").ToList();
			var namesJson = JsonSerializer.Serialize(artistNames);
			var yAxisFormatter = $"function(val) {{ var n = {namesJson}; return n[val] !== undefined ? n[val] : ''; }}";

			// display the release name in the tooltip for each bubble
			var releaseTitlesBySeries = artistsWithReleases?
				.Select(a => a.RecentReleases.Select(r => r.Name).ToList())
				.ToList() ?? [[]];
			var releaseTitlesJson = JsonSerializer.Serialize(releaseTitlesBySeries);
			var tooltipYFormatter = $"function(value, {{ seriesIndex, dataPointIndex }}) {{ var t = {releaseTitlesJson}; var s = t[seriesIndex]; return (s && s[dataPointIndex]) ? s[dataPointIndex] : value; }}";

			// minimum 11px, ideally 27px per artist, +56 px for header & footer
			// this reaches 11px at 115 artists: y=27-x/15*log(x+1)
			var heightPerArtist = 27 - (count / 15f) * Math.Log10(count + 1);
			var heightTotal = 56 + (count + 2) * Math.Max(heightPerArtist, 11);

			var opts = new ApexChartOptions<AlbumDTO> {
				Debug = true,
				Theme = new Theme { Mode = Mode.Dark },
				Chart = new Chart {
					Height = heightTotal,
					Background = "#ffffff00",
					Zoom = new Zoom { AllowMouseWheelZoom = false }
				},
				Grid = new Grid { BorderColor = "#1aa14b" },
				PlotOptions = new PlotOptions {
					Bubble = new PlotOptionsBubble { MinBubbleRadius = 4, MaxBubbleRadius = 16 }
				},
				Tooltip = new ApexCharts.Tooltip {
					Theme = Mode.Dark,
					X = new TooltipX { Format = @"MMMM \ yyyy", Show = true },
					Y = new TooltipY {
						Title = new TooltipYTitle { Formatter = @"function(name) { return name + ':' }" },
						Formatter = tooltipYFormatter
					},
					Z = new TooltipZ { Title = "Popularity:" }
				},
				Xaxis = new XAxis {
					Title = new AxisTitle {
						OffsetY = 5,
						Text = "Month",
						Style = new AxisTitleStyle { FontSize = "14px", Color = "#aaaaaa" }
					},
					Labels = new XAxisLabels { Style = new AxisLabelStyle { FontSize = "14px", Colors = "#aaaaaa" } },
					AxisBorder = new AxisBorder { Height = 0 }
				},
				Legend = new Legend { Show = false },
				Annotations = new Annotations { Yaxis = new List<AnnotationsYAxis>() },
				Yaxis = new List<YAxis>() {
				new YAxis {
					Min = -1,
					Max = count,
					TickAmount = count + 1,
					ForceNiceScale = false,
					Title = new AxisTitle { Text = "Artist", Style = new AxisTitleStyle { FontSize = "14px", Color = "#aaaaaa" } },
					Labels = new YAxisLabels {
						Style = new AxisLabelStyle {
							FontSize = "12px",
							Colors = new ApexCharts.Color(alternatingLabelColors)
						},
						Formatter = yAxisFormatter
					}
				}
			}
			};

			return opts;
		}
	}
}
