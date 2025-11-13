using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Task2
{
    public class RevitApp : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab("AXE_Revit_PLugins");
            }
            catch (Exception) { }

            RibbonPanel panel = null;
            try
            {
                panel = application.GetRibbonPanels("AXE_Revit_PLugins").FirstOrDefault(p => p.Name == "Tools");
            }
            catch (Exception) { }

            if (panel == null)
            {
                panel = application.CreateRibbonPanel("AXE_Revit_PLugins", "Tools");
            }

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            var buttonData = new PushButtonData(
                "Task2",
                "Task2\nProper InRoom Placement",
                assemblyPath,
                typeof(ProperInRoomPlacement).FullName);

            Uri iconUri = new Uri("C:\\Users\\iti\\Desktop\\Axelirate_Tasks\\Axe_Tasks\\Task2\\resources\\toilet.ico");

            var buttonImage = new BitmapImage(iconUri);
            buttonData.LargeImage = buttonImage;


            var button = panel.AddItem(buttonData) as PushButton;
            button.ToolTip = "Proper InRoom Placement";

            return Result.Succeeded;
        }
    }
}