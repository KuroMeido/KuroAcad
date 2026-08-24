using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace KuroAcad
{
    internal static class KuroRibbon
    {
        private const string RibbonTabId = "KUROACAD_TAB";
        private const string RibbonTabTitle = "KuroAcad";

        public static void CreateMyRibbon()
        {
            if (ComponentManager.Ribbon == null)
            {
                ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
            }
            else
            {
                CreateRibbon();
            }
        }

        private static void ComponentManager_ItemInitialized(object? sender, RibbonItemEventArgs e)
        {
            if (ComponentManager.Ribbon != null)
            {
                CreateRibbon();
                ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
            }
        }

        private static void CreateRibbon()
        {
            var ribbonControl = ComponentManager.Ribbon;
            if (ribbonControl == null)
            {
                return;
            }

            var existingTab = ribbonControl.Tabs
                .FirstOrDefault(t => t.Id == RibbonTabId || t.Title == RibbonTabTitle);

            if (existingTab != null)
            {
                ribbonControl.Tabs.Remove(existingTab);
            }

            var tabNew = new RibbonTab
            {
                Title = RibbonTabTitle,
                Id = RibbonTabId
            };

            ribbonControl.Tabs.Add(tabNew);

            var panelSource = new RibbonPanelSource
            {
                Title = "Main"
            };

            var panel = new RibbonPanel
            {
                Source = panelSource
            };

            tabNew.Panels.Add(panel);

            foreach (var item in KuroRibbonButtons.MainPanel)
            {
                panelSource.Items.Add(CreateButton(item.Id, item.Text, item.CommandName, item.IconPath));
            }
        }

        private static RibbonButton CreateButton(string id, string text, string commandName, string iconPath)
        {
            var icon = LoadImage(iconPath);

            return new RibbonButton
            {
                Id = id,
                Text = text,
                ShowText = true,
                ShowImage = icon != null,
                Image = icon,
                LargeImage = icon,
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                CommandHandler = new RibbonCommandHandler(),
                CommandParameter = commandName
            };
        }

        private static ImageSource? LoadImage(string iconPath)
        {
            if (string.IsNullOrWhiteSpace(iconPath))
            {
                return null;
            }

            try
            {
                string assemblyName = typeof(KuroRibbon).Assembly.GetName().Name;
                var uri = new Uri($"pack://application:,,,/{assemblyName};component/{iconPath}", UriKind.Absolute);
                return new BitmapImage(uri);
            }
            catch
            {
                return null;
            }
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
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    return;
                }

                string? command = null;

                if (parameter is RibbonButton button)
                {
                    command = button.CommandParameter as string;
                }
                else if (parameter is string cmdText)
                {
                    command = cmdText;
                }

                if (string.IsNullOrWhiteSpace(command))
                {
                    doc.Editor.WriteMessage("\n[KuroAcad] Ribbon click received but no command found.");
                    return;
                }

                command = command.Replace("^C", "", StringComparison.OrdinalIgnoreCase).Trim();

                doc.Editor.WriteMessage($"\n[KuroAcad] Ribbon clicked. Running: {command}");
                doc.SendStringToExecute("\x03\x03" + command + " ", true, false, true);
            }
        }
    }
}