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
        #endregion
    }
}
