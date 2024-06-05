using Autodesk.AutoCAD.Windows;
using KuroAcad.UI;

[assembly: ExtensionApplication(typeof(KuroAcad.ExtensionApplication))]

namespace KuroAcad
{
    internal class ExtensionApplication : IExtensionApplication
    {
        public void Initialize()
        {

            Application.Idle += OnIdle;

        }

        private void OnIdle(object? sender, EventArgs e)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                Application.Idle -= OnIdle;
                doc.Editor.WriteMessage("\nKuroAcad loaded.\n");
            }
        }

        public void Terminate()
        { }
    }

    public class CustomPaletteSet : PaletteSet
    {
        // constructor
        public CustomPaletteSet()
            : base("MyPalette", new Guid("{0dc9e6a7-1ae1-4ec4-b107-97ff8e0fd74d}"))
        {
            Palette = new demoWPF();
            //get Palette Uri
            var uri = new Uri("pack://application:,,,/KuroAcad;component/UI/demoWPF.xaml");
            Add("Tab 1", uri);
        }

        // public read only property
        public demoWPF Palette { get; }
    }

}
