using System.Collections.Generic;

namespace KuroAcad
{
    internal static class KuroRibbonButtons
    {
        private const string DefaultIcon = "Resources/icons8-urban-64.png";

        public static IReadOnlyList<RibbonTabDefinition> Tabs { get; } =
            new List<RibbonTabDefinition>
            {
                new RibbonTabDefinition(
                    "KUROACAD_TAB",
                    "KuroAcad",
                    new List<RibbonPanelDefinition>
                    {
                        new RibbonPanelDefinition(
                            "Main",
                            new List<RibbonButtonDefinition>
                            {
                                new RibbonButtonDefinition("KUROACAD_BTN_KTEMDAT", "Tem Dat", "KTemDat", DefaultIcon),
                                new RibbonButtonDefinition("KUROACAD_BTN_KGETTD", "Get TD", "KGetTD", DefaultIcon),
                                new RibbonButtonDefinition("KUROACAD_BTN_KINTERSECTION", "Intersection", "KIntersection", DefaultIcon),
                                new RibbonButtonDefinition("KUROACAD_BTN_KROAD", "Road", "KRoad", DefaultIcon),
                                new RibbonButtonDefinition("KUROACAD_BTN_KTKLD", "TKLD", "KTKLD", DefaultIcon),
                                new RibbonButtonDefinition("KUROACAD_BTN_KTRIMROAD", "Trim Road", "KTrimRoad", DefaultIcon),
                                new RibbonButtonDefinition("KUROACAD_BTN_KPALETTE", "Palette", "KPalette", DefaultIcon),
                                new RibbonButtonDefinition("KUROACAD_BTN_KCIRCLEWPF", "Circle WPF", "KCircleWPF", DefaultIcon),
                                new RibbonButtonDefinition("KUROACAD_BTN_KTKSDD", "TKSDD", "KTKSDD", DefaultIcon),
                            }),
                        new RibbonPanelDefinition(
                            "Data",
                            new List<RibbonButtonDefinition>
                            {
                                new RibbonButtonDefinition("KUROACAD_BTN_KEXPORTDATA", "Export Data", "KExportData", DefaultIcon),
                                new RibbonButtonDefinition("KUROACAD_BTN_KIMPORTDATA", "Import Data", "KImportData", DefaultIcon),
                            })
                    }),

                new RibbonTabDefinition(
                    "KUROACAD_TOOLS_TAB",
                    "Kuro Tools",
                    new List<RibbonPanelDefinition>
                    {
                        new RibbonPanelDefinition(
                            "Utilities",
                            new List<RibbonButtonDefinition>
                            {
                                new RibbonButtonDefinition("KUROACAD_BTN_SAMPLE", "Sample", "KSample", DefaultIcon),
                            })
                    }),
            };
    }
}