using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Task2;


namespace Task1
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
                "Task1",
                "Task1\nFloor Creator",
                assemblyPath,
                typeof(FloorCreator).FullName);

            Uri iconUri = new Uri("C:\\Users\\iti\\Desktop\\Axelirate_Tasks\\Axe_Tasks\\Task1\\resources\\icon1.ico");

            //AddButton(panel,
            //          assemblyPath,
            //          "Task1",
            //          "Task1\nFloor Creator",
            //          typeof(FloorCreator).FullName,
            //          @"C:\Users\iti\Desktop\Axelirate_Tasks\Axe_Tasks\Task1\resources\icon1.ico",
            //          "Floor Creator");

            //AddButton(panel,
            //         assemblyPath,
            //         "Task2",
            //         "Task2\nProper InRoom Placement",
            //          typeof(ProperInRoomPlacement).FullName,
            //         @"C:\\Users\\iti\\Desktop\\Axelirate_Tasks\\Axe_Tasks\\Task2\\resources\\toilet.ico",
            //         "Proper InRoom Placement");

            var buttonImage = new BitmapImage(iconUri);
            buttonData.LargeImage = buttonImage;

            var button = panel.AddItem(buttonData) as PushButton;
            button.ToolTip = "Floor Creator";

            return Result.Succeeded;
        }
        //private void AddButton(RibbonPanel panel, string assemblyPath, string internalName, string name,
        //                       string className, string iconPath, string tooltip)
        //{
        //    var buttonData = new PushButtonData(internalName, name, assemblyPath, className);
        //    Uri iconUri = new Uri(iconPath);
        //    buttonData.LargeImage = new BitmapImage(iconUri);
        //    var button = panel.AddItem(buttonData) as PushButton;
        //    button.ToolTip = tooltip;
        //}
    }
}
