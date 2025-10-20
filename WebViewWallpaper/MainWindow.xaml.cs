using Microsoft.Web.WebView2.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using WebViewWallpaper.Settings;

namespace WebViewWallpaper
{
     public partial class MainWindow : Window
     {

          private MonitorHelper.MonitorInfo _monitorInfo;

          public MainWindow(MonitorHelper.MonitorInfo monitor)
          {
               InitializeComponent();
               _monitorInfo = monitor;

               Left = _monitorInfo.Left;
               Top = _monitorInfo.Top;
               Width = _monitorInfo.Width;
               Height = _monitorInfo.Height;
               WindowState = WindowState.Normal;
          }


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
                    IntPtr newStyle = new IntPtr(currentStyle.ToInt64() | Win32Interop.WS_CHILD | Win32Interop.WS_VISIBLE);
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

          private async Task InitializeWebView()
          {
               // This ensures the WebView2 Core environment is created. 
               try
               {
                    await WebViewControl.EnsureCoreWebView2Async(null);
               }
               catch (Exception ex)
               {
                    System.Windows.MessageBox.Show($"WebView2 Initialization failed: {ex.Message}", "WebView Error");
               }
          }


          public void ApplySettings(string URL)
          {
               string source;

               if (File.Exists(URL))
               {
                    string normalized = URL.Replace("\\", "/");
                    source = new Uri(normalized).AbsoluteUri;
               }
               else
               {
                    source = URL;
               }

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

          public void ReloadWallpaper()
          {
               if (WebViewControl != null && WebViewControl.CoreWebView2 != null)
               {
                    WebViewControl.Reload();
               }
          }
     }


}