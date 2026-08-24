using Autodesk.AutoCAD.ApplicationServices;

namespace KuroAcad.Lib.Utils.MainCommand.Intersection
{
    public class IntersectionUtils
    {
        public void Main()
        {
            Document document = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            Database database = document.Database;
            Editor editor = document.Editor;

            PromptEntityResult selectedPolylineResult = SelectPolyline(editor);
            if (selectedPolylineResult.Status != PromptStatus.OK)
                return;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable =
                    (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);

                BlockTableRecord modelSpace =
                    (BlockTableRecord)transaction.GetObject(
                        blockTable[BlockTableRecord.ModelSpace],
                        OpenMode.ForWrite);

                Polyline firstPolyline = CreateFirstPolyline();
                Polyline secondPolyline = CreateSecondPolyline();

                modelSpace.AppendEntity(firstPolyline);
                transaction.AddNewlyCreatedDBObject(firstPolyline, true);

                modelSpace.AppendEntity(secondPolyline);
                transaction.AddNewlyCreatedDBObject(secondPolyline, true);

                Point3dCollection intersectionPoints = GetIntersectionPoints(firstPolyline, secondPolyline);

                editor.WriteMessage($"\nNo. Of Intersection Points: {intersectionPoints.Count}");

                for (int i = 0; i < intersectionPoints.Count; i++)
                {
                    Point3d point = intersectionPoints[i];
                    editor.WriteMessage($"\nPoint No. {i}: X:{point.X}, Y:{point.Y}");
                }

                if (intersectionPoints.Count > 0)
                {
                    AddVertex(firstPolyline, intersectionPoints[0]);
                }

                transaction.Commit();
            }
        }

        private static PromptEntityResult SelectPolyline(Editor editor)
        {
            PromptEntityOptions options = new PromptEntityOptions("\nSelect a polyline: ");
            options.SetRejectMessage("\nSelected object is not a polyline.");
            options.AddAllowedClass(typeof(Polyline), true);

            return editor.GetEntity(options);
        }

        private static Polyline CreateFirstPolyline()
        {
            Polyline polyline = new Polyline();
            polyline.SetDatabaseDefaults();
            polyline.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
            polyline.AddVertexAt(1, new Point2d(0, 50), 0, 0, 0);
            polyline.AddVertexAt(2, new Point2d(75, 50), 0, 0, 0);
            return polyline;
        }

        private static Polyline CreateSecondPolyline()
        {
            Polyline polyline = new Polyline();
            polyline.SetDatabaseDefaults();
            polyline.AddVertexAt(0, new Point2d(30, 30), 0, 0, 0);
            polyline.AddVertexAt(1, new Point2d(-15, 30), 0, 0, 0);
            return polyline;
        }

        private static Point3dCollection GetIntersectionPoints(Polyline first, Polyline second)
        {
            Point3dCollection intersections = new Point3dCollection();

            first.IntersectWith(
                second,
                Intersect.OnBothOperands,
                intersections,
                IntPtr.Zero,
                IntPtr.Zero);

            return intersections;
        }

        private static void AddVertex(Polyline polyline, Point3d point)
        {
            point = polyline.GetClosestPointTo(point, false);
            double parameter = polyline.GetParameterAtPoint(point);
            int segmentIndex = (int)parameter;

            if (parameter == segmentIndex)
                return;

            double bulge = polyline.GetBulgeAt(segmentIndex);
            Plane plane = new Plane(Point3d.Origin, polyline.Normal);

            if (bulge == 0.0)
            {
                polyline.AddVertexAt(
                    segmentIndex + 1,
                    point.Convert2d(plane),
                    0.0,
                    0.0,
                    0.0);
            }
            else
            {
                double angle = Math.Atan(bulge);
                double firstArcBulgeAngle = angle * (parameter - segmentIndex);
                double secondArcBulgeAngle = angle - firstArcBulgeAngle;

                polyline.AddVertexAt(
                    segmentIndex + 1,
                    point.Convert2d(plane),
                    Math.Tan(secondArcBulgeAngle),
                    0.0,
                    0.0);

                polyline.SetBulgeAt(segmentIndex, Math.Tan(firstArcBulgeAngle));
            }
        }
    }
}
