using KuroAcad.UI;
using Microsoft.Data.SqlClient;

[assembly: CommandClass(typeof(KuroAcad.DBUtil))]
namespace KuroAcad
{
    class DBUtil
    {
        [CommandMethod("DBRun")]
        public static void DBRun()
        {
            SqlWPF window = new SqlWPF();
            window.ShowDialog();

            if (window.DialogResult == true)
            {
                DBLoadUtil db = new DBLoadUtil();
                string result = db.LoadLines();
                window.textBlockResult.Text = result;
            }
        }
        public static SqlConnection GetConnection()
        {
            string connStr = Settings1.Default.connstr;
            SqlConnection conn = new SqlConnection(connStr);
            return conn;
        }
    }
}
