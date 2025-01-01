using Microsoft.AspNetCore.Components;
using SpotifyAnalysis.Data;
using System;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Components {
    public abstract class WidgetBase : ComponentBase {
        public abstract string Title { get; }

        protected Elements elements;
        protected ChartBase chart;
        protected Action<Element> onClickCallback;

        public void ProcessData() {
            elements = BuildElements();
        }

        public async Task ProcessDataAsync() {
            elements = await Task.Run(BuildElements);
        }

        public async Task RefreshChartAsync() {
            if (chart is null)
                return;
            await InvokeAsync(StateHasChanged);
            await chart.RefreshChartAsync();
            //await InvokeAsync(StateHasChanged);
        }

        protected abstract Elements BuildElements();

        protected RenderFragment CreateChart(Type chartType) => builder => {
            builder.OpenComponent(0, chartType);
            builder.AddAttribute(1, nameof(ChartBase.Title), Title);
            builder.AddAttribute(2, nameof(ChartBase.Elements), elements);
            builder.AddAttribute(3, nameof(ChartBase.OnClickCallback), onClickCallback);
            builder.AddComponentReferenceCapture(4, o => chart = o as ChartBase);
            builder.CloseComponent();
        };
    }
}
