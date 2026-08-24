[assembly: CommandClass(typeof(KuroAcad.CmdTKLD))]

namespace KuroAcad
{
    internal class CmdTKLD
    {
        [CommandMethod("KTKLD")]
        public void KuroTKLD()
        {
            TKLDUtil.TKLD();
        }
    }
}
