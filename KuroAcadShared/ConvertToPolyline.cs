using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(KuroAcad.ConvertToPolyline))]
namespace KuroAcad
{
    class ConvertToPolyline
    {
        /// <summary>
        /// Replaces a selected Line or Arc with an equivalent Polyline.
        /// The polyline that replaces the Line or Arc inherits the
        /// handle, application data, and common properties of the
        /// Line or Arc.
        /// </summary>

        [CommandMethod("CVPOLY")]
        public static void ConvertToPolyLineCommand()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect a Line or Arc: ");
            peo.SetRejectMessage("\nInvalid input, requires a Line or Arc,\n");
            peo.AddAllowedClass(typeof(Line), false);
            peo.AddAllowedClass(typeof(Arc), false);
            peo.AllowObjectOnLockedLayer = false;
            var per = doc.Editor.GetEntity(peo);
            if (per.Status != PromptStatus.OK)
                return;
            using (var trans = doc.TransactionManager.StartOpenCloseTransaction())
            {
                Curve curve = (Curve)trans.GetObject(per.ObjectId, OpenMode.ForRead);
                try
                {
                    curve.ReplaceWithPolyline(trans);
                    trans.Commit();
                }
                catch (System.Exception ex)
                {
                    doc.Editor.WriteMessage(ex.ToString());
                }
            }
        }
    }
}
