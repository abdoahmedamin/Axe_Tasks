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
        string typeName = "Generic 300mm";
        string levelName = "Level 1";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uIDocument = commandData.Application.ActiveUIDocument;
            Document document = uIDocument.Document;

            try
            {
                List<Line> lines = new List<Line>
                {
                    Line.CreateBound(new XYZ(0, 0, 0), new XYZ(79, 0, 0)),
                    Line.CreateBound(new XYZ(44, 25, 0), new XYZ(13, 25, 0)),
                    Line.CreateBound(new XYZ(13, 40, 0), new XYZ(-8, 40, 0)),
                    Line.CreateBound(new XYZ(55, 34, 0), new XYZ(55, 10, 0)),
                    Line.CreateBound(new XYZ(79, 34, 0), new XYZ(55, 34, 0)),
                    Line.CreateBound(new XYZ(0, 20, 0), new XYZ(0, 0, 0)),
                    Line.CreateBound(new XYZ(55, 10, 0), new XYZ(44, 12, 0)),
                    Line.CreateBound(new XYZ(-8, 40, 0), new XYZ(-8, 20, 0)),
                    Line.CreateBound(new XYZ(79, 0, 0), new XYZ(79, 34, 0)),
                    Line.CreateBound(new XYZ(44, 12, 0), new XYZ(44, 25, 0)),
                    Line.CreateBound(new XYZ(-8, 20, 0), new XYZ(0, 20, 0)),
                    Line.CreateBound(new XYZ(13, 25, 0), new XYZ(13, 40, 0))
                };

                //check if lines can make a curve loop
                CurveLoop curveLoop = CanConstructCurveLoop(lines);
               
                if (curveLoop == null)
                {
                    curveLoop = ArrangeLinesToCurveLoop(lines);

                    if (curveLoop == null)
                    {
                        return Result.Failed;
                    }
                }

                using (Transaction tr = new Transaction(document))
                {
                    tr.Start("Create Floor");

                    var floor = CreateFloor(document, curveLoop, levelName, typeName);
                    if (floor == null)
                    {
                        return Result.Failed;
                    }

                    tr.Commit();

                    TaskDialog.Show("Success", "Floor Created Successfully!");
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", "OOPS ! Error Occurred: " + ex.Message);
                return Result.Failed;
            }
        }
        #region Methods

        private CurveLoop CanConstructCurveLoop(List<Line> lines)
        {
            CurveLoop curveLoop = new CurveLoop();
            try
            {
                foreach (Line line in lines)
                {
                    curveLoop.Append(line);
                }

                if (!curveLoop.IsOpen() && curveLoop.IsCounterclockwise(new XYZ(0, 0, 1)))
                    return curveLoop;

                lines.Reverse();
                CurveLoop reversedLoop = new CurveLoop();
                foreach (Line line in lines)
                {
                    reversedLoop.Append(Line.CreateBound(line.GetEndPoint(1), line.GetEndPoint(0)));
                }
                if (!reversedLoop.IsOpen() && reversedLoop.IsCounterclockwise(new XYZ(0, 0, 1)))
                    return reversedLoop;

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        private CurveLoop ArrangeLinesToCurveLoop(List<Line> lines)
        {
            if (lines == null || lines.Count < 3)
                return null;

            CurveLoop curveLoop = new CurveLoop();

            List<Line> remaining = new List<Line>(lines);
            Line firstLine = remaining[0];
            curveLoop.Append(firstLine);

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
                        curveLoop.Append(l);
                        currentPoint = end;
                        remaining.RemoveAt(i);
                        found = true;
                        break;
                    }
                    else if (currentPoint.IsAlmostEqualTo(end))
                    {
                        Line reversed = Line.CreateBound(end, start);
                        curveLoop.Append(reversed);
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

            return curveLoop;
        }

        private Floor CreateFloor(Document doc, CurveLoop curveLoop, string levelName, string typeName)
        {
            FloorType floorType = new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .Cast<FloorType>()
                .Where(f => f.Name == typeName).FirstOrDefault();

            Level level = new FilteredElementCollector(doc)
                                .OfClass(typeof(Level))
                                .Cast<Level>()
                                .Where(l => l.Name == levelName).FirstOrDefault();

            if (floorType == null || level == null)
            {
                TaskDialog.Show("Error", "Could not find floor type or level.");
                return null;
            }

            return Floor.Create(doc, new List<CurveLoop> { curveLoop }, floorType.Id, level.Id);
        }
        #endregion





    }
}
