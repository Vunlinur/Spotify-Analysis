using Microsoft.AspNetCore.Components;
using SpotifyAnalysis.Data;
using System;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Components {
    public abstract class WidgetBase : ComponentBase {
        public abstract string Title { get; }

        protected Elements elements;
        protected MudItemPieChart chart;
        protected Action<Element> onClickCallback;

        public void ProcessData() {
            elements = BuildElements();
        }

        public async Task ProcessDataAsync() {
            elements = await Task.Run(BuildElements);
        }

        public async Task RefreshChartAsync() {
            await InvokeAsync(StateHasChanged);
            await chart.RefreshChartAsync();
            //await InvokeAsync(StateHasChanged);
        }

        protected abstract Elements BuildElements();

        protected RenderFragment CreateChart() => builder => {
            builder.OpenComponent(0, typeof(MudItemPieChart));
            builder.AddAttribute(1, nameof(MudItemPieChart.Title), Title);
            builder.AddAttribute(2, nameof(MudItemPieChart.Elements), elements);
            builder.AddAttribute(3, nameof(MudItemPieChart.OnClickCallback), onClickCallback);
            builder.AddComponentReferenceCapture(4, o => chart = o as MudItemPieChart);
            builder.CloseComponent();
        };
    }
}
