using KuroAcad;
using KuroAcad.Helper;
using KuroAcad.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class TemDatUtil
{
    public void Main()
    {
        // Get the current database
        Database acCurDb;
        acCurDb = Application.DocumentManager.MdiActiveDocument.Database;

        // Get active Document
        var acDoc = Application.DocumentManager.MdiActiveDocument;

        // Data input
        #region Data input
        string blockName = "";

        string tagName = "";
        string tagArea = "";
        string tagDensity = "";
        string tagFloors = "";
        string tagFAR = "";

        string valueName = "";
        string valueDensity = "";
        string valueFloors = "";
        string valueFAR = "";

        int tagCount = 2;
        int polyIndex = 0;
        #endregion

        // Show Dialog
        #region Get Result from Dialog
        var view = new TemDatView();
        var result = view.ShowDialog();
        if (result != true || view.DialogData == null)
        {
            return;
        }

        var dialogData = view.DialogData;

        tagName = dialogData.TagName;
        tagArea = dialogData.TagArea;
        tagDensity = dialogData.TagDensity;
        tagFloors = dialogData.TagFloors;
        tagFAR = dialogData.TagFAR;

        valueDensity = dialogData.ValueDensity;
        valueFloors = dialogData.ValueFloors;
        valueFAR = dialogData.ValueFAR;

        blockName = dialogData.BlockName;
        polyIndex = dialogData.StartNumber;
        tagCount = dialogData.TagCount;
        #endregion

        //Get list of polyline from user
        #region Get list of polyline from user
        List<Polyline> pls = new List<Polyline>();
        PromptSelectionOptions pso = new PromptSelectionOptions();
        pso.AllowDuplicates = false;
        pso.AllowSubSelections = false;
        pso.MessageForAdding = "\nChọn lô đất";

        PromptSelectionResult psr = acDoc.Editor.GetSelection(pso);
        if (psr.Status != PromptStatus.OK || psr.Value.Count == 0)
        {
            return;
        }
        #endregion

        //Start a transaction
        #region Start a transaction
        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        {
            #region Create new block
            // Open the Block table for read
            BlockTable acBlkTbl;
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

            ObjectId blkRecId = ObjectId.Null;

            if (!acBlkTbl.Has(blockName))
            {
                using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                {
                    acBlkTblRec.Name = blockName;

                    // Set the insertion point for the block
                    acBlkTblRec.Origin = new Point3d(0, 0, 0);

                    // Add a circle to the block
                    using (Circle acCirc = new Circle(),
                        acCirc1 = new Circle())
                    {
                        acCirc.Center = new Point3d(0, 0, 0);
                        acCirc.Radius = 3;

                        acCirc1.Center = new Point3d(0, 0, 0);
                        acCirc1.Radius = 2.8;

                        acBlkTblRec.AppendEntity(acCirc);
                        acBlkTblRec.AppendEntity(acCirc1);

                        // Add an attribute definition to the block
                        using (AttributeDefinition acAttDef = new AttributeDefinition(),
                            acAttDef1 = new AttributeDefinition())
                        {
                            acAttDef.Position = new Point3d(0, 0, 0);
                            acAttDef.Verifiable = true;
                            acAttDef.Prompt = "TEN:";
                            acAttDef.Tag = tagName;
                            acAttDef.TextString = "-";
                            acAttDef.Height = 1;
                            acAttDef.Justify = AttachmentPoint.MiddleCenter;
                            acAttDef.AlignmentPoint = new Point3d(0, 1.5, 0);

                            acAttDef1.Position = new Point3d(0, 0, 0);
                            acAttDef1.Verifiable = true;
                            acAttDef1.Prompt = "Dien tich:";
                            acAttDef1.Tag = tagArea;
                            acAttDef1.TextString = "-";
                            acAttDef1.Height = 1;
                            acAttDef1.Justify = AttachmentPoint.MiddleCenter;
                            acAttDef1.AlignmentPoint = new Point3d(0, -1.5, 0);

                            acBlkTblRec.AppendEntity(acAttDef);
                            acBlkTblRec.AppendEntity(acAttDef1);

                            acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForWrite);
                            acBlkTbl.Add(acBlkTblRec);
                            acTrans.AddNewlyCreatedDBObject(acBlkTblRec, true);
                        }

                    }
                    blkRecId = acBlkTblRec.Id;
                }
            }
            else
            {
                blkRecId = acBlkTbl[blockName];
            }
            #endregion

            //Get list of polyline
            #region Get list of polyline List<PolyLine> pls
            foreach (SelectedObject so in psr.Value)
            {
                Entity ent = (Entity)acTrans.GetObject(so.ObjectId, OpenMode.ForRead);
                if (ent is Polyline)
                {
                    Polyline pl = ent as Polyline;
                    pls.Add(pl);
                }
            }
            #endregion

            // Insert the block into the current space
            #region Insert the block into the current space
            foreach (Polyline pl in pls)
            {
                //Get center point of polyline
                Point3d cenPt = GeometryHelper.GetCenterPoint(pl);

                //Get area of polyline
                double area = pl.Area;

                //Get name of polyline
                valueName = dialogData.Prefix + polyIndex.ToString();

                // Create and insert the new block reference
                #region Create and insert the new block reference
                if (blkRecId != ObjectId.Null)
                {
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = acTrans.GetObject(blkRecId, OpenMode.ForRead) as BlockTableRecord;


                    using (BlockReference acBlkRef = new BlockReference(cenPt, blkRecId))
                    {
                        BlockTableRecord acCurSpaceBlkTblRec;
                        acCurSpaceBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                        acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                        acTrans.AddNewlyCreatedDBObject(acBlkRef, true);

                        // Verify block table record has attribute definitions associated with it
                        #region Verify block table record has attribute definitions associated with it
                        if (acBlkTblRec.HasAttributeDefinitions)
                        {
                            // Add attributes from the block table record
                            foreach (ObjectId objID in acBlkTblRec)
                            {
                                DBObject dbObj = acTrans.GetObject(objID, OpenMode.ForRead) as DBObject;

                                if (dbObj is AttributeDefinition)
                                {
                                    AttributeDefinition acAtt = dbObj as AttributeDefinition;

                                    if (!acAtt.Constant)
                                    {
                                        using (AttributeReference acAttRef = new AttributeReference())
                                        {
                                            acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                                            acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform);

                                            acAttRef.TextString = acAtt.TextString;

                                            acBlkRef.AttributeCollection.AppendAttribute(acAttRef);

                                            acTrans.AddNewlyCreatedDBObject(acAttRef, true);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        // Get attribute value
                        AttributeCollection acAttColl = acBlkRef.AttributeCollection;

                        //Set attribute value
                        #region Set attribute value
                        foreach (ObjectId acAttId in acAttColl)
                        {
                            using (AttributeReference acAtt = (AttributeReference)acTrans.GetObject(acAttId, OpenMode.ForRead))
                            {
                                if (acAtt.Tag == tagName)
                                {
                                    acAtt.TextString = valueName;
                                }
                                else if (acAtt.Tag.ToString() == tagArea)
                                {
                                    acAtt.TextString = area.ToString("F2");
                                }
                                else if (acAtt.Tag.ToString() == tagDensity)
                                {
                                    acAtt.TextString = valueDensity;
                                }
                                else if (acAtt.Tag.ToString() == tagFloors)
                                {
                                    acAtt.TextString = valueFloors;
                                }
                                else if (acAtt.Tag.ToString() == tagFAR)
                                {
                                    acAtt.TextString = valueFAR;
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion

                polyIndex++;
            }
            #endregion

            // Save the new object to the database
            acTrans.Commit();

            // Dispose of the transaction
        }
        #endregion
    }

}

