namespace KuroAcad
{
    internal class BlockAtt
    {
        //method to insert block with attribute
        public void InsertBlockAtt(string blockName, int attTag, Point3d insertPoint, Database acCurdb)
        {
            using (Transaction acTrans = acCurdb.TransactionManager.StartTransaction())
            {
                //Open the block table for read

                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForRead) as BlockTable;

                ObjectId blRecId = ObjectId.Null;

                if (!acBlkTbl.Has(blockName))
                {
                    using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                    {
                        acBlkTblRec.Name = blockName;

                        // Set the insertion point for the block
                        acBlkTblRec.Origin = insertPoint;

                        // Add a circle to the block
                        using (Circle acCirc = new Circle())
                        {
                            if (attTag == 1)
                            {
                                // Set the center and the radius of the circle
                                acCirc.Center = insertPoint;
                                acCirc.Radius = 1.5;

                                acBlkTblRec.AppendEntity(acCirc);

                                // Add attribute definition to the block
                                AddAttToBlock(blockName, attTag, insertPoint, acCurdb);
                            }
                            if (attTag == 2)
                            {
                                // Set the center and the radius of the circle
                                acCirc.Center = insertPoint;
                                acCirc.Radius = 3;

                                acBlkTblRec.AppendEntity(acCirc);

                                // Add attribute definition to the block
                                AddAttToBlock(blockName, attTag, insertPoint, acCurdb);
                            }
                            if (attTag == 4)
                            {
                                // Set the center and the radius of the circle
                                acCirc.Center = insertPoint;
                                acCirc.Radius = 8;

                                acBlkTblRec.AppendEntity(acCirc);
                                // Add attribute definition to the block
                                AddAttToBlock(blockName, attTag, insertPoint, acCurdb);
                            }
                            if (attTag == 5)
                            {
                                // Set the center and the radius of the circle
                                acCirc.Center = insertPoint;
                                acCirc.Radius = 8;

                                acBlkTblRec.AppendEntity(acCirc);
                                // Add attribute definition to the block
                                AddAttToBlock(blockName, attTag, insertPoint, acCurdb);
                            }

                        }
                        blRecId = acBlkTblRec.Id;
                    }
                }
                else
                {
                    blRecId = acBlkTbl[blockName];
                }
                //Insert the block to current space
                if (blRecId != ObjectId.Null)
                {
                    BlockTableRecord acBlTblRec;
                    acBlTblRec = acTrans.GetObject(acCurdb.CurrentSpaceId, OpenMode.ForRead) as BlockTableRecord;

                    //create and insert the new block reference
                    using (BlockReference acBlkRef = new BlockReference(insertPoint, blRecId))
                    {
                        BlockTableRecord acCurSpaceBlkTblRec;
                        acCurSpaceBlkTblRec = acTrans.GetObject(acCurdb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                        acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                        acTrans.AddNewlyCreatedDBObject(acBlkRef, true);

                        //Verify block table record has attribute definitions associated with it
                        if (acBlTblRec.HasAttributeDefinitions)
                        {
                            //Add attributes from the block table record
                            foreach (ObjectId objID in acBlTblRec)
                            {
                                DBObject dbObj = acTrans.GetObject(objID, OpenMode.ForRead) as DBObject;
                                if (dbObj is AttributeDefinition)
                                {
                                    AttributeDefinition acAttDef = dbObj as AttributeDefinition;
                                    using (AttributeReference acAttRef = new AttributeReference())
                                    {
                                        acAttRef.SetAttributeFromBlock(acAttDef, acBlkRef.BlockTransform);
                                        acAttRef.Position = acAttDef.Position.TransformBy(acBlkRef.BlockTransform);
                                        acAttRef.TextString = acAttDef.TextString;
                                        acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                                        acTrans.AddNewlyCreatedDBObject(acAttRef, true);
                                    }
                                }
                            }
                        }
                    }
                }
                //save the new object to the database
                acTrans.Commit();
            }

        }

        //method to add list attribute tag to block
        public void AddAttToBlock(string blockName, int attTag, Point3d insertPoint, Database acCurdb)
        {
            using (Transaction acTrans = acCurdb.TransactionManager.StartTransaction())
            {
                //Open the block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (!acBlkTbl.Has(blockName))
                {
                    using (BlockTableRecord acBlTblRec = new BlockTableRecord())
                    {
                        acBlTblRec.Name = blockName;

                        // Add an attribute definition to the block
                        if (attTag == 1)
                        {
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = insertPoint;
                                acAttDef.Prompt = "Enter Tag: ";
                                acAttDef.Tag = "TL";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                                acTrans.AddNewlyCreatedDBObject(acAttDef, true);
                            }
                        }
                        if (attTag == 2)
                        {
                            Point3d pt1 = new Point3d(insertPoint.X, insertPoint.Y + 1, insertPoint.Z);
                            Point3d pt2 = new Point3d(insertPoint.X, insertPoint.Y - 1, insertPoint.Z);

                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = pt1;
                                acAttDef.Prompt = "Enter Tag: ";
                                acAttDef.Tag = "TL";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                                acTrans.AddNewlyCreatedDBObject(acAttDef, true);
                            }
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = pt2;
                                acAttDef.Prompt = "Enter Tag: ";
                                acAttDef.Tag = "DT";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                                acTrans.AddNewlyCreatedDBObject(acAttDef, true);
                            }
                        }
                        if (attTag == 4)
                        {
                            Point3d pt1 = new Point3d(insertPoint.X, insertPoint.Y + 3.5, insertPoint.Z);
                            Point3d pt2 = insertPoint;
                            Point3d pt3 = new Point3d(insertPoint.X - 2, insertPoint.Y - 3.5, insertPoint.Z);
                            Point3d pt4 = new Point3d(insertPoint.X + 2, insertPoint.Y - 3.5, insertPoint.Z);
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = pt1;
                                acAttDef.Prompt = "Ten lo: ";
                                acAttDef.Tag = "TL";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                                acTrans.AddNewlyCreatedDBObject(acAttDef, true);
                            }
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = pt2;
                                acAttDef.Prompt = "Dien tich: ";
                                acAttDef.Tag = "DT";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                                acTrans.AddNewlyCreatedDBObject(acAttDef, true);
                            }
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = pt3;
                                acAttDef.Prompt = "Mat do xay dung: ";
                                acAttDef.Tag = "MDXD";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                                acTrans.AddNewlyCreatedDBObject(acAttDef, true);
                            }
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = pt4;
                                acAttDef.Prompt = "Tang cao: ";
                                acAttDef.Tag = "TC";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                                acTrans.AddNewlyCreatedDBObject(acAttDef, true);
                            }
                        }
                        if (attTag == 5)
                        {
                            Point3d pt1 = new Point3d(insertPoint.X, insertPoint.Y + 3.5, insertPoint.Z);
                            Point3d pt2 = new Point3d(insertPoint.X - 2, insertPoint.Y, insertPoint.Z); ;
                            Point3d pt3 = new Point3d(insertPoint.X + 2, insertPoint.Y, insertPoint.Z);
                            Point3d pt4 = new Point3d(insertPoint.X - 2, insertPoint.Y - 3.5, insertPoint.Z);
                            Point3d pt5 = new Point3d(insertPoint.X + 2, insertPoint.Y - 3.5, insertPoint.Z);
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = pt1;
                                acAttDef.Prompt = "Ten lo: ";
                                acAttDef.Tag = "TL";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                                acTrans.AddNewlyCreatedDBObject(acAttDef, true);
                            }
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = pt2;
                                acAttDef.Prompt = "Dien tich: ";
                                acAttDef.Tag = "DT";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                                acTrans.AddNewlyCreatedDBObject(acAttDef, true);
                            }
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = pt3;
                                acAttDef.Prompt = "Mat do xay dung: ";
                                acAttDef.Tag = "MDXD";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                            }
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = pt4;
                                acAttDef.Prompt = "Tang cao: ";
                                acAttDef.Tag = "TC";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                                acTrans.AddNewlyCreatedDBObject(acAttDef, true);
                            }
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = pt5;
                                acAttDef.Prompt = "He so: ";
                                acAttDef.Tag = "HSSDD";
                                acAttDef.TextString = "";
                                acAttDef.Height = 1;
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlTblRec.AppendEntity(acAttDef);

                                acTrans.GetObject(acCurdb.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlTblRec);
                                acTrans.AddNewlyCreatedDBObject(acAttDef, true);
                            }
                        }
                        //save the new object to the database
                        acTrans.Commit();
                        //Dispose of the transaction 
                    }
                }
            }
        }

        static void WriteBoundaryArea(Database acCurdb, ObjectIdCollection collection)
        {
            double boundaryArea = GetBoundaryArea(acCurdb, collection);

            using (Transaction tr = acCurdb.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in collection)
                {
                    Entity ent = (Entity)tr.GetObject(id, OpenMode.ForWrite);

                    if (ent is BlockReference)
                    {
                        BlockReference blockRef = (BlockReference)ent;
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead);

                        // Tìm text object ở center của boundary
                        foreach (ObjectId subId in btr)
                        {
                            Entity subEnt = (Entity)tr.GetObject(subId, OpenMode.ForWrite);
                            if (subEnt is MText)
                            {
                                MText mtext = (MText)subEnt;
                                mtext.Contents = $"Boundary Area: {boundaryArea:F2} sq.units";
                                mtext.Attachment = AttachmentPoint.MiddleCenter;
                                break;
                            }
                        }
                    }
                }

                tr.Commit();
            }
        }

        private static double GetBoundaryArea(Database acCurdb, ObjectIdCollection collection)
        {
            double totalArea = 0;

            using (Transaction tr = acCurdb.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in collection)
                {
                    Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);

                    if (ent is Polyline)
                    {
                        Polyline poly = (Polyline)ent;
                        totalArea += poly.Area;
                    }
                    else if (ent is Circle)
                    {
                        Circle circle = (Circle)ent;
                        totalArea += Math.PI * circle.Radius * circle.Radius;
                    }
                    // Xử lý các loại đối tượng hình học khác tương tự
                }

                tr.Commit();
            }

            return totalArea;
        }
    }
}
