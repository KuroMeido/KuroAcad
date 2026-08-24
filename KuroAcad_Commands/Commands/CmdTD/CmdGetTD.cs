[assembly: CommandClass(typeof(KuroAcad.CmdGetTD))]
namespace KuroAcad
{
    internal class CmdGetTD
    {
        [CommandMethod("KGetTD")]
        public void KuroGetTD()
        {
            GetTDUtil.GetTD();
        }
    }
}
