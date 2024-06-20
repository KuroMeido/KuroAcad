using System.Management;
using Autodesk.AutoCAD.Windows;
using KuroAcad.UI;
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

    internal class KeyGenerator
    {
        //method to check IsExpiredActive() As Boolean
        internal static bool IsExpiredActive()
        {
            DateTime activeDate = new DateTime();
            double countDay = 0;

            activeDate = DateTime.Parse("2025-01-01");

            countDay = (DateTime.Now - activeDate).TotalDays;
            if (countDay > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //method to check IsRightComputer(key As String) As Boolean
        internal static bool IsRightComputer(string strKey)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed) // Checking for hard drives
                {
                    var disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive.Name.Remove(1) + @":""");
                    disk.Get();
                    string serialNumber = disk["VolumeSerialNumber"].ToString();
                    // Check if the key is in the serial number
                    if (strKey.IndexOf(serialNumber, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                    // Check if the key is in the serial number but not at the beginning
                    else if (strKey.IndexOf(serialNumber, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        // Do nothing, just continue checking
                    }
                }
            }
            return false;
        }

    }

}
