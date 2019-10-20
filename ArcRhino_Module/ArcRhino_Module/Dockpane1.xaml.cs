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

   }
}
