using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task4
{
    public class FramingWallCommand : IExternalCommand
    {
        double studTickness = 0.15;
        double studSpacing = 2.0;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;


            try
            {

                Reference wallRef = uiDocument.Selection.PickObject(ObjectType.Element, new SelectionFilter("Walls"), "Pick a Wall");
                Wall wall = document.GetElement(wallRef) as Wall;

                Solid wallSolid = GetWallSolid(wall);
                if (wallSolid == null)
                {
                    message = "Failed to get wall geometry.";
                    return Result.Failed;
                }

                // wall face
                Face wallFace = wallSolid.Faces.Cast<Face>().OrderByDescending(f => f.Area).FirstOrDefault();
                XYZ wallNormal = wallFace.ComputeNormal(new UV(0.5, 0.5));
                IList<CurveLoop> curveLoops = wallFace.GetEdgesAsCurveLoops();

                using (Transaction tr = new Transaction(document))
                {
                    tr.Start("Framing Wall");


                    tr.Commit();
                }
            }
            catch
            {

            }
        }

        #region Method
        private Solid GetWallSolid(Wall wall)
        {
            Options options = new Options();
            options.ComputeReferences = true;
            GeometryElement wallGeometry = wall.get_Geometry(options);

            return wallGeometry
                .Cast<GeometryObject>()
                .OfType<Solid>()
                .FirstOrDefault(solid => solid.Volume > 0);
        }

        private void CreateFraming(Document doc, Wall wall, Solid wallSolid, Face wallFace, XYZ wallNormal, IList<CurveLoop> curveLoops)
        {
            LocationCurve locCurve = wall.Location as LocationCurve;
            Curve wallCurve = locCurve.Curve;
            XYZ wallDir = (wallCurve.GetEndPoint(1) - wallCurve.GetEndPoint(0)).Normalize();
            double wallWidth = wall.Width;

            // vertical studs
            double spacing = UnitUtils.ConvertToInternalUnits(studSpacing, UnitTypeId.Feet);
            int pointCount = (int)(wallCurve.Length / spacing);

            for (int i = 1; i <= pointCount; i++)
            {
                double dist = i * spacing;
                if (Math.Abs(dist - wallCurve.Length) < 0.01) continue; // skip last point

                XYZ pointOnWall = wallCurve.Evaluate(dist / wallCurve.Length, true);
                CreateVerticalStudAtPoint(doc, pointOnWall, wallSolid, wallNormal, wallDir, wallWidth);
            }

            // bottom stud
            Transform moveTransform = Transform.CreateTranslation(wallNormal * wallWidth * 0.5);
            Curve movedCurve = wallCurve.CreateTransformed(moveTransform);

            Transform heightTransform = Transform.CreateTranslation(XYZ.BasisZ * studTickness);
            Curve offsetCurve = movedCurve.CreateTransformed(heightTransform);

            CreateModelCurve(doc, movedCurve, wallNormal, movedCurve.GetEndPoint(0));
            CreateModelCurve(doc, offsetCurve, wallNormal, offsetCurve.GetEndPoint(0));

            // uter studs
            CurveLoop outerLoop = curveLoops[0];
            foreach (Curve curve in outerLoop)
            {
                if (IsBottomEdge(curve)) continue; // skip bottom edge

                CreateStudPair(doc, curve, wallNormal);
            }

            // openings
            if (curveLoops.Count > 1)
            {
                IEnumerable<FamilyInstance> openings = GetWallOpenings(doc, wall);

                for (int i = 1; i < curveLoops.Count; ++i)
                {
                    CurveLoop openingLoop = curveLoops[i];
                    CreateOpeningFraming(doc, openingLoop, wallNormal,
                        openings.Any(o => o.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors));
                }
            }
        }

        

        #endregion
    }
}
