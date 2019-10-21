using System.Runtime.CompilerServices;
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

      static Dockpane1ViewModel()
      {
         RhinoInside.Resolver.Initialize();
         // MessageBox.Show("The Rhino has landed!", "ArcRhino");
      }

      protected override void NotifyPropertyChanged([CallerMemberName] string name = "")
      {
         // MessageBox.Show("Notify ")
         base.NotifyPropertyChanged(name);
      }

   }

   /// <summary>
   /// Button implementation to show the DockPane.
   /// </summary>
   internal class Dockpane1_ShowButton : Button
   {
      protected override void OnClick() => Dockpane1ViewModel.Show();
   }

   /// <summary>
   /// Button implementation for the button on the menu of the burger button.
   /// </summary>
   internal class Dockpane1_MenuButton : Button
   {
      protected override void OnClick()
      {
      }
   }
}
