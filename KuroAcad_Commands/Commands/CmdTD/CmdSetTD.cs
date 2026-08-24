[assembly: CommandClass(typeof(KuroAcad.CmdSetTD))]
namespace KuroAcad
{
    internal class CmdSetTD
    {
        [CommandMethod("KSetTD")]
        public void KuroSetTD()
        {
            SetTDUtil.SetTD();
        }
    }
}
