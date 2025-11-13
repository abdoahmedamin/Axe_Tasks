using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task4
{
    public class SelectionFilter : ISelectionFilter
    {
        private string categoryName;
        public SelectionFilter(string categoryName)
        {
            this.categoryName = categoryName;
        }
        public bool AllowElement(Element elem)
        {
            return elem.Category != null && elem.Category.Name == categoryName;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
