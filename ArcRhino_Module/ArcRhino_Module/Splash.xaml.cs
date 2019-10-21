using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ArcRhino_Module
{
   /// <summary>
   /// Interaction logic for Splash.xaml
   /// </summary>
   public partial class Splash : Window
   {
      public Splash() => InitializeComponent();

      private void close(object sender, MouseButtonEventArgs e) => Close();

      private void clickUrl(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
      {
         Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
         e.Handled = true;
      }
   }
}
