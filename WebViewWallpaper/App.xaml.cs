using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using WebViewWallpaper.Settings;
using WebViewWallpaper.Utils;
using static Win32Interop;

namespace WebViewWallpaper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
          private AppSettings _settings;
          private CoreWebView2Environment _sharedEnvironment;
          private Win32Interop.WinEventDelegate _winEventDelegate;
          private IntPtr _hookHandle;

          protected override async void OnStartup(StartupEventArgs e)
          {
               base.OnStartup(e);

               _settings = SettingsManager.Load();

               string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebViewWallpaper");

               var options = new CoreWebView2EnvironmentOptions
               {
                    AdditionalBrowserArguments = "--disable-features=CalculateNativeWinOcclusion"
               };
               _sharedEnvironment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);

               var monitors = MonitorHelper.GetAllMonitors();

               foreach (var monitor in monitors)
               {
                    var window = new MainWindow(monitor, _sharedEnvironment);
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

               SetupEventHook();
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
               if (_hookHandle != IntPtr.Zero)
                    Win32Interop.UnhookWinEvent(_hookHandle);
               TaskTrayManager.Dispose();
               Current.Shutdown();
          }

          private void SetupEventHook()
          {
               // Store the delegate in a class member to prevent GC
               _winEventDelegate = new Win32Interop.WinEventDelegate(WinEventCallback);

               // Listen for foreground changes and location changes (moves/maximizes)
               _hookHandle = Win32Interop.SetWinEventHook(
                   Win32Interop.EVENT_SYSTEM_FOREGROUND,
                   Win32Interop.EVENT_OBJECT_LOCATIONCHANGE,
                   IntPtr.Zero,
                   _winEventDelegate,
                   0, 0,
                   Win32Interop.WINEVENT_OUTOFCONTEXT);
          }

          private void WinEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
          {
               // Filter out non-window objects (like cursor moves or menu items)
               if (idObject != 0) return;

               // Trigger the visibility check across all wallpaper windows
               foreach (Window window in Current.Windows)
               {
                    if (window is MainWindow mw)
                    {
                         mw.UpdatePlaybackState();
                    }
               }
          }
     }

}
