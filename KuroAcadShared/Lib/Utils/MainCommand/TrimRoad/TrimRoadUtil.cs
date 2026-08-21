using KuroAcad.Helper;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace KuroAcad
{
    internal static class TrimRoadUtil
    {
        internal static void TrimRoad()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            var listCur = new List<Curve>();
            var pso = new PromptSelectionOptions
            {
                MessageForAdding = "\nSelect Polyline to trim: ",
                SingleOnly = false
            };

            var psr = acDoc.Editor.GetSelection(pso);
            if (psr.Status != PromptStatus.OK)
            {
                return;
            }

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject selectedObject in psr.Value)
                {
                    var ent = acTrans.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as Entity;
                    if (ent is Curve curve)
                    {
                        listCur.Add(curve);
                    }
                }

                for (int i = 0; i < listCur.Count; i++)
                {
                    for (int j = i + 1; j < listCur.Count; j++)
                    {
                        var curve1 = listCur[i];
                        var curve2 = listCur[j];

                        var pts = new Point3dCollection();
                        curve1.IntersectWith(curve2, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                        var pline1 = GetPolyline(curve1, acTrans);
                        var pline2 = GetPolyline(curve2, acTrans);

                        if (pline1 == null || pline2 == null || pts.Count == 0)
                        {
                            continue;
                        }

                        foreach (Point3d pt in pts)
                        {
                            AddVertex(pline1, pt);
                            AddVertex(pline2, pt);
                        }
                    }
                }

                acTrans.Commit();
            }
        }

        private static Polyline GetPolyline(Curve curve, Transaction trans)
        {
            if (curve is Polyline polyline)
            {
                return polyline;
            }

            if (curve is Line || curve is Arc)
            {
                return curve.ReplaceWithPolyline(trans);
            }

            return null;
        }

        internal static void AddVertex(Polyline pline, Point3d point)
        {
            point = pline.GetClosestPointTo(point, false);
            double parameter = pline.GetParameterAtPoint(point);
            int index = (int)parameter;

            if (parameter == index)
            {
                return;
            }

            double bulge = pline.GetBulgeAt(index);
            var plane = new Plane(Point3d.Origin, pline.Normal);

            if (bulge == 0.0)
            {
                pline.AddVertexAt(index + 1, point.Convert2d(plane), 0.0, 0.0, 0.0);
            }
            else
            {
                double angle = Math.Atan(bulge);
                double angle1 = angle * (parameter - index);
                double angle2 = angle - angle1;

                pline.AddVertexAt(index + 1, point.Convert2d(plane), Math.Tan(angle2), 0.0, 0.0);
                pline.SetBulgeAt(index, Math.Tan(angle1));
            }
        }
    }


}