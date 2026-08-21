using System.Linq;
using System.Windows.Input;
using Autodesk.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace KuroAcad
{
    internal static class KuroRibbon
    {
        private const string RibbonTabId = "KUROACAD_TAB";
        private const string RibbonTabTitle = "KuroAcad";
        private const string RibbonPanelTitle = "Main";

        internal static bool TryCreate()
        {
            var ribbon = ComponentManager.Ribbon;
            if (ribbon == null)
            {
                return false;
            }

            var existingTab = ribbon.Tabs.FirstOrDefault(t => t.Id == RibbonTabId);
            if (existingTab != null)
            {
                return true;
            }

            var tab = new RibbonTab
            {
                Id = RibbonTabId,
                Title = RibbonTabTitle
            };

            var panelSource = new RibbonPanelSource
            {
                Title = RibbonPanelTitle
            };

            var panel = new RibbonPanel
            {
                Source = panelSource
            };

            panelSource.Items.Add(CreateButton("KUROACAD_BTN_KTEMDAT", "Tem Dat", "KTemDat"));

            tab.Panels.Add(panel);
            ribbon.Tabs.Add(tab);

            return true;
        }

        private static RibbonButton CreateButton(string id, string text, string commandName)
        {
            return new RibbonButton
            {
                Id = id,
                Text = text,
                ShowText = true,
                ShowImage = false,
                Size = RibbonItemSize.Large,
                CommandHandler = new RibbonCommandHandler(),
                CommandParameter = commandName,
            };
        }

        private sealed class RibbonCommandHandler : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                if (parameter is not string command || string.IsNullOrWhiteSpace(command))
                {
                    return;
                }

                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    return;
                }

                // Temporary debug line
                doc.Editor.WriteMessage($"\n[KuroAcad] Ribbon button clicked. Command: {command}");

                doc.SendStringToExecute("\x03\x03" + command + " ", true, false, true);
            }
        }
    }
}