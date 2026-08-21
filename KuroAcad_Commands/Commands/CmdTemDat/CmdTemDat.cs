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
