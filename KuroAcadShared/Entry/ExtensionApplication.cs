using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: ExtensionApplication(typeof(KuroAcad.ExtensionApplication))]

namespace KuroAcad
{
    internal class ExtensionApplication : IExtensionApplication
    {
        public void Initialize()
        {
            if (!KeyGenerator.IsRightComputer("18DBE8E0"))
            {
                Application.ShowAlertDialog("The key is not right");
                //not load the application
                Application.Quit();
            }
            if (KeyGenerator.IsExpiredActive())
            {
                Application.ShowAlertDialog("The key is out of date");
                //not load the application
                Application.Quit();
            }
            else
            {
                Application.Idle += OnIdle;
            }
            //VinxDemo vi = new VinxDemo(50, 100);
            //vi.VinxDemoMethod();
            //vi.TinhToan();
            //vi.Xulynoidung("a","b");
            //double chuvi = vi.KQ_ChuVi;
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

}
