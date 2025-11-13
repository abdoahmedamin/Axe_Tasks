using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace Task5
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RevitApp5 : IExternalApplication
    {
        private static HashSet<ElementId> _processedSections = new HashSet<ElementId>();

        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentChanged += OnDocumentChanged;
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentChanged -= OnDocumentChanged;
            return Result.Succeeded;
        }

        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            Document doc = e.GetDocument();

            View activeView = doc.ActiveView;

            Level viewLevel = activeView.GenLevel;

            if (viewLevel == null)
                return;

            foreach (ElementId addedId in e.GetAddedElementIds())
            {
                Element addedElement = doc.GetElement(addedId);

                if (addedElement is ViewSection viewSection &&
                    viewSection.ViewType == ViewType.Section &&
                    !_processedSections.Contains(addedId))
                { 
                    _processedSections.Add(addedId);

                    using (Transaction tx = new Transaction(doc, "Adjust Section Crop Box"))
                    {
                        tx.Start();

                        try
                        {

                        }
                        catch (Exception ex)
                        {
                            tx.RollBack();
                            TaskDialog.Show("Error", ex.Message);
                            return;
                        }

                        tx.Commit();
                    }

                }



            }
        }



    }
}