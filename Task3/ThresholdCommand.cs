using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task3
{
    [Transaction(TransactionMode.Manual)]
    public class ThresholdCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uIDocument = commandData.Application.ActiveUIDocument;
            Document document = uIDocument.Document;

            TaskDialog.Show("Threshold Command", "This is a placeholder for the Door Threshold functionality.");

            try
            {
                if (!(document.ActiveView is ViewPlan viewPlan) || viewPlan.ViewType != ViewType.FloorPlan)
                {
                    TaskDialog.Show("Error", "run this command in a floor plan view.");
                    return Result.Failed;
                }

                //Get all rooms
                FilteredElementCollector allrooms = new FilteredElementCollector(document)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType();

                List<Room> rooms = allrooms.Cast<Room>()
                    .Where(r => r.Location != null && r.Area > 0)
                    .ToList();

                if (rooms.Count == 0)
                {
                    TaskDialog.Show("Error", "no rooms found in the document.");
                    return Result.Failed;
                }

                using (Transaction tr = new Transaction(document))
                {
                    tr.Start("Rooms Thresholds");

                    foreach (Room room in rooms)
                    {
                       
                            Level level = room.Level;

                            


                        
                    }

                }   
            }   
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", "An error occurred: " + ex.Message);
                return Result.Failed;
            }

            return Result.Succeeded;

        }
        #region Methods

        private Solid GetOrCreateRoomFloor(Document doc, Room room, Level level, out Floor outFloor, out double offset)
        {
            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
            IList<IList<BoundarySegment>> boundaries = room.GetBoundarySegments(options);
            if (boundaries == null || boundaries.Count == 0)
            {
                outFloor = null;
                offset = 0;
                return null;
            }

            List<CurveLoop> loops = new List<CurveLoop>();

            foreach (IList<BoundarySegment> boundaryList in boundaries)
            {
                CurveLoop loop = new CurveLoop();
                foreach (BoundarySegment segment in boundaryList)
                {
                    loop.Append(segment.GetCurve());
                }
                loops.Add(loop);
            }

            // no existing floor
            double floorThickness = 0.15;
            offset = 0;

            // all floors
            FilteredElementCollector floors = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Floors)
                .OfClass(typeof(Floor));

            // intersected floor if exist
            XYZ roomCentroid = (room.Location as LocationPoint).Point;
            Line intersectionLine = Line.CreateBound(
                roomCentroid,
                new XYZ(roomCentroid.X, roomCentroid.Y, roomCentroid.Z - 10));

            foreach (Floor floor in floors)
            {
                GeometryElement floorGeom = floor.get_Geometry(new Options());
                if (floorGeom == null) continue;

                foreach (GeometryObject obj in floorGeom)
                {
                    Solid floorSolid = obj as Solid;
                    if (floorSolid != null && floorSolid.Volume > 0)
                    {
                        SolidCurveIntersection intersection = floorSolid.IntersectWithCurve(
                            intersectionLine,
                            new SolidCurveIntersectionOptions());

                        if (intersection != null && intersection.SegmentCount > 0)
                        {
                            outFloor = floor;
                            floorThickness = floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();
                            offset = floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();

                            if (offset != 0)
                            {
                                XYZ translation = new XYZ(0, 0, offset);
                                Transform offsetTransform = Transform.CreateTranslation(translation);

                                List<CurveLoop> offsetLoops = new List<CurveLoop>();
                                foreach (CurveLoop loop in loops)
                                {
                                    offsetLoops.Add(CurveLoop.CreateViaTransform(loop, offsetTransform));
                                }

                                return GeometryCreationUtilities.CreateExtrusionGeometry(
                                    offsetLoops,
                                    XYZ.BasisZ.Negate(),
                                    floorThickness);
                            }

                            return floorSolid;
                        }
                    }
                }
            }


        }

        #endregion
    }
}
