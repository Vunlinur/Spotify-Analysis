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
        [Parameter, EditorRequired]
        public virtual IEnumerable<Element> Elements { get; set; }
        [Parameter]
        public virtual Action<Element> OnClickCallback { get; set; }
        [Parameter]
        public virtual Type HelpType { get; set; }
        [Parameter]
        public Action<ApexChartOptions<Element>>? ConfigureOptions { get; set; }

        public virtual ApexChartOptions<Element> Options { get; }

        protected ApexChart<Element> chart;
        private bool flaggedForUpdate = true; // true for initial render
        private bool parameterSet = false;

        protected abstract void ProcessElements();

        public void FlagForUpdate() => flaggedForUpdate = true;

        protected override void OnInitialized() {
            if (Options != null)
                ConfigureOptions?.Invoke(Options);
        }

        protected override Task OnParametersSetAsync() {
            ProcessElements();
            parameterSet = true;
            return Task.CompletedTask;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (flaggedForUpdate && parameterSet) {
                flaggedForUpdate = parameterSet = false;
                await chart.UpdateSeriesAsync(false);
                await chart.UpdateOptionsAsync(false, false, true);
            }
        }

        protected RenderFragment BuildHelp() => builder => {
            builder.OpenComponent(0, HelpType);
            builder.CloseComponent();
        };
    }
}
