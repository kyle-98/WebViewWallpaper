public class MonitorInfo
{
     public int Left, Top, Width, Height;
}

public static class MonitorHelper
{
     public class MonitorInfo
     {
          public int Left;
          public int Top;
          public int Width;
          public int Height;
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
