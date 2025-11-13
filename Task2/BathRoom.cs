using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task2
{
    public class BathRoom
    {
        public string Name { get; set; }

        public Curve WallSegment { get; set; }

        public XYZ DoorLocation { get; set; } = null;

        public XYZ Center { get; set; }

        public BathRoom(string name)
        {
            Name = name;
        }
    }
}
