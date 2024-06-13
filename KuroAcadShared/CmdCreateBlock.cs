using KuroAcad.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using RadioButton = System.Windows.Controls.RadioButton;

[assembly: CommandClass(typeof(KuroAcad.CmdCreateBlock))]
namespace KuroAcad
{
    class CmdCreateBlock
    {
        [CommandMethod("KuroAddBlockAtt")]
        public void KuroAddBlockAtt()
        {
            // Get the current database and start a transaction
            Database acCurDb;
            acCurDb = Application.DocumentManager.MdiActiveDocument.Database;

            // Data input
            string blockName = "";

            string tagName = "";
            string tagArea = "";
            string tagDensity = "";
            string tagFloors = "";
            string tagFAR = "";

            string valueName = "";
            string valueArea = "";
            string valueDensity = "";
            string valueFloors = "";
            string valueFAR = "";

            int tagCount = 2;
            int polyIndex = 0;

            double circleRadius = 3;
            
            // Show Dialog
            KuroTLWPF kuroTLWPF = new KuroTLWPF();
            kuroTLWPF.ShowDialog();

            if (kuroTLWPF.DialogResult != true)
            {
                return;
            }
            if (kuroTLWPF.DialogResult == true)
            {
                string ActionSelectionButtonName = (kuroTLWPF.groupBox_Option.Content as System.Windows.Controls.Grid)
                                .Children.OfType<RadioButton>()
                                .FirstOrDefault(rb => rb.IsChecked.Value == true)
                                .Name;
                if (ActionSelectionButtonName == "radioButton2")
                {
                    tagName = kuroTLWPF.textBoxName.Text;
                    tagArea = kuroTLWPF.textBoxArea.Text;

                }
                else if (ActionSelectionButtonName == "radioButton4")
                {
                    tagName = kuroTLWPF.textBoxName.Text;
                    tagArea = kuroTLWPF.textBoxArea.Text;
                    tagDensity = kuroTLWPF.textBoxDensity.Text;
                    tagFloors = kuroTLWPF.textBoxFloors.Text;

                    valueDensity = kuroTLWPF.textBoxDensityValue.Text;
                    valueFloors = kuroTLWPF.textBoxFloorsValue.Text;

                    tagCount = 4;
                }
                else if (ActionSelectionButtonName == "radioButton5")
                {
                    tagName = kuroTLWPF.textBoxName.Text;
                    tagArea = kuroTLWPF.textBoxArea.Text;
                    tagDensity = kuroTLWPF.textBoxDensity.Text;
                    tagFloors = kuroTLWPF.textBoxFloors.Text;
                    tagFAR = kuroTLWPF.textBoxFAR.Text;

                    valueDensity = kuroTLWPF.textBoxDensityValue.Text;
                    valueFloors = kuroTLWPF.textBoxFloorsValue.Text;
                    valueFAR = kuroTLWPF.textBoxFARValue.Text;

                    tagCount = 5;
                }
                blockName = kuroTLWPF.textBoxBlockName.Text;
                polyIndex = int.Parse(kuroTLWPF.textBoxStartNumber.Text);
                valueName = kuroTLWPF.textBoxPrefix.Text + polyIndex.ToString();
            }


            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                ObjectId blkRecId = ObjectId.Null;

                if (!acBlkTbl.Has("TEMDAT"))
                {
                    using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                    {
                        acBlkTblRec.Name = "TEMDAT";

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
                                acAttDef.Tag = "TL";
                                acAttDef.TextString = "LK01";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acAttDef.AlignmentPoint = new Point3d(0, 1.5, 0);

                                acAttDef1.Position = new Point3d(0, 0, 0);
                                acAttDef1.Verifiable = true;
                                acAttDef1.Prompt = "Dien tich:";
                                acAttDef1.Tag = "DT";
                                acAttDef1.TextString = "150";
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
                    blkRecId = acBlkTbl["TEMDAT"];
                }

                // Insert the block into the current space
                if (blkRecId != ObjectId.Null)
                {
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = acTrans.GetObject(blkRecId, OpenMode.ForRead) as BlockTableRecord;

                    // Create and insert the new block reference
                    using (BlockReference acBlkRef = new BlockReference(new Point3d(2, 2, 0), blkRecId))
                    {
                        BlockTableRecord acCurSpaceBlkTblRec;
                        acCurSpaceBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                        acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                        acTrans.AddNewlyCreatedDBObject(acBlkRef, true);

                        // Verify block table record has attribute definitions associated with it
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
                    }
                }

                // Save the new object to the database
                acTrans.Commit();

                // Dispose of the transaction
            }
        }
    }
}
