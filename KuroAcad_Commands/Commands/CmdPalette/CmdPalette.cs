using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(KuroAcad.UI.CmdPalette))]
namespace KuroAcad.UI
{
    class CmdPalette
    {
        //instance of the PaletteSet
        static CustomPaletteSet paletteSet;

        //instance fied 
        double radius = 10.0;
        string layer;

        /// <summary>
        /// Palette display command
        /// </summary>

        [CommandMethod("KPalette")]
        public void ShowPalette()
        {
            if (paletteSet == null)
            {
                paletteSet = new CustomPaletteSet();
            }
            paletteSet.Visible = true;
        }

        /// <summary>
        /// Circle drawing Command
        /// </summary>
        [CommandMethod("KCircleWPF")]
        public void DrawCircle()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // choice the layer
            if (string.IsNullOrEmpty(layer))
                layer = (string)Application.GetSystemVariable("clayer");
            var strOptions = new PromptStringOptions("\nLayer Name: ");
            strOptions.DefaultValue = layer;
            strOptions.UseDefaultValue = true;
            var strResult = ed.GetString(strOptions);
            if (strResult.Status != PromptStatus.OK)
                return;
            layer = strResult.StringResult;

            //radius circle
            var distOptions = new PromptDistanceOptions("\nRadius: ");
            distOptions.DefaultValue = radius;
            distOptions.UseDefaultValue = true;
            var distResult = ed.GetDistance(distOptions);
            if (distResult.Status != PromptStatus.OK)
                return;
            radius = distResult.Value;

            // center specify
            var ppr = ed.GetPoint("\nCenter: ");
            if (ppr.Status == PromptStatus.OK)
            {
                // draw the circle in the current space
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var curSpace =
                        (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    using (var circle = new Circle(ppr.Value, Vector3d.ZAxis, distResult.Value))
                    {
                        circle.TransformBy(ed.CurrentUserCoordinateSystem);
                        circle.Layer = strResult.StringResult;
                        curSpace.AppendEntity(circle);
                        tr.AddNewlyCreatedDBObject(circle, true);
                    }
                    tr.Commit();
                }
            }
        }
    }
}
