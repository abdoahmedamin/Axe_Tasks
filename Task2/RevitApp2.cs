using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Task2
{
    public class RevitApp2 : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab("AXE_Revit_PLugins_Task2");
            }
            catch (Exception) { }

            RibbonPanel panel = null;
            try
            {
                panel = application.GetRibbonPanels("AXE_Revit_PLugins_Task2").FirstOrDefault(p => p.Name == "Tools");
            }
            catch (Exception) { }

            if (panel == null)
            {
                panel = application.CreateRibbonPanel("AXE_Revit_PLugins_Task2", "Tools");
            }

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            var buttonData = new PushButtonData(
                "Task2",
                "Task2\nProperInRoomPlacement",
                assemblyPath,
                typeof(ProperInRoomPlacement).FullName);

            Uri iconUri = new Uri("C:\\Users\\iti\\Desktop\\Axelirate_Tasks\\Axe_Tasks\\Task2\\resources\\toilet.ico");

            var buttonImage = new BitmapImage(iconUri);
            buttonData.LargeImage = buttonImage;

            var button = panel.AddItem(buttonData) as PushButton;
            button.ToolTip = "ProperInRoomPlacement";

            return Result.Succeeded;
        }
    }}
