using System.Collections.Generic;

namespace KuroAcad
{
    internal sealed class RibbonPanelDefinition
    {
        public string Title { get; }
        public IReadOnlyList<RibbonButtonDefinition> Buttons { get; }

        public RibbonPanelDefinition(string title, IReadOnlyList<RibbonButtonDefinition> buttons)
        {
            Title = title;
            Buttons = buttons;
        }
    }
}