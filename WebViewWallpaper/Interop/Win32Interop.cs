using System.Diagnostics;
using System.Runtime.InteropServices;

public static class Win32Interop
{
     public const int GWL_STYLE = -16;
     public const int WS_CHILD = 0x40000000;
     public const int WS_VISIBLE = 0x10000000;
     private const uint WM_SPAWN_WORKERW = 0x052C;

     public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
     public const uint SWP_NOMOVE = 0x0002;
     public const uint SWP_NOSIZE = 0x0001;
     public const uint SWP_NOZORDER = 0x0004;
     public const uint SWP_NOACTIVATE = 0x0010;
     public const uint SWP_SHOWWINDOW = 0x0040;

     // P/Invoke for SetWindowPos
     [DllImport("user32.dll")]
     public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
         int X, int Y, int cx, int cy, uint uFlags);

     // P/Invoke for Get/SetWindowLongPtr
     [DllImport("user32.dll")]
     public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

     [DllImport("user32.dll", SetLastError = true)]
     public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

     [DllImport("user32.dll")]
     public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

     // --- Other Win32 P/Invoke for WorkerW ---
     [DllImport("user32.dll", SetLastError = true)]
     public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

     [DllImport("user32.dll", SetLastError = true)]
     public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

     [DllImport("user32.dll", CharSet = CharSet.Auto)]
     public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam,
         uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

     [DllImport("user32.dll")]
     [return: MarshalAs(UnmanagedType.Bool)]
     private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

     private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

     public static IntPtr GetDesktopWorkerW()
     {
          IntPtr progman = FindWindow("Progman", null);
          if (progman == IntPtr.Zero)
          {
               Debug.WriteLine("ProgMan not found.");
               return IntPtr.Zero;
          }

          // Send message to create WorkerW
          SendMessageTimeout(progman, WM_SPAWN_WORKERW, IntPtr.Zero, IntPtr.Zero, 0x0000, 1000, out _);

          IntPtr workerW = IntPtr.Zero;

          EnumWindows((topHandle, lParam) =>
          {
               IntPtr shellView = FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
               if (shellView != IntPtr.Zero)
               {
                    return true;
               }

               IntPtr worker = FindWindowEx(IntPtr.Zero, topHandle, "WorkerW", null);
               if (worker != IntPtr.Zero)
               {
                    workerW = topHandle; // parent WorkerW
                    return false; // stop enumeration
               }

               return true; // continue enumeration
          }, IntPtr.Zero);

          if (workerW == IntPtr.Zero)
          {
               Debug.WriteLine("Could not find WorkerW after spawning.");
          }

          return workerW;
     }
}
