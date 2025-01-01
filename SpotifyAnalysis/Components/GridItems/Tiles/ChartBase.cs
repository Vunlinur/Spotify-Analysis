using ApexCharts;
using Microsoft.AspNetCore.Components;
using SpotifyAnalysis.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Components {
    public abstract class ChartBase : ComponentBase {
        [Parameter]
        public virtual string Title { get; set; }
        [Parameter]
        public virtual IEnumerable<Element> Elements { get; set; }
        [Parameter]
        public virtual Action<Element> OnClickCallback { get; set; }

        protected ApexChart<Element> chart;


        public virtual async Task RefreshChartAsync() {
            if (chart is null)
                return;
            ProcessElements();
            await chart.UpdateSeriesAsync(false);
            await chart.UpdateOptionsAsync(false, false, true);
        }

        protected override void OnInitialized() {
            ProcessElements();
        }

        protected abstract void ProcessElements();
    }
}
