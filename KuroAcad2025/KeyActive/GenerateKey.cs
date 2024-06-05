using System.IO;

namespace KuroAcad.KeyActive
{
    public class KeyGenerator
    {
        
        //method to check IsExpiredActive() As Boolean
        private bool IsExpiredActive()
        {
            DateTime activeDate = new DateTime();
            double countDay = 0;

            activeDate = DateTime.Parse("2025-01-01");

            countDay = (DateTime.Now - activeDate).TotalDays;
            if (countDay > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //method to check IsRightKey(key As String) As Boolean
        private bool IsRightKey(string key)
        {
            //get drive series
            string rightKey = Path.GetPathRoot(Environment.SystemDirectory).Substring(0, 1);
            if (IsExpiredActive())
            {
                return false;
            }

            if (key == rightKey)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
