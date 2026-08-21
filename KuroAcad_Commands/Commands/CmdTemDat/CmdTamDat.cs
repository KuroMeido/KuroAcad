using KuroAcad.ModelItems;
using KuroAcad.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using RadioButton = System.Windows.Controls.RadioButton;

[assembly: CommandClass(typeof(KuroAcad.CmdTemDat))]
namespace KuroAcad
{
    public class CmdTemDat
    {
        [CommandMethod("KTemDat")]
        public void KuroTemDat()
        {
            TemDatUtil temDatUtil = new TemDatUtil();
            temDatUtil.Main();

        }
    }
}
