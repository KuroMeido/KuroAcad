namespace KuroAcad
{
    internal sealed class KuroRibbonButtonData
    {
        public string Id { get; }
        public string Text { get; }
        public string CommandName { get; }
        public string IconPath { get; }

        public KuroRibbonButtonData(string id, string text, string commandName, string iconPath)
        {
            Id = id;
            Text = text;
            CommandName = commandName;
            IconPath = iconPath;
        }
    }

    internal static class KuroRibbonButtons
    {
        private const string DefaultIcon = "Resources/icons8-urban-64.png";

        public static IReadOnlyList<KuroRibbonButtonData> MainPanel { get; } =
            new List<KuroRibbonButtonData>
            {
                new KuroRibbonButtonData("KUROACAD_BTN_KTEMDAT", "Tem Dat", "KTemDat", DefaultIcon),
                new KuroRibbonButtonData("KUROACAD_BTN_KGETTD", "Get TD", "KGetTD", DefaultIcon),
                new KuroRibbonButtonData("KUROACAD_BTN_KINTERSECTION", "Intersection", "KIntersection", DefaultIcon),
                new KuroRibbonButtonData("KUROACAD_BTN_KROAD", "Road", "KRoad", DefaultIcon),
                new KuroRibbonButtonData("KUROACAD_BTN_KTKLD", "TKLD", "KTKLD", DefaultIcon),
                new KuroRibbonButtonData("KUROACAD_BTN_KTRIMROAD", "Trim Road", "KTrimRoad", DefaultIcon),
                new KuroRibbonButtonData("KUROACAD_BTN_KPALETTE", "Palette", "KPalette", DefaultIcon),
                new KuroRibbonButtonData("KUROACAD_BTN_KCIRCLEWPF", "Circle WPF", "KCircleWPF", DefaultIcon),
            };
    }
}