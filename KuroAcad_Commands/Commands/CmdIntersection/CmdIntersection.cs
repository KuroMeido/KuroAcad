using KuroAcad.Lib.Utils.MainCommand.Intersection;

[assembly: CommandClass(typeof(KuroAcad.CmdIntersection))]
namespace KuroAcad
{
    class CmdIntersection
    {
        [CommandMethod("KIntersection")]
        public static void Intersection()
        {
            IntersectionUtils intersectionUtils = new IntersectionUtils();
            intersectionUtils.Main();

        }

    }
}
