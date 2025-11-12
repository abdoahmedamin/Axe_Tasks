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

                

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", "OOPS ! error occurred: " + ex.Message);
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

        #endregion





    }
}
