using MudBlazor;
using SpotifyAnalysis.Data.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Pages {
    delegate bool DurationFilter(TrackPlaylist t, int duration);
    delegate bool ReleaseDateFilter(TrackPlaylist t, DateTime date);

    public partial class BrowseTracks {
        ReleaseDateFilter releaseDateFilter = releaseDateOperators["on"];
        DurationFilter durationFilter = durationOperators["is"];
        readonly FilterDefinition<TrackPlaylist> releaseDateFilterDefinition = new();
        readonly FilterDefinition<TrackPlaylist> durationFilterDefinition = new();

        DateTime? date = DateTime.Today;
        int msTotal;

        int Seconds { get { return _seconds; } set { _seconds = value; DurationChanged(); } }
        int _seconds;
        int Minutes { get { return _minutes; } set { _minutes = value; DurationChanged(); } }
        int _minutes;
        int Hours { get { return _hours; } set { _hours = value; DurationChanged(); } }
        int _hours;
        void DurationChanged() {
            msTotal = 500  // to offset the precision missing when displaying seconds
                + Seconds * 1000
                + Minutes * 60000
                + Hours * 3600000;
        }

        static readonly Dictionary<string, ReleaseDateFilter> releaseDateOperators = new() {
            { "on",             (t, date) => Parse(t.Track.Album.ReleaseDate) == date },
            { "not on",         (t, date) => Parse(t.Track.Album.ReleaseDate) != date },
            { "after",          (t, date) => Parse(t.Track.Album.ReleaseDate) > date },
            { "on or after",    (t, date) => Parse(t.Track.Album.ReleaseDate) >= date },
            { "before",         (t, date) => Parse(t.Track.Album.ReleaseDate) < date },
            { "on or before",   (t, date) => Parse(t.Track.Album.ReleaseDate) <= date }
        };

        static readonly Dictionary<string, DurationFilter> durationOperators = new() {
            { "is",                     (t, dur) => Math.Abs(t.Track.DurationMs - dur) < 500 },
            { "is not",                 (t, dur) => Math.Abs(t.Track.DurationMs - dur) >= 500 },
            { "longer than",            (t, dur) => t.Track.DurationMs > dur },
            { "longer than or equal",   (t, dur) => t.Track.DurationMs >= dur },
            { "shorter than",           (t, dur) => t.Track.DurationMs < dur },
            { "shorter than or equal",  (t, dur) => t.Track.DurationMs <= dur },
        };

        public BrowseTracks() {
            releaseDateFilterDefinition.FilterFunction = tp => releaseDateFilter(tp, date ?? DateTime.Today);
            durationFilterDefinition.FilterFunction = tp => durationFilter(tp, msTotal);
        }

        async Task ClearReleaseDateFilter(FilterContext<TrackPlaylist> context) => await context.Actions.ClearFilterAsync(releaseDateFilterDefinition);
        async Task ApplyReleaseDateFilter(FilterContext<TrackPlaylist> context) => await context.Actions.ApplyFilterAsync(releaseDateFilterDefinition);

        async Task ClearDurationFilter(FilterContext<TrackPlaylist> context) => await context.Actions.ClearFilterAsync(durationFilterDefinition);
        async Task ApplyDurationFilter(FilterContext<TrackPlaylist> context) => await context.Actions.ApplyFilterAsync(durationFilterDefinition);

        static DateTime Parse(string input) {
            string format = input.Length switch {
                4 => "yyyy",
                7 => "yyyy-MM",
                10 => "yyyy-MM-dd",
                _ => throw new ArgumentException($"Unexpected ReleaseDate format: {input}")
            };
            return DateTime.ParseExact(input, format, CultureInfo.InvariantCulture);
        }

        static string FormatDuration(CellContext<TrackPlaylist> context) {
            var ts = TimeSpan.FromMilliseconds(context.Item.Track.DurationMs);
            return ts.ToString(ts.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");
        }
    }

    class TrackPlaylist(TrackDTO t, PlaylistDTO p) {
        public readonly TrackDTO Track = t;
        public readonly PlaylistDTO Playlist = p;
    }
}
