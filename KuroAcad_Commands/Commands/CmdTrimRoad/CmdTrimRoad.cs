[assembly: CommandClass(typeof(KuroAcad.CmdTrimRoad))]

namespace KuroAcad
{
    internal class CmdTrimRoad
    {
        [CommandMethod("KTrimRoad")]
        public static void TrimRoad()
        {
            TrimRoadUtil.TrimRoad();
        }
    }
}