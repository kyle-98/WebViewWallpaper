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

     private const int WM_WINDOWPOSCHANGED = 0x0047;

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
     public static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

     [DllImport("user32.dll", SetLastError = true)]
     public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

     [DllImport("user32.dll", CharSet = CharSet.Auto)]
     public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam,
         uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

     [DllImport("user32.dll")]
     [return: MarshalAs(UnmanagedType.Bool)]
     private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

     private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

     [StructLayout(LayoutKind.Sequential)]
     public struct RECT
     {
          public int Left, Top, Right, Bottom;
     }

     [DllImport("user32.dll")]
     public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

     public static IntPtr GetDesktopWorkerW()
     {
          // REFERENCE: https://web.archive.org/web/20250212211512/http://www.codeproject.com/Articles/856020/Draw-behind-Desktop-Icons-in-Windows
          // Find the Progman window
          IntPtr progman = FindWindow("Progman", null);
          if (progman == IntPtr.Zero)
          {
               Debug.WriteLine("ProgMan not found.");
               return IntPtr.Zero;
          }

          // Send the 0x052C message to spawn the WorkerW behind desktop icons
          // REFERENCE: https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendmessagetimeouta (SMTO_NORMAL = 0x0000)
          SendMessageTimeout(progman, WM_SPAWN_WORKERW, IntPtr.Zero, IntPtr.Zero, 0x0000, 1000, out _);

          IntPtr workerW = IntPtr.Zero;

          // Enumerate all top-level windows
          EnumWindows((topHandle, lParam) =>
          {
               // Check if this window contains the SHELLDLL_DefView (this contains the desktop icons)
               IntPtr shellView = FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
               if (shellView != IntPtr.Zero)
               {
                    // If found, grab the next WorkerW window after it
                    workerW = FindWindowEx(IntPtr.Zero, topHandle, "WorkerW", null);
                    return false; // stop enumeration
               }

               return true; // continue enumerating
          }, IntPtr.Zero);

          if (workerW == IntPtr.Zero)
               Debug.WriteLine("Could not find WorkerW after spawning.");

          return workerW;
     }


     public static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
     {
          if (msg == WM_WINDOWPOSCHANGED)
          {
               SetWindowPos(
                   hwnd,
                   HWND_BOTTOM,
                   0, 0, 0, 0,
                   SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE
               );
          }

          return IntPtr.Zero;
     }

     public static void HideFromAltTab(IntPtr hwnd)
     {
          const int GWL_EXSTYLE = -20;
          const int WS_EX_TOOLWINDOW = 0x00000080;

          IntPtr exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
          IntPtr newStyle = new IntPtr(exStyle.ToInt64() | WS_EX_TOOLWINDOW);
          SetWindowLongPtr(hwnd, GWL_EXSTYLE, newStyle);
     }
}
