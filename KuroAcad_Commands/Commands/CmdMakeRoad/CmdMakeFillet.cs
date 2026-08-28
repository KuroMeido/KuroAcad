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

[assembly: CommandClass(typeof(KuroAcad.Commands.CmdMakeFillet.CmdLayerRoadProcessor))]

namespace KuroAcad.Commands.CmdMakeFillet
{
    internal class CmdLayerRoadProcessor
    {
        private const double Tol = 1e-6;
        private const double FilletRadius = 6.0;

        [CommandMethod("MakeRoadFillet4")]
        public void MakeRoadFillet4()
        {
            Document doc = AcAp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = "\nSelect 4 lines: ";
            PromptSelectionResult psr = ed.GetSelection(pso);
            if (psr.Status != PromptStatus.OK) return;

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
                    ed.WriteMessage($"\nPlease select exactly 4 lines. Current selected line count: {selectedLines.Count}");
                    return;
                }

                List<Line> horizontals = selectedLines.Where(IsHorizontal).OrderByDescending(x => x.StartPoint.Y).ToList();
                List<Line> verticals = selectedLines.Where(IsVertical).OrderByDescending(x => x.StartPoint.X).ToList();

                if (horizontals.Count != 2 || verticals.Count != 2)
                {
                    ed.WriteMessage("\nNeed exactly 2 horizontal lines and 2 vertical lines.");
                    return;
                }

                Line hTop = horizontals.OrderByDescending(l => l.StartPoint.Y).First();
                Line hBottom = horizontals.OrderBy(l => l.StartPoint.Y).First();
                Line vRight = verticals.OrderByDescending(l => l.StartPoint.X).First();
                Line vLeft = verticals.OrderBy(l => l.StartPoint.X).First();

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                StringBuilder debug = new StringBuilder();
                debug.AppendLine($"Selected line count: {selectedLines.Count}");
                debug.AppendLine();

                List<CornerInfo> corners = new List<CornerInfo>();

                AddCorner(corners, hTop, vRight, "TopRight");
                AddCorner(corners, hTop, vLeft, "TopLeft");
                AddCorner(corners, hBottom, vRight, "BottomRight");
                AddCorner(corners, hBottom, vLeft, "BottomLeft");

                foreach (var c in corners)
                {
                    debug.AppendLine($"Intersect {c.Name}: {FormatPoint(c.Intersection)}");
                }

                debug.AppendLine();

                List<TrimmedLineInfo> finalSegments = new List<TrimmedLineInfo>();

                foreach (Line baseLine in selectedLines)
                {
                    List<Point3d> ipsOnLine = corners
                        .Select(c => c.Intersection)
                        .Where(p => IsPointOnLine(baseLine, p))
                        .ToList();

                    if (ipsOnLine.Count != 2)
                    {
                        MessageBox.Show(
                            $"Line does not contain exactly 2 intersections:\n{FormatLine(baseLine)}",
                            "Debug Fillet");
                        return;
                    }

                    Point3d ip1 = ipsOnLine[0];
                    Point3d ip2 = ipsOnLine[1];

                    Point3d sp = baseLine.StartPoint;
                    Point3d ep = baseLine.EndPoint;

                    Point3d startTarget = sp.DistanceTo(ip1) <= sp.DistanceTo(ip2) ? ip1 : ip2;
                    Point3d endTarget = ep.DistanceTo(ip1) <= ep.DistanceTo(ip2) ? ip1 : ip2;

                    finalSegments.Add(new TrimmedLineInfo
                    {
                        BaseLine = baseLine,
                        OriginalEnd = sp,
                        Intersection = startTarget,
                        IsHorizontal = IsHorizontal(baseLine),
                        IsVertical = IsVertical(baseLine)
                    });

                    finalSegments.Add(new TrimmedLineInfo
                    {
                        BaseLine = baseLine,
                        OriginalEnd = ep,
                        Intersection = endTarget,
                        IsHorizontal = IsHorizontal(baseLine),
                        IsVertical = IsVertical(baseLine)
                    });
                }

                List<Line> createdLines = new List<Line>();

                foreach (TrimmedLineInfo seg in finalSegments)
                {
                    Point3d newEnd = MoveFromIntersectionTowardOriginalEnd(seg.Intersection, seg.OriginalEnd, FilletRadius);

                    if (newEnd.DistanceTo(seg.OriginalEnd) < Tol)
                        continue;

                    Line ln = new Line(seg.OriginalEnd, newEnd);
                    ln.ColorIndex = 3;
                    btr.AppendEntity(ln);
                    tr.AddNewlyCreatedDBObject(ln, true);
                    createdLines.Add(ln);
                    seg.CreatedLine = ln;

                    debug.AppendLine($"Trimmed line: {FormatPoint(seg.OriginalEnd)} -> {FormatPoint(newEnd)}");
                }

                debug.AppendLine();

                List<Arc> createdArcs = new List<Arc>();

                foreach (CornerInfo corner in corners)
                {
                    TrimmedLineInfo hSeg = finalSegments.FirstOrDefault(s =>
                        s.IsHorizontal &&
                        s.Intersection.DistanceTo(corner.Intersection) < Tol &&
                        s.CreatedLine != null);

                    TrimmedLineInfo vSeg = finalSegments.FirstOrDefault(s =>
                        s.IsVertical &&
                        s.Intersection.DistanceTo(corner.Intersection) < Tol &&
                        s.CreatedLine != null);

                    if (hSeg == null || vSeg == null)
                    {
                        debug.AppendLine($"Skip fillet at {corner.Name} because missing segment.");
                        continue;
                    }

                    Point3d pH = hSeg.CreatedLine.EndPoint;
                    Point3d pV = vSeg.CreatedLine.EndPoint;

                    Point3d center = GetOuterFilletCenter(corner.Name, corner.Intersection, FilletRadius);

                    double a1 = Math.Atan2(pH.Y - center.Y, pH.X - center.X);
                    double a2 = Math.Atan2(pV.Y - center.Y, pV.X - center.X);

                    double startAngle, endAngle;
                    GetMinorArcAngles(a1, a2, out startAngle, out endAngle);

                    Arc arc = new Arc(center, FilletRadius, startAngle, endAngle);
                    arc.ColorIndex = 2;
                    btr.AppendEntity(arc);
                    tr.AddNewlyCreatedDBObject(arc, true);
                    createdArcs.Add(arc);

                    debug.AppendLine($"Fillet arc {corner.Name}: center {FormatPoint(center)}, start {FormatPoint(pH)}, end {FormatPoint(pV)}");
                }

                debug.AppendLine();

                foreach (Line oldLine in selectedLines)
                {
                    debug.AppendLine($"Deleted selected line: {FormatLine(oldLine)}");
                    oldLine.Erase();
                }

                debug.AppendLine();
                debug.AppendLine($"Summary: {createdLines.Count} trimmed lines created.");
                debug.AppendLine($"Summary: {createdArcs.Count} fillet arcs created.");
                debug.AppendLine($"Summary: {selectedLines.Count} selected lines deleted.");

                MessageBox.Show(debug.ToString(), "Debug Fillet");

                tr.Commit();
            }
        }

        private static void AddCorner(List<CornerInfo> corners, Line h, Line v, string name)
        {
            Point3dCollection pts = new Point3dCollection();
            h.IntersectWith(v, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
            if (pts.Count > 0)
            {
                corners.Add(new CornerInfo
                {
                    Name = name,
                    Intersection = pts[0],
                    Horizontal = h,
                    Vertical = v
                });
            }
        }

        private static Point3d GetOuterFilletCenter(string cornerName, Point3d ip, double r)
        {
            switch (cornerName)
            {
                case "TopRight":
                    return new Point3d(ip.X + r, ip.Y + r, 0);
                case "TopLeft":
                    return new Point3d(ip.X - r, ip.Y + r, 0);
                case "BottomRight":
                    return new Point3d(ip.X + r, ip.Y - r, 0);
                case "BottomLeft":
                    return new Point3d(ip.X - r, ip.Y - r, 0);
                default:
                    return ip;
            }
        }

        private static Point3d MoveFromIntersectionTowardOriginalEnd(Point3d intersection, Point3d originalEnd, double dist)
        {
            Vector3d dir = originalEnd - intersection;
            if (dir.Length < Tol)
                return intersection;

            dir = dir.GetNormal() * dist;
            return intersection + dir;
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

        private static bool IsHorizontal(Line line)
        {
            return Math.Abs(line.StartPoint.Y - line.EndPoint.Y) < Tol;
        }

        private static bool IsVertical(Line line)
        {
            return Math.Abs(line.StartPoint.X - line.EndPoint.X) < Tol;
        }

        private static bool IsPointOnLine(Line line, Point3d pt)
        {
            double dist1 = line.StartPoint.DistanceTo(pt);
            double dist2 = line.EndPoint.DistanceTo(pt);
            double lineLen = line.StartPoint.DistanceTo(line.EndPoint);
            return Math.Abs((dist1 + dist2) - lineLen) < 1e-4;
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
            public string Name { get; set; }
            public Point3d Intersection { get; set; }
            public Line Horizontal { get; set; }
            public Line Vertical { get; set; }
        }

        private class TrimmedLineInfo
        {
            public Line BaseLine { get; set; }
            public Point3d OriginalEnd { get; set; }
            public Point3d Intersection { get; set; }
            public bool IsHorizontal { get; set; }
            public bool IsVertical { get; set; }
            public Line CreatedLine { get; set; }
        }
    }
}
