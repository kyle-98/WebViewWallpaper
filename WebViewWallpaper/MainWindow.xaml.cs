using System.Windows;
using System.Windows.Interop;

namespace WebViewWallpaper
{
     public partial class MainWindow : Window
     {
          public MainWindow(int left, int top, int width, int height)
          {
               InitializeComponent();

               Left = left;
               Top = top;
               Width = width;
               Height = height;
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


          private void SetupDesktopParent()
          {
               IntPtr desktopHandle = Win32Interop.GetDesktopWorkerW();

               if (desktopHandle != IntPtr.Zero)
               {
                    var hwnd = new WindowInteropHelper(this).Handle;

                    // Set parent
                    Win32Interop.SetParent(hwnd, desktopHandle);

                    // Set Z-order behind all windows
                    Win32Interop.SetWindowPos(
                        hwnd,
                        Win32Interop.HWND_BOTTOM,
                        0, 0, 0, 0,
                        Win32Interop.SWP_NOMOVE | Win32Interop.SWP_NOSIZE | Win32Interop.SWP_NOACTIVATE
                    );

                    IntPtr currentStyle = Win32Interop.GetWindowLongPtr(hwnd, Win32Interop.GWL_STYLE);
                    IntPtr newStyle = new IntPtr(currentStyle.ToInt64() | Win32Interop.WS_CHILD | Win32Interop.WS_VISIBLE);
                    Win32Interop.SetWindowLongPtr(hwnd, Win32Interop.GWL_STYLE, newStyle);

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

                    string urlToLoad = "https://www.youtube.com";

                    WebViewControl.Source = new Uri(urlToLoad);
               }
               catch (Exception ex)
               {
                    System.Windows.MessageBox.Show($"WebView2 Initialization failed: {ex.Message}", "WebView Error");
               }
          }
     }


}