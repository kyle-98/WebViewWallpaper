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
                    Left = screens[i].Bounds.Left,
                    Top = screens[i].Bounds.Top,
                    Width = screens[i].Bounds.Width,
                    Height = screens[i].Bounds.Height
               };
          }

          return monitors;
     }
}
