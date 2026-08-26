using System.Linq;
using Autodesk.Windows;

namespace KuroAcad
{
    internal sealed class KuroRibbonService : IKuroRibbonService
    {
        private const string RibbonTabId = "KUROACAD_TAB";
        private const string RibbonTabTitle = "KuroAcad";
        private const string MainPanel = "Main";
        private const string DataPanel = "Data";

        private readonly IImageLoader imageLoader;
        private readonly ICommandHandlerFactory commandHandlerFactory;
        private bool isSubscribedToItemInitialized;

        public KuroRibbonService(IImageLoader imageLoader, IAcadCommandExecutor commandExecutor)
        {
            this.imageLoader = imageLoader;
            commandHandlerFactory = new RibbonCommandHandlerFactory(commandExecutor);
        }

        public void Create()
        {
            if (ComponentManager.Ribbon == null)
            {
                SubscribeToRibbonInitialization();
                return;
            }

            CreateOrReplaceRibbon();
        }

        private void SubscribeToRibbonInitialization()
        {
            if (isSubscribedToItemInitialized)
            {
                return;
            }

            ComponentManager.ItemInitialized += OnComponentManagerItemInitialized;
            isSubscribedToItemInitialized = true;
        }

        private void OnComponentManagerItemInitialized(object? sender, RibbonItemEventArgs e)
        {
            if (ComponentManager.Ribbon == null)
            {
                return;
            }

            CreateOrReplaceRibbon();
            ComponentManager.ItemInitialized -= OnComponentManagerItemInitialized;
            isSubscribedToItemInitialized = false;
        }
        private void CreateOrReplaceRibbon()
        {
            var ribbonControl = ComponentManager.Ribbon;
            if (ribbonControl == null)
            {
                return;
            }

            foreach (var tabData in KuroRibbonButtons.Tabs)
            {
                var existingTab = ribbonControl.Tabs
                    .FirstOrDefault(tab => tab.Id == tabData.Id || tab.Title == tabData.Title);

                if (existingTab != null)
                {
                    ribbonControl.Tabs.Remove(existingTab);
                }

                var tab = new RibbonTab
                {
                    Id = tabData.Id,
                    Title = tabData.Title
                };

                ribbonControl.Tabs.Add(tab);

                foreach (var panelData in tabData.Panels)
                {
                    var panelSource = new RibbonPanelSource
                    {
                        Title = panelData.Title
                    };

                    var panel = new RibbonPanel
                    {
                        Source = panelSource
                    };

                    tab.Panels.Add(panel);

                    foreach (var buttonData in panelData.Buttons)
                    {
                        panelSource.Items.Add(CreateButton(buttonData));
                    }
                }
            }
        }
        private RibbonButton CreateButton(RibbonButtonDefinition buttonData)
        {
            var icon = imageLoader.Load(buttonData.IconPath);

            return new RibbonButton
            {
                Id = buttonData.Id,
                Text = buttonData.Text,
                ShowText = true,
                ShowImage = icon != null,
                Image = icon,
                LargeImage = icon,
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                CommandHandler = commandHandlerFactory.Create(),
                CommandParameter = buttonData.CommandName
            };
        }
    }
}