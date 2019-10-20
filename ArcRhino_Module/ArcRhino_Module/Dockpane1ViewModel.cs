using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Rhino.Runtime.InProcess;

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

      /// <summary>
      /// Text shown near the top of the DockPane.
      /// </summary>
      // private string _heading = "My DockPane";
      // public string Heading
      // {
      //    get { return _heading; }
      //    set
      //    {
      //       SetProperty(ref _heading, value, () => Heading);
      //    }
      // }

      #region Burger Button

      /// <summary>
      /// Tooltip shown when hovering over the burger button.
      /// </summary>
      public string BurgerButtonTooltip
      {
         get { return "Options"; }
      }

      /// <summary>
      /// Menu shown when burger button is clicked.
      /// </summary>
      public System.Windows.Controls.ContextMenu BurgerButtonMenu
      {
         get { return FrameworkApplication.CreateContextMenu(_menuID); }
      }
      #endregion
   }

   /// <summary>
   /// Button implementation to show the DockPane.
   /// </summary>
   internal class Dockpane1_ShowButton : Button
   {
      protected override void OnClick()
      {
         Dockpane1ViewModel.Show();
      }
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
