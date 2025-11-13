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

                
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", "An error occurred: " + ex.Message);
                return Result.Failed;
            }

            return Result.Succeeded;

        }
    }
}
