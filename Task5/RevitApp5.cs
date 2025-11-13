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

        }
    }
}