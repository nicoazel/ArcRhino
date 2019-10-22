﻿using Rhino;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Microsoft.Win32;

namespace ArcRhino_Module
{
   /// <summary>
   /// Interaction logic for Dockpane1View.xaml
   /// </summary>
   public partial class Dockpane1View : UserControl
   {
      UserControl1 userControl;
      internal RhinoDoc rhinoDoc => RhinoDoc.ActiveDoc ?? null;
      public Dockpane1View()
      {
         InitializeComponent();
      }

      private void UserControl_Loaded(object sender, RoutedEventArgs e)
      {
         WindowsFormsHost host = new WindowsFormsHost();
         Grid.SetRow(host, 1);
         userControl = new UserControl1() { Tag = this }; // set Tag to access autoupdate check state
         host.Child = userControl;
         grid.Children.Add(host);

      }

      private void bImport_Click(object sender, RoutedEventArgs e) => GisUtil.copySelectedObjects(rhinoDoc);
      private void bExportGH_Click(object sender, RoutedEventArgs e) => GhUtil.showDocumentPreview(rhinoDoc);

      private void bExport_Click(object sender, RoutedEventArgs e)
      {
         var t = QueuedTask.Run(() =>
         {
            if (rhinoDoc != null)
            {
               //Rhino.DocObjects.ObjRef[] obref;
               //Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetMultipleObjects("Select object", true, Rhino.DocObjects.ObjectType.AnyObject, out obref);

               //foreach (var obj in obref)
               foreach (var rhobj in rhinoDoc.Objects)
                  {

                  //Rhino.DocObjects.RhinoObject rhobj = obj.Object();
                  int layerIndex = rhobj.Attributes.LayerIndex;
                  string layerName = rhinoDoc.Layers[layerIndex].Name;
                  MessageBox.Show($"Got layer {layerName} with selected features");
                  var thisLayer = MapView.Active.Map.FindLayers(layerName).FirstOrDefault() as BasicFeatureLayer;
                  thisLayer = (thisLayer == null) ? MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault() as BasicFeatureLayer : thisLayer;

                  RhinoUtil.ThrowItOverTheFence(thisLayer, rhobj, rhinoDoc);
               }
            }// end push layer to map
         });
      }

      private void clickOpenRhinoFile(object sender, RoutedEventArgs e)
      {
         var ofd = new OpenFileDialog() { Filter = "3DM | *.3dm" };
         ofd.ShowDialog();
      }

      private void clickSetLatLon(object sender, RoutedEventArgs e)
      {
         // TODO: 
         string todo = "Feature not yet implemented. Coming soon." +
            "\n1.  Get center X,Y from ArcGIS map" +
            "\n2.  Cache that to Rhino document properties/user text" +
            "\n3.  Use that reference to transform to/from ArcGIS and Rhino";
         MessageBox.Show(todo);
      }

      private void clickHelp(object sender, RoutedEventArgs e)
      {
         var splash = new Splash();
         splash.ShowDialog();
      }
   }
}
