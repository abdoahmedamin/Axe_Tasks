using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Task1
{
    [Transaction(TransactionMode.Manual)]
    public class FloorCreator : IExternalCommand
    {
        string typeName = "Cores - Mechanical Space";
        string levelName = "L1";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uIDocument = commandData.Application.ActiveUIDocument;
            Document document = uIDocument.Document;

            try
            {
                // convert to Feet
                double ToFeet(double meters) => meters * 3.28084;

                List<Line> lines = new List<Line>
                {
                    Line.CreateBound(new XYZ(ToFeet(0), ToFeet(0), 0), new XYZ(ToFeet(79), ToFeet(0), 0)),
                    Line.CreateBound(new XYZ(ToFeet(44), ToFeet(25), 0), new XYZ(ToFeet(13), ToFeet(25), 0)),
                    Line.CreateBound(new XYZ(ToFeet(13), ToFeet(40), 0), new XYZ(ToFeet(-8), ToFeet(40), 0)),
                    Line.CreateBound(new XYZ(ToFeet(55), ToFeet(34), 0), new XYZ(ToFeet(55), ToFeet(10), 0)),
                    Line.CreateBound(new XYZ(ToFeet(79), ToFeet(34), 0), new XYZ(ToFeet(55), ToFeet(34), 0)),
                    Line.CreateBound(new XYZ(ToFeet(0), ToFeet(20), 0), new XYZ(ToFeet(0), ToFeet(0), 0)),
                    Line.CreateBound(new XYZ(ToFeet(55), ToFeet(10), 0), new XYZ(ToFeet(44), ToFeet(12), 0)),
                    Line.CreateBound(new XYZ(ToFeet(-8), ToFeet(40), 0), new XYZ(ToFeet(-8), ToFeet(20), 0)),
                    Line.CreateBound(new XYZ(ToFeet(79), ToFeet(0), 0), new XYZ(ToFeet(79), ToFeet(34), 0)),
                    Line.CreateBound(new XYZ(ToFeet(44), ToFeet(12), 0), new XYZ(ToFeet(44), ToFeet(25), 0)),
                    Line.CreateBound(new XYZ(ToFeet(-8), ToFeet(20), 0), new XYZ(ToFeet(0), ToFeet(20), 0)),
                    Line.CreateBound(new XYZ(ToFeet(13), ToFeet(25), 0), new XYZ(ToFeet(13), ToFeet(40), 0))
                };

                CurveLoop curveLoop = CanConstructCurveLoop(lines) ?? ArrangeLinesToCurveLoop(lines);

                if (curveLoop == null)
                {
                    TaskDialog.Show("Error", "❌ Could not form a valid closed loop for the floor boundary.");
                    return Result.Failed;
                }

                // get the type and level of floor
                FloorType floorType = GetFloorType(document, typeName);
                Level level = GetLevel(document, levelName);

                if (floorType == null || level == null)
                {
                    TaskDialog.Show("Error", "❌ Could not find floor type or level in the project.");
                    ShowAvailableTypesAndLevels(document);
                    return Result.Failed;
                }

                using (Transaction tr = new Transaction(document, "Create Floor"))
                {
                    tr.Start();
                    Floor floor = Floor.Create(document, new List<CurveLoop> { curveLoop }, floorType.Id, level.Id);
                    tr.Commit();

                    TaskDialog.Show("Success", $"✅ Floor '{floorType.Name}' created on Level '{level.Name}'!");
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

        #region Helper Methods

        private CurveLoop CanConstructCurveLoop(List<Line> lines)
        {
            try
            {
                CurveLoop loop = new CurveLoop();
                foreach (Line line in lines)
                    loop.Append(line);

                if (!loop.IsOpen() && loop.IsCounterclockwise(new XYZ(0, 0, 1)))
                    return loop;

                lines.Reverse();
                CurveLoop reversedLoop = new CurveLoop();
                foreach (Line line in lines)
                    reversedLoop.Append(Line.CreateBound(line.GetEndPoint(1), line.GetEndPoint(0)));

                if (!reversedLoop.IsOpen() && reversedLoop.IsCounterclockwise(new XYZ(0, 0, 1)))
                    return reversedLoop;

                return null;
            }
            catch
            {
                return null;
            }
        }

        private CurveLoop ArrangeLinesToCurveLoop(List<Line> lines)
        {
            if (lines == null || lines.Count < 3)
                return null;

            CurveLoop loop = new CurveLoop();
            List<Line> remaining = new List<Line>(lines);
            Line firstLine = remaining[0];
            loop.Append(firstLine);
            remaining.RemoveAt(0);
            XYZ currentPoint = firstLine.GetEndPoint(1);

            while (remaining.Count > 0)
            {
                bool found = false;
                for (int i = 0; i < remaining.Count; i++)
                {
                    Line l = remaining[i];
                    XYZ start = l.GetEndPoint(0);
                    XYZ end = l.GetEndPoint(1);
                    if (currentPoint.IsAlmostEqualTo(start))
                    {
                        loop.Append(l);
                        currentPoint = end;
                        remaining.RemoveAt(i);
                        found = true;
                        break;
                    }
                    else if (currentPoint.IsAlmostEqualTo(end))
                    {
                        loop.Append(Line.CreateBound(end, start));
                        currentPoint = start;
                        remaining.RemoveAt(i);
                        found = true;
                        break;
                    }
                }
                if (!found) return null;
            }

            if (!firstLine.GetEndPoint(0).IsAlmostEqualTo(currentPoint))
                return null;

            return loop;
        }

        private FloorType GetFloorType(Document doc, string name)
        {
            FloorType type = new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .Cast<FloorType>()
                .FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (type == null)
            {
                type = new FilteredElementCollector(doc)
                           .OfClass(typeof(FloorType))
                           .Cast<FloorType>()
                           .FirstOrDefault();
            }

            return type;
        }

        private Level GetLevel(Document doc, string name)
        {
            Level level = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (level == null)
            {
                level = new FilteredElementCollector(doc)
                           .OfClass(typeof(Level))
                           .Cast<Level>()
                           .FirstOrDefault();
            }

            return level;
        }

        private void ShowAvailableTypesAndLevels(Document doc)
        {
            var floorTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .Cast<FloorType>()
                .Select(f => f.Name)
                .ToList();

            var levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .Select(l => l.Name)
                .ToList();

            string msg = "Available Floor Types:\n" + string.Join("\n", floorTypes) +
                         "\n\nAvailable Levels:\n" + string.Join("\n", levels);

            TaskDialog.Show("Available Types and Levels", msg);
        }

        #endregion
    }
}
