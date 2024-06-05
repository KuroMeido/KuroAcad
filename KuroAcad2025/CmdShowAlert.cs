using KuroAcad.UI;
[assembly: CommandClass(typeof(KuroAcad.CmdShowAlert))]

namespace KuroAcad
{
    class CmdShowAlert
    {
        [CommandMethod  ("KUROSHOWALERT")]
        public void KuroShowAlert()
        {
            alertWPF alert = new alertWPF();
            alert.ShowDialog();
        }
    }
}
