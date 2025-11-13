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

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", "OOPS! Error Occurred: " + ex.Message);
                return Result.Failed;
            }
        }

    }
}
