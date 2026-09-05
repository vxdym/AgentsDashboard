using AgentsDashboard.Models;
using AgentsDashboard.Services;
using Microsoft.AspNetCore.Components;

namespace AgentsDashboard.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] private FeatureStore Store { get; set; } = null!;
        [Inject] private NavigationManager Navigation { get; set; } = null!;

        private List<Feature> features = new();

        protected override void OnInitialized()
        {
            features = Store.GetAll();
        }

        private void OpenFeature(Feature feature)
        {
            Navigation.NavigateTo($"feature/{feature.Id}");
        }
    }
}
