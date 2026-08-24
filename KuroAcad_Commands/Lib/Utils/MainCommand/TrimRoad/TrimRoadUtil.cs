using KuroAcad.Helper;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace KuroAcad
{
    internal static class TrimRoadUtil
    {
        internal static void TrimRoad()
        {
            // Get current AutoCAD document and database
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;
            var editor = document.Editor;

            // Ask user to select curves to trim/split at intersections
            var selectionOptions = new PromptSelectionOptions
            {
                MessageForAdding = "\nSelect polylines to trim: ",
                SingleOnly = false
            };

            var selectionResult = editor.GetSelection(selectionOptions);
            if (selectionResult.Status != PromptStatus.OK)
            {
                return;
            }

            using (var transaction = database.TransactionManager.StartTransaction())
            {
                // Collect all selected curve entities
                var curves = GetSelectedCurves(selectionResult, transaction);

                // Find intersections between each pair of curves
                for (int i = 0; i < curves.Count; i++)
                {
                    for (int j = i + 1; j < curves.Count; j++)
                    {
                        ProcessCurveIntersection(curves[i], curves[j], transaction);
                    }
                }

                transaction.Commit();
            }
        }

        /// <summary>
        /// Extracts curve entities from the selected objects.
        /// </summary>
        private static List<Curve> GetSelectedCurves(PromptSelectionResult selectionResult, Transaction transaction)
        {
            var curves = new List<Curve>();

            foreach (SelectedObject selectedObject in selectionResult.Value)
            {
                if (selectedObject == null)
                {
                    continue;
                }

                var entity = transaction.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as Entity;
                if (entity is Curve curve)
                {
                    curves.Add(curve);
                }
            }

            return curves;
        }

        /// <summary>
        /// Finds intersection points between two curves and adds vertices at those points.
        /// </summary>
        private static void ProcessCurveIntersection(Curve curve1, Curve curve2, Transaction transaction)
        {
            var polyline1 = GetOrCreatePolyline(curve1, transaction);
            var polyline2 = GetOrCreatePolyline(curve2, transaction);

            if (polyline1 == null || polyline2 == null)
            {
                return;
            }

            var intersectionPoints = new Point3dCollection();
            curve1.IntersectWith(curve2, Intersect.OnBothOperands, intersectionPoints, IntPtr.Zero, IntPtr.Zero);

            if (intersectionPoints.Count == 0)
            {
                return;
            }

            foreach (Point3d point in intersectionPoints)
            {
                AddVertexAtPoint(polyline1, point);
                AddVertexAtPoint(polyline2, point);
            }
        }

        /// <summary>
        /// Returns a polyline from the given curve.
        /// If the curve is a line or arc, it is converted to a polyline.
        /// </summary>
        private static Polyline GetOrCreatePolyline(Curve curve, Transaction transaction)
        {
            if (curve is Polyline polyline)
            {
                return polyline;
            }

            if (curve is Line || curve is Arc)
            {
                return curve.ReplaceWithPolyline(transaction);
            }

            return null;
        }

        /// <summary>
        /// Adds a vertex to the polyline at the specified point if the point is not already a vertex.
        /// Handles both straight and arc segments.
        /// </summary>
        internal static void AddVertexAtPoint(Polyline polyline, Point3d point)
        {
            // Get the closest point on the polyline
            point = polyline.GetClosestPointTo(point, false);

            // Get curve parameter at the point
            double parameter = polyline.GetParameterAtPoint(point);
            int segmentIndex = (int)parameter;

            // If parameter is an integer, the point is already on an existing vertex
            if (parameter == segmentIndex)
            {
                return;
            }

            double bulge = polyline.GetBulgeAt(segmentIndex);
            var plane = new Plane(Point3d.Origin, polyline.Normal);
            var point2d = point.Convert2d(plane);

            // Straight segment
            if (bulge == 0.0)
            {
                polyline.AddVertexAt(segmentIndex + 1, point2d, 0.0, 0.0, 0.0);
                return;
            }

            // Arc segment:
            // Split the arc into two parts by recalculating bulge values
            double angle = Math.Atan(bulge);
            double firstArcAngle = angle * (parameter - segmentIndex);
            double secondArcAngle = angle - firstArcAngle;

            polyline.AddVertexAt(segmentIndex + 1, point2d, Math.Tan(secondArcAngle), 0.0, 0.0);
            polyline.SetBulgeAt(segmentIndex, Math.Tan(firstArcAngle));
        }
    }
}
