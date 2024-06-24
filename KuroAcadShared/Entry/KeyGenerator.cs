using System.Management;

namespace KuroAcad
{
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
