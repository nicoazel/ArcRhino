using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ArcRhino_Module
{
   internal class Dockpane1ViewModel : DockPane
   {
      private const string _dockPaneID = "ArcRhino_Module_Dockpane1";
      private const string _menuID = "ArcRhino_Module_Dockpane1_Menu";
      
      protected Dockpane1ViewModel() { }

      /// <summary>
      /// Show the DockPane.
      /// </summary>
      internal static void Show()
      {
         DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
         if (pane == null)
            return;

         pane.Activate();
      }
   }
}
