using Rhino;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
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
         host.Child = userControl;
         grid.Children.Add(host);

      }

      private void bImport_Click(object sender, RoutedEventArgs e) => GisUtil.copySelectedObjects(rhinoDoc);

      private void bExport_Click(object sender, RoutedEventArgs e)
      {

      }

      private void clickOpenRhinoFile(object sender, RoutedEventArgs e)
      {
         var ofd = new OpenFileDialog() { Filter = "3DM | *.3dm" };
      }

      private void clickSetLatLon(object sender, RoutedEventArgs e)
      {

      }

      private void clickZoomExtents(object sender, RoutedEventArgs e)
      {
         var ids = RhinoDoc.ActiveDoc.Objects.Select(o => o.Id).ToList();
         RhinoDoc.ActiveDoc.Objects.Select(ids);
         MessageBox.Show("IDS:\n" + string.Join("\n", ids.Select(i => i.ToString()).ToList()));
         RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.ZoomBoundingBox(new Rhino.Geometry.BoundingBox(new List<Rhino.Geometry.Point3d>()
         {
            new Rhino.Geometry.Point3d(1357650, 418700, -1),
            new Rhino.Geometry.Point3d(1357900, 418900, 1)
         }));
      }

        private void pushToMap(object sender, RoutedEventArgs e)
        {

        }
    }
}
