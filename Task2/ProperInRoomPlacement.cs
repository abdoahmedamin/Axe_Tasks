using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Task2
{
    [Transaction(TransactionMode.Manual)]
    public class ProperInRoomPlacement
    {
        string familyTypeName = "ADA";
        string roomName = "Bathroom";
        double offsetFromWall = 1.5;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uIDocument = commandData.Application.ActiveUIDocument;
            Document document = uIDocument.Document;

            try
            {
                Reference wallRef = uIDocument.Selection.PickObject(ObjectType.Element, new SelectionFilter("Walls"), "Pick a Wall");
                Wall wall = document.GetElement(wallRef) as Wall;

                //Get the suitable family symbol in the document
                FamilySymbol familySymbol = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol))
                                                                .Cast<FamilySymbol>()
                                                                .Where(e => e.Name == familyTypeName)
                                                                .FirstOrDefault();

                if (familySymbol == null)
                {
                    TaskDialog.Show("Error", $"Family Type '{familyTypeName}' not found.");
                    return Result.Failed;
                }

                // wall is boundry segment in ?? rooms
                BoundingBoxXYZ wallbb = wall.get_BoundingBox(null);
                Outline outline = new Outline(wallbb.Min, wallbb.Max);
                BoundingBoxIntersectsFilter boundingBoxIntersectsFilter = new BoundingBoxIntersectsFilter(outline);

                // all rooms with name contains Bathroom and intersext with the wall
                List<Room> rooms = new FilteredElementCollector(document, document.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rooms)
                                                                    .WherePasses(boundingBoxIntersectsFilter)
                                                                    .Cast<Room>()
                                                                    .Where(r => r.Name.Contains(roomName)).ToList();

                if (rooms.Count == 0)
                {
                    TaskDialog.Show("Error", "Wall is not Boundry For any BathRoom.");
                    return Result.Failed;
                }

                SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
                options.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreBoundary;

                List<BathRoom> bathRooms = GetBathroomsData(document, wall, rooms, document.ActiveView, options);


                using (Transaction tr = new Transaction(document))
                {
                    tr.Start("Family Placement");

                    foreach (var room in bathRooms)
                    {
                        if (room.DoorLocation == null || room == null) continue;

                        Curve curve = room.WallSegment;

                        XYZ startPoint = curve.GetEndPoint(0);
                        XYZ endPoint = curve.GetEndPoint(1);

                        double d1 = room.DoorLocation.DistanceTo(startPoint);
                        double d2 = room.DoorLocation.DistanceTo(endPoint);

                        XYZ placementPoint = d1 > d2 ? startPoint : endPoint;

                        if (!familySymbol.IsActive)
                        {
                            familySymbol.Activate();
                        }

                        XYZ vector = (d1 > d2 ? (endPoint - placementPoint) : (startPoint - placementPoint)).Normalize();

                        placementPoint = placementPoint + vector * offsetFromWall;

                        FamilyInstance familyInstance = document.Create.NewFamilyInstance(placementPoint, familySymbol, wall, StructuralType.NonStructural);

                        if (!IsFamilyFacingInSideRoom(familyInstance, placementPoint, room))
                            familyInstance.flipFacing();

                        if ((placementPoint.X > room.DoorLocation.X && IsParallel(familyInstance.FacingOrientation, XYZ.BasisY)
                            || (placementPoint.Y < room.DoorLocation.Y && IsParallel(familyInstance.FacingOrientation, XYZ.BasisX))))
                        {
                            familyInstance.flipHand();
                        }

                    }

                    tr.Commit();
                    TaskDialog.Show("Success", "Family Placement Done!");
                }
                 return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", "OOPS! Error Occurred: " + ex.Message);
                return Result.Failed;
            }

        }
        #region Methods

        private List<BathRoom> GetBathroomsData(Document doc, Wall wall, List<Room> rooms, View view, SpatialElementBoundaryOptions options)
        {
            if (wall == null || rooms.Count <= 0) return null;

            List<BathRoom> bathRooms = new List<BathRoom>();
            FamilyInstance door = null;

            foreach (Room room in rooms)
            {
                if (room == null) continue;

                BoundingBoxXYZ roombb = room.get_BoundingBox(null);
                Outline outline = new Outline(roombb.Min, roombb.Max);
                BoundingBoxIntersectsFilter boundingBoxIntersectsFilter = new BoundingBoxIntersectsFilter(outline);

                // bathRoom door
                door = new FilteredElementCollector(doc, view.Id)
                                              .OfCategory(BuiltInCategory.OST_Doors)
                                              .WherePasses(boundingBoxIntersectsFilter)
                                              .Cast<FamilyInstance>()
                                              .FirstOrDefault();


                BathRoom bathRoom = new BathRoom(room.Name);
                bathRoom.Center = (room.Location as LocationPoint).Point;

                // door location
                if (door != null)
                    bathRoom.DoorLocation = (door.Location as LocationPoint).Point;

                IList<IList<BoundarySegment>> boundarySegments = room.GetBoundarySegments(options);

                foreach (var boundarySegment in boundarySegments)
                {
                    foreach (var segment in boundarySegment)
                    {
                        // wall segment
                        if (segment.ElementId == wall.Id)
                            bathRoom.WallSegment = segment.GetCurve();
                    }
                }
                bathRooms.Add(bathRoom);
            }
            return bathRooms;
        }

        private bool IsFamilyFacingInSideRoom(FamilyInstance familyInstance, XYZ location, BathRoom room)
        {
            XYZ FamilyFacingDirection = familyInstance.FacingOrientation;
            XYZ familyToRoom = room.Center - location;

            if (familyToRoom.DotProduct(FamilyFacingDirection) > 0)
                return true;

            return false;
        }

        private bool IsParallel(XYZ vector1, XYZ vector2)
        {
            return vector1.DotProduct(vector2) == 1 || vector1.DotProduct(vector2) == -1;
        }
        #endregion

    }
}
