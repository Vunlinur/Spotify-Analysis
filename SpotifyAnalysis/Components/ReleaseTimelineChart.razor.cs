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
		ApexChartOptions<AlbumDTO> ChartOptions => GetChartOptions(ArtistsWithReleases);
		readonly ApexChartOptions<AlbumDTO> chartOptionsScoped = BuildRawChartOptions();

		ApexChartOptions<AlbumDTO> GetChartOptions(IEnumerable<ArtistWithReleases> artistsWithReleases) {
			int count = artistsWithReleases?.Count() ?? 0;
			chartOptionsScoped.Chart.Height = CalculateHeight(artistsWithReleases);
			chartOptionsScoped.Tooltip.Y.Formatter = CreateTooltipYFormatter(artistsWithReleases);
			chartOptionsScoped.Yaxis[0].Max = count;
			chartOptionsScoped.Yaxis[0].TickAmount = count + 1;
			chartOptionsScoped.Yaxis[0].Labels.Formatter = CreateYAxisFormatter(artistsWithReleases);
			return chartOptionsScoped;
		}

		/// Calculate the height of the chart based on the number of artists - more artists = less pixels per artist
		private static double CalculateHeight(IEnumerable<ArtistWithReleases> artistsWithReleases) {
			var count = artistsWithReleases?.Count() ?? 0;
			// minimum 11px, ideally 27px per artist, +56 px for header & footer
			// this reaches 11px at 115 artists: y=27-x/15*log(x+1)
			var heightPerArtist = 27 - (count / 15f) * Math.Log10(count + 1);
			return 56 + (count + 2) * Math.Max(heightPerArtist, 11);
		}

		/// Creates JS formatter replacing the numbers on the Y axis with artist names
		private static string CreateYAxisFormatter(IEnumerable<ArtistWithReleases> artistsWithReleases) {
			var artistNames = artistsWithReleases.Select(a => a.Artist?.Name ?? "Unknown").ToList();
			var namesJson = JsonSerializer.Serialize(artistNames);
			var yAxisFormatter = $"function(val) {{ var n = {namesJson}; return n[val] !== undefined ? n[val] : ''; }}";
			return yAxisFormatter;
		}

		/// Creates JS formatter to display the release name in the tooltip for each bubble
		private static string CreateTooltipYFormatter(IEnumerable<ArtistWithReleases> artistsWithReleases) {
			var releaseTitlesBySeries = artistsWithReleases?
				.Select(a => a.RecentReleases.Select(r => r.Name).ToList())
				.ToList() ?? [[]];
			var releaseTitlesJson = JsonSerializer.Serialize(releaseTitlesBySeries);
			var tooltipYFormatter = $"function(value, {{ seriesIndex, dataPointIndex }}) {{ var t = {releaseTitlesJson}; var s = t[seriesIndex]; return (s && s[dataPointIndex]) ? s[dataPointIndex] : value; }}";
			return tooltipYFormatter;
		}

		private static ApexChartOptions<AlbumDTO> BuildRawChartOptions() => new() {
			Debug = true,
			Theme = new Theme { Mode = Mode.Dark },
			Chart = new Chart {
				Height = "",  // overwritten
				Background = "#ffffff00",
				Zoom = new Zoom { AllowMouseWheelZoom = false }
			},
			Grid = new Grid { BorderColor = "#1aa14b" },
			PlotOptions = new PlotOptions {
				Bubble = new PlotOptionsBubble { MinBubbleRadius = 4, MaxBubbleRadius = 16 }
			},
			Tooltip = new Tooltip {
				Theme = Mode.Dark,
				X = new TooltipX { Format = @"MMMM \ yyyy", Show = true },
				Y = new TooltipY {
					Title = new TooltipYTitle { Formatter = @"function(name) { return name + ':' }" },
					Formatter = "",  // overwritten
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
					Max = 10,  // overwritten
					TickAmount = 11,  // overwritten
					ForceNiceScale = false,
					Title = new AxisTitle { Text = "Artist", Style = new AxisTitleStyle { FontSize = "14px", Color = "#aaaaaa" } },
					Labels = new YAxisLabels {
						Style = new AxisLabelStyle {
							FontSize = "12px",
							Colors = new Color(["#1aa14b", "#cccccc"])
						},
						Formatter = "",  // overwritten
					}
				}
			}
		};
	}
}
