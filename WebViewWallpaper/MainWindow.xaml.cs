using Microsoft.Web.WebView2.Core;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace WebViewWallpaper
{
     public partial class MainWindow : Window
     {

          private readonly MonitorHelper.MonitorInfo _monitorInfo;
          private readonly CoreWebView2Environment _env;
          private bool _isPaused = false;

          // Enable debugging output to the console
          private const bool IS_DEBUG = false;

          public MainWindow(MonitorHelper.MonitorInfo monitor, CoreWebView2Environment env)
          {
               InitializeComponent();
               _monitorInfo = monitor;
               _env = env;

               Left = _monitorInfo.Left;
               Top = _monitorInfo.Top;
               Width = _monitorInfo.Width;
               Height = _monitorInfo.Height;
               WindowState = WindowState.Normal;
          }

          #region Event Functions

          private void Window_SourceInitialized(object sender, EventArgs e)
          {
               var hwnd = new WindowInteropHelper(this).Handle;
               Win32Interop.HideFromAltTab(hwnd);
               var source = HwndSource.FromHwnd(hwnd);
               source.AddHook(Win32Interop.WndProc);
          }


          private async void Window_Loaded(object sender, RoutedEventArgs e)
          {
               // Initialize WebView2 and load the content
               SetupDesktopParent();

               await InitializeWebView();
          }

          private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
          {
               foreach(var window in System.Windows.Application.Current.Windows)
               {
                    if (window is MainWindow mw)
                    {
                         mw.WebViewControl.Dispose();
                    }
               }

               GC.Collect();
               GC.WaitForPendingFinalizers();
          }
          #endregion


          #region Helper Functions
          /// <summary>
          /// Setup the WPF application to be parented to the desktop
          /// </summary>
          private void SetupDesktopParent()
          {
               IntPtr desktopHandle = Win32Interop.GetDesktopWorkerW();

               if (desktopHandle != IntPtr.Zero)
               {
                    var hwnd = new WindowInteropHelper(this).Handle;

                    // Set parent
                    Win32Interop.SetParent(hwnd, desktopHandle);

                    Win32Interop.GetWindowRect(desktopHandle, out var workerRect);
                    Left = _monitorInfo.Left - workerRect.Left;
                    Top = _monitorInfo.Top - workerRect.Top;
                    Width = _monitorInfo.Width;
                    Height = _monitorInfo.Height;

                    IntPtr currentStyle = Win32Interop.GetWindowLongPtr(hwnd, Win32Interop.GWL_STYLE);
                    IntPtr newStyle = new(currentStyle.ToInt64() | Win32Interop.WS_CHILD | Win32Interop.WS_VISIBLE);
                    Win32Interop.SetWindowLongPtr(hwnd, Win32Interop.GWL_STYLE, newStyle);

                    // Set Z-order behind all windows
                    Win32Interop.SetWindowPos(
                        hwnd,
                        Win32Interop.HWND_BOTTOM,
                        0, 0, 0, 0,
                        Win32Interop.SWP_NOMOVE | Win32Interop.SWP_NOSIZE | Win32Interop.SWP_NOACTIVATE
                    );

                    Console.WriteLine("WPF Window successfully parented to the desktop.");
               }
               else
               {
                    System.Windows.MessageBox.Show("Could not find the desktop parent window. Wallpaper may not function correctly.", "Setup Warning");
               }
          }


          /// <summary>
          /// Initialize a webview
          /// </summary>
          /// <returns>Asynchronous task that is completed upon initializing the webview</returns>
          private async Task InitializeWebView()
          {
               // This ensures the webview is created properly 
               try
               {
                    await WebViewControl.EnsureCoreWebView2Async(_env);
                    WebViewControl.CoreWebView2.NavigationCompleted += (s, e) =>
                    {
                         _isPaused = !IsThisMonitorObscured();
                         UpdatePlaybackState();
                    };
                    WebViewControl.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                    WebViewControl.CoreWebView2.Settings.IsZoomControlEnabled = false;
               }
               catch (Exception ex)
               {
                    System.Windows.MessageBox.Show($"WebView2 Initialization failed: {ex.Message}", "WebView Error");
               }
          }


          /// <summary>
          /// Apply application settings input by the user
          /// </summary>
          /// <param name="URL">URL to the page the user saved in the settings dialog</param>
          public void ApplySettings(string URL)
          {
               try
               {
                    WebViewControl.Source = new Uri(URL);
               }
               catch(Exception ex)
               {
                    System.Windows.MessageBox.Show($"Error settings wallpaper: {ex.Message}", "Save Error");
                    return;
               }
          }


          /// <summary>
          /// Refresh the web view controller
          /// </summary>
          public void ReloadWallpaper()
          {
               if (WebViewControl != null && WebViewControl.CoreWebView2 != null)
               {
                    WebViewControl.Reload();
               }
          }


          /// <summary>
          /// Check if the desktop on a specific monitor is obscured by a window being maximized
          /// </summary>
          /// <returns>True if the monitor has a maximized window, false if it doesnt</returns>
          public bool IsThisMonitorObscured()
          {
               bool isObscured = false;
               IntPtr myHwnd = new WindowInteropHelper(this).Handle;

               Win32Interop.EnumWindows((hWnd, lParam) =>
               {
                    if (hWnd == myHwnd || !Win32Interop.IsWindowVisible(hWnd)) return true;

                    Win32Interop.DwmGetWindowAttribute(hWnd, Win32Interop.DWMWA_CLOAKED, out int cloaked, sizeof(int));
                    if (cloaked != 0) return true;

                    StringBuilder sb = new(512);
                    Win32Interop.GetWindowText(hWnd, sb, 512);
                    string title = sb.ToString();

                    StringBuilder sbClass = new(512);
                    Win32Interop.GetClassName(hWnd, sbClass, 512);
                    string className = sbClass.ToString();

                    if (string.IsNullOrWhiteSpace(title)) return true;

                    if (title == "Program Manager" ||
                        title == "Microsoft Text Input Application" ||
                        className == "WorkerW" ||
                        className == "Shell_TrayWnd")
                         return true;

                    Win32Interop.WINDOWPLACEMENT placement = new();
                    placement.length = Marshal.SizeOf(placement);
                    Win32Interop.GetWindowPlacement(hWnd, ref placement);

                    // 4. Check if maximized
                    if (placement.showCmd == Win32Interop.SW_SHOWMAXIMIZED)
                    {
                         if (Win32Interop.GetWindowRect(hWnd, out var rect))
                         {
                              int windowWidth = rect.Right - rect.Left;
                              int windowHeight = rect.Bottom - rect.Top;

                              int cx = rect.Left + (windowWidth / 2);
                              int cy = rect.Top + (windowHeight / 2);

                              if (cx >= _monitorInfo.Left && cx < (_monitorInfo.Left + _monitorInfo.Width) &&
                                  cy >= _monitorInfo.Top && cy < (_monitorInfo.Top + _monitorInfo.Height))
                              {
                                   if (windowWidth > (_monitorInfo.Width * 0.8))
                                   {
                                        // This will print out the name of the class that is blocking the wallpaper from animating
                                        if (IS_DEBUG)
                                             System.Diagnostics.Debug.WriteLine($"PAUSING: Monitor {_monitorInfo.Left} blocked by [{title}] Class: [{className}]");
                                        isObscured = true;
                                        return false;
                                   }
                              }
                         }
                    }
                    return true;
               }, IntPtr.Zero);

               return isObscured;
          }


          /// <summary>
          /// Update the playback state of video elements in the webview.
          /// </summary>
          public async void UpdatePlaybackState()
          {
               bool shouldPause = IsThisMonitorObscured();

               if (shouldPause == _isPaused) return;
               _isPaused = shouldPause;

               if (WebViewControl?.CoreWebView2 != null)
               {
                    try
                    {
                         string script = shouldPause
                             ? "document.querySelectorAll('video').forEach(v => v.pause());"
                             : "document.querySelectorAll('video').forEach(v => v.play());";

                         await WebViewControl.ExecuteScriptAsync(script);

                    }
                    catch { }
               }
          }
          #endregion
     }
}