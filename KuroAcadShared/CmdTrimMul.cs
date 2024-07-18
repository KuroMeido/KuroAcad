using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(KuroAcad.CmdTrimMul))]

namespace KuroAcad
{
    class CmdTrimMul
    {
        [CommandMethod("KuroIntersect")]
        public static void KuroIntersect()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            Editor ed = doc.Editor;

            //Start a transaction
            Transaction
            // First Polyline
            Polyline pl1 = new Polyline();
            pl1.AddVertexAt(0, new Point2d(600097.3438, 494820.3637), 0, 0, 0);
            pl1.AddVertexAt(0, new Point2d(600101.7191, 494825.6028), 0, 0, 0);
            pl1.AddVertexAt(0, new Point2d(600107.4447, 494835.9176), 0, 0, 0);

            // 2nd  Polyline
            Polyline pl2 = new Polyline();
            pl2.AddVertexAt(0, new Point2d(600110.2043, 494822.9429), 0, 0, 0);
            pl2.AddVertexAt(0, new Point2d(600096.5547, 494827.2256), 0, 0, 0);

            // Intersection Points Collection 
            Point3dCollection intPoints = new Point3dCollection();
            pl1.IntersectWith(pl2, Intersect.OnBothOperands, intPoints, IntPtr.Zero, IntPtr.Zero);

            ed.WriteMessage("\nNo. Of Intersection Points: {0}", intPoints.Count);
            for (int i = 0; i < intPoints.Count; i++)
            {
                ed.WriteMessage("\n Point No. {0} X:{1} Y:{2}", i.ToString(), intPoints[i].X, intPoints[i].Y);
            }
        }


    }
}
