using Rhino;
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
      RhinoDoc rhinoDoc => RhinoDoc.ActiveDoc ?? null;
      public Dockpane1View() => InitializeComponent();

      private void UserControl_Loaded(object sender, RoutedEventArgs e)
      {
         WindowsFormsHost host = new WindowsFormsHost();
         Grid.SetRow(host, 1);
         userControl = new UserControl1();
         userControl.Tag = this;
         host.Child = userControl;
         grid.Children.Add(host);

      }

      private void bImport_Click(object sender, RoutedEventArgs e) => GisUtil.copySelectedObjects(rhinoDoc);

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

                  RhinoUtil.ThrowItOverTheFence(thisLayer, rhobj);

               }//end for each
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

      }

      private void bExportGH_Click(object sender, RoutedEventArgs e)
      {
         GhUtil.showDocumentPreview();
      }
   }
}
