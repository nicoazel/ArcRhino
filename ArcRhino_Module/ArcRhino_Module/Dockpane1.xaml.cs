using Rhino.Runtime.InProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms.Integration;
using Microsoft.Win32;

namespace ArcRhino_Module
{
   /// <summary>
   /// Interaction logic for Dockpane1View.xaml
   /// </summary>
   public partial class Dockpane1View : UserControl
   {

      public Dockpane1View()
      {
         InitializeComponent();
      }

      private void UserControl_Loaded(object sender, RoutedEventArgs e)
      {
         WindowsFormsHost host = new WindowsFormsHost();
         Grid.SetRow(host, 1);
         var userControl = new UserControl1();
         host.Child = userControl ;
         grid.Children.Add(host);
         
      }

      private void bImport_Click(object sender, RoutedEventArgs e)
      {
         GisUtil.getFirstLayer();
      }

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
