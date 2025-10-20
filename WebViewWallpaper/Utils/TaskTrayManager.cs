using System.Windows;

namespace WebViewWallpaper.Utils
{
     public class TaskTrayManager
     {

          private static NotifyIcon _notifyIcon;
          public static event Action? OnSettingsClicked;
          public static event Action? OnReloadClicked;
          public static event Action? OnExitClicked;

          public static void Initialize()
          {
               if (_notifyIcon != null)
                    return;

               _notifyIcon = new()
               {
                    Icon = new System.Drawing.Icon("Resources/tasktrayicon.ico"),
                    Text = "WebView Wallpaper",
                    Visible = true
               };

               var contextMenu = new ContextMenuStrip();
               contextMenu.Items.Add("Settings", null, (s, e) => OnSettingsClicked?.Invoke());
               contextMenu.Items.Add(new ToolStripSeparator());
               contextMenu.Items.Add("Reload Wallpaper", null, (s, e) => OnReloadClicked?.Invoke());
               contextMenu.Items.Add(new ToolStripSeparator());
               contextMenu.Items.Add("Exit", null, (s, e) => OnExitClicked?.Invoke());

               _notifyIcon.ContextMenuStrip = contextMenu;

               _notifyIcon.DoubleClick += (s, e) =>
               {
                    
               };
          }

          private static void Exit_Click()
          {
               foreach (Window w in System.Windows.Application.Current.Windows)
               {
                    w.Close();
               }

               _notifyIcon.Visible = false;
               _notifyIcon.Dispose();
               System.Windows.Application.Current.Shutdown();
          }

          public static void Dispose()
          {
               if (_notifyIcon != null)
               {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
               }
          }
     }
}
