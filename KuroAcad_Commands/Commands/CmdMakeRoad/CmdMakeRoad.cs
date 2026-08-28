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

[assembly: CommandClass(typeof(KuroAcad.Commands.CmdMakeRoad.CmdLayerRoadProcessor))]

namespace KuroAcad.Commands.CmdMakeRoad
{
    internal class CmdLayerRoadProcessor
    {
        private const double Tol = 1e-6;
        private const double CircleRadius = 5.0;
        private const double ChamferDist = 4.0;

        [CommandMethod("Mark4Intersections")]
        public void Mark4Intersections()
        {
            Document doc = AcAp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = "\nSelect 4 lines: ";
            PromptSelectionResult psr = ed.GetSelection(pso);

            if (psr.Status != PromptStatus.OK)
                return;

            ObjectId[] ids = psr.Value.GetObjectIds();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                List<Line> selectedLines = new List<Line>();

                foreach (ObjectId id in ids)
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    if (ent is Line ln)
                        selectedLines.Add(ln);
                }

                if (selectedLines.Count != 4)
                {
                    ed.WriteMessage($"\nPlease select exactly 4 lines. Current selected line count: {selectedLines.Count}");
                    return;
                }

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                List<Point3d> uniqueIntersections = new List<Point3d>();
                List<Line> createdLines = new List<Line>();
                List<Circle> createdCircles = new List<Circle>();
                List<Line> keptLines = new List<Line>();
                List<Line> diagonalLines = new List<Line>();

                StringBuilder debug = new StringBuilder();
                debug.AppendLine($"Selected line count: {selectedLines.Count}");
                debug.AppendLine();

                // 1) Tìm giao điểm H-V và vẽ circle
                for (int i = 0; i < selectedLines.Count; i++)
                {
                    for (int j = i + 1; j < selectedLines.Count; j++)
                    {
                        Line a = selectedLines[i];
                        Line b = selectedLines[j];

                        bool aHorizontal = IsHorizontal(a);
                        bool aVertical = IsVertical(a);
                        bool bHorizontal = IsHorizontal(b);
                        bool bVertical = IsVertical(b);

                        Line hLine = null;
                        Line vLine = null;

                        if (aHorizontal && bVertical)
                        {
                            hLine = a;
                            vLine = b;
                        }
                        else if (aVertical && bHorizontal)
                        {
                            hLine = b;
                            vLine = a;
                        }
                        else
                        {
                            continue;
                        }

                        Point3dCollection pts = new Point3dCollection();
                        hLine.IntersectWith(vLine, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                        foreach (Point3d pt in pts)
                        {
                            bool exists = uniqueIntersections.Any(p => p.DistanceTo(pt) < Tol);
                            if (!exists)
                            {
                                uniqueIntersections.Add(pt);

                                Circle c = new Circle(pt, Vector3d.ZAxis, CircleRadius);
                                c.ColorIndex = 1;
                                btr.AppendEntity(c);
                                tr.AddNewlyCreatedDBObject(c, true);
                                createdCircles.Add(c);
                            }
                        }
                    }
                }

                // 2) Tạo line từ mỗi đầu mút line gốc đến tất cả giao điểm nằm trên line đó
                foreach (Line baseLine in selectedLines)
                {
                    List<Point3d> pointsOnThisLine = uniqueIntersections
                        .Where(p => IsPointOnLine(baseLine, p))
                        .ToList();

                    foreach (Point3d ip in pointsOnThisLine)
                    {
                        Line lineFromStart = new Line(baseLine.StartPoint, ip);
                        lineFromStart.ColorIndex = 3;
                        btr.AppendEntity(lineFromStart);
                        tr.AddNewlyCreatedDBObject(lineFromStart, true);
                        createdLines.Add(lineFromStart);

                        Line lineFromEnd = new Line(baseLine.EndPoint, ip);
                        lineFromEnd.ColorIndex = 3;
                        btr.AppendEntity(lineFromEnd);
                        tr.AddNewlyCreatedDBObject(lineFromEnd, true);
                        createdLines.Add(lineFromEnd);
                    }
                }

                // 3) Xóa các new lines trùng điểm bắt đầu ở đầu mút line cũ, giữ line ngắn hơn
                List<Line> linesToErase = new List<Line>();

                foreach (Line baseLine in selectedLines)
                {
                    Point3d[] oldEndpoints = new Point3d[] { baseLine.StartPoint, baseLine.EndPoint };

                    foreach (Point3d endpoint in oldEndpoints)
                    {
                        var sameStartLines = createdLines
                            .Where(l => l.StartPoint.DistanceTo(endpoint) < Tol)
                            .ToList();

                        if (sameStartLines.Count > 1)
                        {
                            Line shortest = sameStartLines
                                .OrderBy(l => l.Length)
                                .First();

                            foreach (Line ln in sameStartLines)
                            {
                                if (ln.ObjectId != shortest.ObjectId)
                                    linesToErase.Add(ln);
                            }
                        }
                    }
                }

                linesToErase = linesToErase
                    .GroupBy(l => l.ObjectId)
                    .Select(g => g.First())
                    .ToList();

                foreach (Line ln in linesToErase)
                {
                    ln.Erase();
                }

                keptLines = createdLines
                    .Where(l => !linesToErase.Any(x => x.ObjectId == l.ObjectId))
                    .ToList();

                // 4) Tạo 4 line chéo
                foreach (Point3d ip in uniqueIntersections)
                {
                    Line hLine = keptLines.FirstOrDefault(l =>
                        l.EndPoint.DistanceTo(ip) < Tol && IsHorizontal(l));

                    Line vLine = keptLines.FirstOrDefault(l =>
                        l.EndPoint.DistanceTo(ip) < Tol && IsVertical(l));

                    if (hLine == null || vLine == null)
                        continue;

                    Point3d hOffsetPoint = GetPointFromIntersectionTowardStart(hLine, ChamferDist);
                    Point3d vOffsetPoint = GetPointFromIntersectionTowardStart(vLine, ChamferDist);

                    Line diagonal = new Line(hOffsetPoint, vOffsetPoint);
                    diagonal.ColorIndex = 2; // yellow
                    btr.AppendEntity(diagonal);
                    tr.AddNewlyCreatedDBObject(diagonal, true);
                    diagonalLines.Add(diagonal);
                }

                // 5) Move điểm mút của kept lines tại giao điểm vào trong 4 đơn vị
                foreach (Line kept in keptLines)
                {
                    Point3d oldEnd = kept.EndPoint;

                    if (!uniqueIntersections.Any(p => p.DistanceTo(oldEnd) < Tol))
                        continue;

                    Vector3d dir = kept.StartPoint - kept.EndPoint; // từ giao điểm về đầu còn lại
                    if (dir.Length < Tol)
                        continue;

                    dir = dir.GetNormal() * ChamferDist;
                    Point3d newEnd = kept.EndPoint + dir;

                    kept.UpgradeOpen();
                    kept.EndPoint = newEnd;
                }

                // 6) Debug: chỉ list kept new lines + diagonal lines
                debug.AppendLine("Kept new lines:");
                foreach (Line ln in keptLines)
                {
                    if (!ln.IsErased)
                        debug.AppendLine($"  {FormatLine(ln)}");
                }

                debug.AppendLine();
                debug.AppendLine("Diagonal lines:");
                foreach (Line ln in diagonalLines)
                {
                    if (!ln.IsErased)
                        debug.AppendLine($"  {FormatLine(ln)}");
                }

                // 7) Xóa circle giao điểm
                foreach (Circle c in createdCircles)
                {
                    if (!c.IsErased)
                        c.Erase();
                }

                // 8) Xóa 4 line selected ban đầu
                foreach (Line ln in selectedLines)
                {
                    if (!ln.IsErased)
                    {
                        ln.UpgradeOpen();
                        ln.Erase();
                    }
                }

                debug.AppendLine();
                debug.AppendLine($"Summary: {createdCircles.Count} circles created then deleted.");
                debug.AppendLine($"Summary: {createdLines.Count} new lines created before cleanup.");
                debug.AppendLine($"Summary: {linesToErase.Count} new lines deleted by duplicate-start cleanup.");
                debug.AppendLine($"Summary: {keptLines.Count} new lines kept.");
                debug.AppendLine($"Summary: {diagonalLines.Count} diagonal lines created.");
                debug.AppendLine($"Summary: {selectedLines.Count} selected source lines deleted.");

                MessageBox.Show(debug.ToString(), "Debug Intersections");

                tr.Commit();
            }
        }

        private static bool IsHorizontal(Line line)
        {
            return Math.Abs(line.StartPoint.Y - line.EndPoint.Y) < Tol;
        }

        private static bool IsVertical(Line line)
        {
            return Math.Abs(line.StartPoint.X - line.EndPoint.X) < Tol;
        }

        private static Point3d GetPointFromIntersectionTowardStart(Line line, double dist)
        {
            Vector3d dir = line.StartPoint - line.EndPoint;
            if (dir.Length < Tol)
                return line.EndPoint;

            dir = dir.GetNormal() * dist;
            return line.EndPoint + dir;
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
    }
}
