using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(KuroAcad.Commands.CmdMakeFillet.CmdSkewRoadFillet))]

namespace KuroAcad.Commands.CmdMakeFillet
{
    internal class CmdSkewRoadFillet
    {
        private const double Tol = 1e-6;
        private double FilletRadius = 6.0;

        [CommandMethod("MakeRoadFillet")]
        public void MakeRoadFilletSkew4()
        {
            Document doc = AcAp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptSelectionOptions pso = new PromptSelectionOptions
            {
                MessageForAdding = "\nSelect exactly 4 lines (2 pairs of parallel lines): "
            };

            PromptDoubleOptions radiusOpts = new PromptDoubleOptions("\nNhap FilletRadius: ");
            radiusOpts.AllowNegative = false;
            radiusOpts.AllowZero = false;
            radiusOpts.DefaultValue = 6.0;
            radiusOpts.UseDefaultValue = true;

            FilletRadius = ed.GetDouble(radiusOpts).Value;

            PromptSelectionResult psr = ed.GetSelection(pso);
            if (psr.Status != PromptStatus.OK)
                return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                List<Line> selectedLines = new List<Line>();

                foreach (ObjectId id in psr.Value.GetObjectIds())
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForWrite) as Entity;
                    if (ent is Line ln)
                        selectedLines.Add(ln);
                }

                if (selectedLines.Count != 4)
                {
                    ed.WriteMessage($"\nPlease select exactly 4 lines. Current: {selectedLines.Count}");
                    return;
                }

                // Chia 4 line thành 2 nhóm song song
                if (!TryGroupIntoParallelPairs(selectedLines, out List<Line> familyA, out List<Line> familyB))
                {
                    ed.WriteMessage("\nCannot classify 4 selected lines into 2 pairs of parallel lines.");
                    return;
                }

                // Kiểm tra 2 họ line không được song song với nhau
                if (AreParallel(GetDirection(familyA[0]), GetDirection(familyB[0])))
                {
                    ed.WriteMessage("\nThe two line families are parallel to each other. No intersections.");
                    return;
                }

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr =
                    (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                StringBuilder debug = new StringBuilder();
                debug.AppendLine("=== MakeRoadFilletSkew4 Debug ===");
                debug.AppendLine($"Selected line count: {selectedLines.Count}");
                debug.AppendLine();

                // Tạo 4 corner từ giao điểm giữa familyA x familyB
                List<CornerInfo> corners = new List<CornerInfo>();
                foreach (Line a in familyA)
                {
                    foreach (Line b in familyB)
                    {
                        Point3d? ip = GetIntersection(a, b);
                        if (!ip.HasValue)
                        {
                            ed.WriteMessage("\nSome line pairs do not intersect as finite segments.");
                            return;
                        }

                        corners.Add(new CornerInfo
                        {
                            Intersection = ip.Value,
                            LineA = a,
                            LineB = b
                        });
                    }
                }

                if (corners.Count != 4)
                {
                    ed.WriteMessage("\nExpected 4 corners but failed to build them.");
                    return;
                }

                // Tâm vùng 4-giao-điểm, dùng để xác định hướng fillet ra ngoài
                Point3d centerOfShape = GetAveragePoint(corners.Select(c => c.Intersection).ToList());

                debug.AppendLine("Corners:");
                foreach (CornerInfo c in corners)
                {
                    debug.AppendLine($"- {FormatPoint(c.Intersection)}");
                }
                debug.AppendLine();

                // Với mỗi corner:
                // - xác định 2 tia đi ra ngoài
                // - tính 2 điểm tiếp xúc trên 2 line
                // - tính tâm arc
                // - tạo arc
                foreach (CornerInfo corner in corners)
                {
                    if (!TryBuildFilletAtCorner(corner, centerOfShape, FilletRadius, out FilletAtCorner fillet))
                    {
                        debug.AppendLine($"Skip corner at {FormatPoint(corner.Intersection)}");
                        continue;
                    }

                    corner.Fillet = fillet;
                }

                // Gom các điểm trim theo từng line
                Dictionary<ObjectId, List<Point3d>> trimPointsByLine = new Dictionary<ObjectId, List<Point3d>>();

                foreach (CornerInfo corner in corners)
                {
                    if (corner.Fillet == null)
                        continue;

                    AddTrimPoint(trimPointsByLine, corner.LineA.ObjectId, corner.Fillet.TangentOnA);
                    AddTrimPoint(trimPointsByLine, corner.LineB.ObjectId, corner.Fillet.TangentOnB);
                }

                List<Line> createdLines = new List<Line>();

                // Mỗi line phải có đúng 2 điểm trim
                foreach (Line oldLine in selectedLines)
                {
                    if (!trimPointsByLine.TryGetValue(oldLine.ObjectId, out List<Point3d> trimPts) || trimPts.Count != 2)
                    {
                        ed.WriteMessage($"\nLine does not have exactly 2 trim points: {FormatLine(oldLine)}");
                        return;
                    }

                    // Một line cũ sẽ được thay bởi 2 line mới:
                    // [Start -> trim gần Start] và [End -> trim gần End]
                    Point3d tp1 = trimPts[0];
                    Point3d tp2 = trimPts[1];

                    Point3d nearStart = oldLine.StartPoint.DistanceTo(tp1) <= oldLine.StartPoint.DistanceTo(tp2) ? tp1 : tp2;
                    Point3d nearEnd = oldLine.EndPoint.DistanceTo(tp1) <= oldLine.EndPoint.DistanceTo(tp2) ? tp1 : tp2;

                    if (oldLine.StartPoint.DistanceTo(nearStart) > Tol)
                    {
                        Line l1 = new Line(oldLine.StartPoint, nearStart);
                        l1.ColorIndex = 3;
                        btr.AppendEntity(l1);
                        tr.AddNewlyCreatedDBObject(l1, true);
                        createdLines.Add(l1);

                        debug.AppendLine($"Trimmed A: {FormatPoint(oldLine.StartPoint)} -> {FormatPoint(nearStart)}");
                    }

                    if (oldLine.EndPoint.DistanceTo(nearEnd) > Tol)
                    {
                        Line l2 = new Line(nearEnd, oldLine.EndPoint);
                        l2.ColorIndex = 3;
                        btr.AppendEntity(l2);
                        tr.AddNewlyCreatedDBObject(l2, true);
                        createdLines.Add(l2);

                        debug.AppendLine($"Trimmed B: {FormatPoint(nearEnd)} -> {FormatPoint(oldLine.EndPoint)}");
                    }
                }

                List<Arc> createdArcs = new List<Arc>();

                foreach (CornerInfo corner in corners)
                {
                    if (corner.Fillet == null)
                        continue;

                    double startAngle = Math.Atan2(
                        corner.Fillet.TangentOnA.Y - corner.Fillet.Center.Y,
                        corner.Fillet.TangentOnA.X - corner.Fillet.Center.X);

                    double endAngle = Math.Atan2(
                        corner.Fillet.TangentOnB.Y - corner.Fillet.Center.Y,
                        corner.Fillet.TangentOnB.X - corner.Fillet.Center.X);

                    GetMinorArcAngles(startAngle, endAngle, out double sa, out double ea);

                    Arc arc = new Arc(corner.Fillet.Center, FilletRadius, sa, ea);
                    arc.ColorIndex = 2;
                    btr.AppendEntity(arc);
                    tr.AddNewlyCreatedDBObject(arc, true);
                    createdArcs.Add(arc);

                    debug.AppendLine(
                        $"Arc: center={FormatPoint(corner.Fillet.Center)}, " +
                        $"tanA={FormatPoint(corner.Fillet.TangentOnA)}, " +
                        $"tanB={FormatPoint(corner.Fillet.TangentOnB)}");
                }

                // Xóa line cũ
                foreach (Line oldLine in selectedLines)
                {
                    debug.AppendLine($"Deleted: {FormatLine(oldLine)}");
                    oldLine.Erase();
                }

                debug.AppendLine();
                debug.AppendLine($"Summary: {createdLines.Count} trimmed lines created.");
                debug.AppendLine($"Summary: {createdArcs.Count} fillet arcs created.");
                debug.AppendLine($"Summary: {selectedLines.Count} old lines deleted.");

                MessageBox.Show(debug.ToString(), "Debug Fillet");
                tr.Commit();
            }
        }

        /// <summary>
        /// Cố gắng chia 4 line thành đúng 2 nhóm song song, mỗi nhóm 2 line.
        /// </summary>
        private static bool TryGroupIntoParallelPairs(
            List<Line> lines,
            out List<Line> familyA,
            out List<Line> familyB)
        {
            familyA = null;
            familyB = null;

            // Chỉ có 3 cách bắt cặp 4 phần tử thành 2 cặp:
            // (0,1)(2,3), (0,2)(1,3), (0,3)(1,2)
            int[][] pairings =
            {
                new[] { 0, 1, 2, 3 },
                new[] { 0, 2, 1, 3 },
                new[] { 0, 3, 1, 2 }
            };

            foreach (int[] p in pairings)
            {
                Line a1 = lines[p[0]];
                Line a2 = lines[p[1]];
                Line b1 = lines[p[2]];
                Line b2 = lines[p[3]];

                if (AreParallel(GetDirection(a1), GetDirection(a2)) &&
                    AreParallel(GetDirection(b1), GetDirection(b2)) &&
                    !AreParallel(GetDirection(a1), GetDirection(b1)))
                {
                    familyA = new List<Line> { a1, a2 };
                    familyB = new List<Line> { b1, b2 };
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tạo fillet tại 1 corner giữa 2 line cắt nhau.
        /// Fillet được tạo về phía ngoài vùng trung tâm.
        /// </summary>
        private static bool TryBuildFilletAtCorner(
            CornerInfo corner,
            Point3d centerOfShape,
            double radius,
            out FilletAtCorner fillet)
        {
            fillet = null;

            Point3d ip = corner.Intersection;

            // Trên mỗi line đi qua corner, có 2 hướng ngược nhau.
            // Ta chọn hướng "đi ra xa centerOfShape" để fillet nằm ngoài.
            if (!TryGetOutwardDirection(corner.LineA, ip, centerOfShape, out Vector3d dirA))
                return false;

            if (!TryGetOutwardDirection(corner.LineB, ip, centerOfShape, out Vector3d dirB))
                return false;

            // Góc giữa 2 tia outward
            double angle = dirA.GetAngleTo(dirB);
            if (angle < Tol || Math.Abs(angle - Math.PI) < Tol)
                return false;

            // Khoảng lùi từ giao điểm đến điểm tiếp xúc trên mỗi line
            double offsetToTangent = radius / Math.Tan(angle / 2.0);

            Point3d tangentA = ip + dirA * offsetToTangent;
            Point3d tangentB = ip + dirB * offsetToTangent;

            // Tâm fillet nằm trên phân giác góc
            Vector3d bisector = (dirA + dirB);
            if (bisector.Length < Tol)
                return false;

            bisector = bisector.GetNormal();

            double offsetToCenter = radius / Math.Sin(angle / 2.0);
            Point3d center = ip + bisector * offsetToCenter;

            fillet = new FilletAtCorner
            {
                Center = center,
                TangentOnA = tangentA,
                TangentOnB = tangentB
            };

            return true;
        }

        /// <summary>
        /// Chọn hướng trên line sao cho đi từ giao điểm ra xa tâm vùng.
        /// </summary>
        private static bool TryGetOutwardDirection(Line line, Point3d intersection, Point3d centerOfShape, out Vector3d outwardDir)
        {
            outwardDir = Vector3d.XAxis;

            Point3d other1 = line.StartPoint;
            Point3d other2 = line.EndPoint;

            Vector3d d1 = other1 - intersection;
            Vector3d d2 = other2 - intersection;

            bool valid1 = d1.Length > Tol;
            bool valid2 = d2.Length > Tol;

            if (!valid1 && !valid2)
                return false;

            if (valid1) d1 = d1.GetNormal();
            if (valid2) d2 = d2.GetNormal();

            Vector3d toCenter = centerOfShape - intersection;

            // Hướng outward là hướng có dot với vector tới center nhỏ hơn
            // tức là nó đi ngược phía center nhiều hơn.
            if (valid1 && valid2)
            {
                double dot1 = d1.DotProduct(toCenter);
                double dot2 = d2.DotProduct(toCenter);
                outwardDir = dot1 <= dot2 ? d1 : d2;
                return true;
            }

            outwardDir = valid1 ? d1 : d2;
            return true;
        }

        private static void AddTrimPoint(Dictionary<ObjectId, List<Point3d>> dict, ObjectId id, Point3d pt)
        {
            if (!dict.ContainsKey(id))
                dict[id] = new List<Point3d>();

            if (!dict[id].Any(p => p.DistanceTo(pt) < Tol))
                dict[id].Add(pt);
        }

        private static Point3d? GetIntersection(Line l1, Line l2)
        {
            Point3dCollection pts = new Point3dCollection();
            l1.IntersectWith(l2, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);

            if (pts.Count == 0)
                return null;

            return pts[0];
        }

        private static Vector3d GetDirection(Line line)
        {
            Vector3d dir = line.EndPoint - line.StartPoint;
            return dir.Length < Tol ? Vector3d.XAxis : dir.GetNormal();
        }

        private static bool AreParallel(Vector3d v1, Vector3d v2)
        {
            return v1.CrossProduct(v2).Length < 1e-6;
        }

        private static Point3d GetAveragePoint(List<Point3d> pts)
        {
            double x = pts.Average(p => p.X);
            double y = pts.Average(p => p.Y);
            double z = pts.Average(p => p.Z);
            return new Point3d(x, y, z);
        }

        private static void GetMinorArcAngles(double a1, double a2, out double startAngle, out double endAngle)
        {
            a1 = NormalizeAngle(a1);
            a2 = NormalizeAngle(a2);

            double ccw = a2 - a1;
            if (ccw < 0) ccw += Math.PI * 2.0;

            if (ccw <= Math.PI)
            {
                startAngle = a1;
                endAngle = a2;
            }
            else
            {
                startAngle = a2;
                endAngle = a1;
            }
        }

        private static double NormalizeAngle(double a)
        {
            while (a < 0) a += Math.PI * 2.0;
            while (a >= Math.PI * 2.0) a -= Math.PI * 2.0;
            return a;
        }

        private static string FormatPoint(Point3d pt)
        {
            return $"({pt.X:F6}, {pt.Y:F6}, {pt.Z:F6})";
        }

        private static string FormatLine(Line ln)
        {
            return $"({ln.StartPoint.X:F6}, {ln.StartPoint.Y:F6}, {ln.StartPoint.Z:F6}) -> " +
                   $"({ln.EndPoint.X:F6}, {ln.EndPoint.Y:F6}, {ln.EndPoint.Z:F6})";
        }

        private class CornerInfo
        {
            public Point3d Intersection { get; set; }
            public Line LineA { get; set; }
            public Line LineB { get; set; }
            public FilletAtCorner Fillet { get; set; }
        }

        private class FilletAtCorner
        {
            public Point3d Center { get; set; }
            public Point3d TangentOnA { get; set; }
            public Point3d TangentOnB { get; set; }
        }
    }
}
