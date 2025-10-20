using System.Windows;
using System.Windows.Interop;
using WebViewWallpaper.Settings;
using WebViewWallpaper.Utils;

namespace WebViewWallpaper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
          private AppSettings _settings;


          protected override void OnStartup(StartupEventArgs e)
          {
               base.OnStartup(e);

               _settings = SettingsManager.Load();

               var monitors = MonitorHelper.GetAllMonitors();

               foreach (var monitor in monitors)
               {
                    var window = new MainWindow(monitor);
                    window.Show();
                    window.ApplySettings(_settings.URL);

                    var hwnd = new WindowInteropHelper(window).Handle;
                    Win32Interop.SetWindowPos(
                        hwnd,
                        Win32Interop.HWND_BOTTOM,
                        0, 0, 0, 0,
                        Win32Interop.SWP_NOMOVE | Win32Interop.SWP_NOSIZE | Win32Interop.SWP_NOACTIVATE | Win32Interop.SWP_SHOWWINDOW
                    );
               }

               TaskTrayManager.Initialize();
               TaskTrayManager.OnSettingsClicked += ShowSettingsWindow;
               TaskTrayManager.OnReloadClicked += ReloadWallpaper;
               TaskTrayManager.OnExitClicked += ExitApp;
          }

          private void ShowSettingsWindow()
          {
               var settingsWindow = new SettingsWindow(_settings);
               settingsWindow.OnUrlSaved += UpdateAllWallpapers;
               settingsWindow.ShowDialog();
          }

          private void ReloadWallpaper()
          {
               foreach (Window window in Current.Windows)
               {
                    if (window is MainWindow mw)
                    {
                         mw.ReloadWallpaper();
                    }
               }
          }

          private void UpdateAllWallpapers(string newUrl)
          {
               foreach (Window window in Current.Windows)
               {
                    if (window is MainWindow mw)
                    {
                         mw.ApplySettings(newUrl);
                    }
               }
          }

          private void ExitApp()
          {
               TaskTrayManager.Dispose();
               Current.Shutdown();
          }
     }

}
