public class MonitorInfo
{
     public int Left, Top, Width, Height;
}

public static class MonitorHelper
{
     private static List<MonitorInfo> _cachedMonitors = [];
     public static List<MonitorInfo> CachedMonitors => _cachedMonitors;

     public class MonitorInfo
     {
          public int Left;
          public int Top;
          public int Width;
          public int Height;
     }


     public static void RefreshCache()
     {
          var screens = System.Windows.Forms.Screen.AllScreens;
          var newList = new List<MonitorInfo>();

          foreach (var screen in screens)
          {
               newList.Add(new MonitorInfo
               {
                    Left = screen.Bounds.Left,
                    Top = screen.Bounds.Top,
                    Width = screen.Bounds.Width,
                    Height = screen.Bounds.Height
               });
          }
          _cachedMonitors = newList;
     }
     

     public static MonitorInfo[] GetAllMonitors()
     {
          var screens = Screen.AllScreens;
          var monitors = new MonitorInfo[screens.Length];

          for (int i = 0; i < screens.Length; i++)
          {
               monitors[i] = new MonitorInfo
               {
                    Left = screens[i].WorkingArea.Left,
                    Top = screens[i].WorkingArea.Top,
                    Width = screens[i].WorkingArea.Width,
                    Height = screens[i].WorkingArea.Height
               };
          }

          return monitors;
     }
}
