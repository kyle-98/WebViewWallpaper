using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Interop;

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

               foreach (var monitor in monitors)
               {
                    var window = new MainWindow(monitor);
                    window.Show();

                    var hwnd = new WindowInteropHelper(window).Handle;
                    Win32Interop.SetWindowPos(
                        hwnd,
                        Win32Interop.HWND_BOTTOM,
                        0, 0, 0, 0,
                        Win32Interop.SWP_NOMOVE | Win32Interop.SWP_NOSIZE | Win32Interop.SWP_NOACTIVATE | Win32Interop.SWP_SHOWWINDOW
                    );
               }
          }
     }

}
