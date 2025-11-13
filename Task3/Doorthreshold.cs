using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task3
{
    public class Doorthreshold
    {
        public XYZ Locatin { get; set; }

        public double Width { get; set; }

        public double Depth { get; set; }

        public Room Room { get; set; }

        public Wall HostWall { get; set; }

        public FamilyInstance Door { get; set; }
    }
}
