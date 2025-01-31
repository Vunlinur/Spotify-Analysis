using MudBlazor;
using SpotifyAnalysis.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Pages {
    delegate bool DurationFilter(TrackPlaylist t, int duration);

    public partial class BrowseTracks {
        DurationFilter durationFilter = Operators["is"];
        readonly FilterDefinition<TrackPlaylist> filterDefinition = new();

        int Seconds { get { return _seconds; } set { _seconds = value; DurationChanged(); } }
        int _seconds;
        int Minutes { get { return _minutes; } set { _minutes = value; DurationChanged(); } }
        int _minutes;
        int Hours { get { return _hours; } set { _hours = value; DurationChanged(); } }
        int _hours;

        static readonly Dictionary<string, DurationFilter> Operators = new() {
            { "is",                     (t, dur) => Math.Abs(t.Track.DurationMs - dur) < 500 },
            { "is not",                 (t, dur) => Math.Abs(t.Track.DurationMs - dur) >= 500 },
            { "longer than",            (t, dur) => t.Track.DurationMs > dur },
            { "longer than or equal",   (t, dur) => t.Track.DurationMs >= dur },
            { "shorter than",           (t, dur) => t.Track.DurationMs < dur },
            { "shorter than or equal",  (t, dur) => t.Track.DurationMs <= dur },
        };

        void DurationChanged() {
            var ms = 500  // to offset the precision missing when displaying seconds
                + Seconds * 1000
                + Minutes * 60000
                + Hours * 3600000;
            filterDefinition.FilterFunction = x => durationFilter(x, ms);
        }

        async Task ClearFilter(FilterContext<TrackPlaylist> context) => await context.Actions.ClearFilterAsync(filterDefinition);

        async Task ApplyFilter(FilterContext<TrackPlaylist> context) => await context.Actions.ApplyFilterAsync(filterDefinition);

        static string FormatDuration(CellContext<TrackPlaylist> context) {
            var ts = TimeSpan.FromMilliseconds(context.Item.Track.DurationMs);
            return ts.ToString(ts.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");
        }
    }

    class TrackPlaylist(TrackDTO t, PlaylistDTO p) {
        public readonly TrackDTO Track = t;
        public readonly PlaylistDTO Playlist = p;
        public readonly string Artists = string.Join(", ", t.Artists.Select(a => a.Name));
    }
}
