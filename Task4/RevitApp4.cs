using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace Task4
{
    public class RevitApp4 : IExternalApplication
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
                "Task4",
                "Task4\nFraming Wall",
                assemblyPath,
                typeof(FramingWallCommand).FullName);

            Uri iconUri = new Uri("C:\\Users\\iti\\Desktop\\Axelirate_Tasks\\Axe_Tasks\\Task4\\resources\\icon4.ico");

            var buttonImage = new BitmapImage(iconUri);
            buttonData.LargeImage = buttonImage;

            var button = panel.AddItem(buttonData) as PushButton;
            button.ToolTip = "Framing Wall";

            return Result.Succeeded;
        }
    }
}