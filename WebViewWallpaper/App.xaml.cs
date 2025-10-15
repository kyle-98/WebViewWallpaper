using System.Configuration;
using System.Data;
using System.Windows;

namespace WebViewWallpaper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
          protected override void OnStartup(StartupEventArgs e)
          {
               base.OnStartup(e);

               var monitors = MonitorHelper.GetAllMonitors();

               foreach (var mon in monitors)
               {
                    var window = new MainWindow(mon.Left, mon.Top, mon.Width, mon.Height);
                    window.Show();
               }
          }
     }

}
