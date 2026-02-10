using Microsoft.AspNetCore.Components;
using SpotifyAnalysis.Data;
using System;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Components {
    public abstract class WidgetBase : ComponentBase {
        public abstract string Title { get; }
        protected virtual Type HelpType{ get; }

        protected Elements elements;
        protected ChartBase chart;
        protected Action<Element> onClickCallback;

        protected abstract Elements BuildElements();

        public void FlagForUpdate() => chart?.FlagForUpdate();

        protected override async Task OnParametersSetAsync() {
			elements = await Task.Run(BuildElements);
		}


        protected RenderFragment CreateChart(Type chartType) => builder => {
            builder.OpenComponent(0, chartType);
            builder.AddAttribute(1, nameof(ChartBase.Title), Title);
            builder.AddAttribute(2, nameof(ChartBase.Elements), elements);
            builder.AddAttribute(3, nameof(ChartBase.OnClickCallback), onClickCallback);
            builder.AddAttribute(4, nameof(ChartBase.HelpType), HelpType);
            builder.AddComponentReferenceCapture(5, o => chart = o as ChartBase);
            builder.CloseComponent();
        };
    }
}
