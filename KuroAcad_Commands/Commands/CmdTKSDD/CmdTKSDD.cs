[assembly: CommandClass(typeof(KuroAcad.CmdTKSDD))]

namespace KuroAcad
{
    internal class CmdTKSDD
    {
        [CommandMethod("KTKSDD")]
        public void KuroTKSDD()
        {
            TKSDDUtil.TKSDD();
        }
    }
}