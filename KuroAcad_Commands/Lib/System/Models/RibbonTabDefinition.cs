using System.Collections.Generic;

namespace KuroAcad
{
    internal sealed class RibbonTabDefinition
    {
        public string Id { get; }
        public string Title { get; }
        public IReadOnlyList<RibbonPanelDefinition> Panels { get; }

        public RibbonTabDefinition(string id, string title, IReadOnlyList<RibbonPanelDefinition> panels)
        {
            Id = id;
            Title = title;
            Panels = panels;
        }
    }
}