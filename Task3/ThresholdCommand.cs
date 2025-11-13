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



        }

        #endregion
    }
}
