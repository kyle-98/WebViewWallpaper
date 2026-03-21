using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

internal static class Win32Interop
{
     #region Constants
     // Window Styles & Constants
     public const int GWL_STYLE = -16;                                // Index to get/set basic window styles
     public const int WS_CHILD = 0x40000000;                          // Style for a child window (sub-window)
     public const int WS_VISIBLE = 0x10000000;                        // Style for a window that is visible
     private const uint WM_SPAWN_WORKERW = 0x052C;                    // Message to force Windows to create a WorkerW layer
     public const long WS_CAPTION = 0x00C00000;                       // Style for a window with a title bar

     // Set Window Position Flags
     public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);       // Constant to push a window to the back of the Z-order
     public const uint SWP_NOMOVE = 0x0002;                           // Dont change window X/Y position
     public const uint SWP_NOSIZE = 0x0001;                           // Dont change window width/height
     public const uint SWP_NOZORDER = 0x0004;                         // Dont change the window's Z-order (depth)
     public const uint SWP_NOACTIVATE = 0x0010;                       // Dont focus/activate the window
     public const uint SWP_SHOWWINDOW = 0x0040;                       // Display the window if it was hidden

     // Window Messages
     private const int WM_WINDOWPOSCHANGED = 0x0047;                  // Sent to a window whose size, position, or Z-order has changed

     // Window Placement and Monitors
     public const int SW_SHOWMAXIMIZED = 3;                           // Command flag for a window being in maximized state
     public const uint MONITOR_DEFAULTTONULL = 0;                     // If window isnt on a monitor, return null instead of a default

     // Desktop Windows Manager Attributes
     public const int DWMWA_CLOAKED = 14;                             // Attribute to check if a window is cloaked. This is some bullshit windows does to UWP apps like settings instead of closing them fully
     #endregion

     #region Imports

     // Gets the handle (HWND) of the window currently in the foreground (focused)
     [DllImport("user32.dll")]
     public static extern IntPtr GetForegroundWindow();

     // Gets attributes for a window, such as its cloaked state
     [DllImport("dwmapi.dll")]
     public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);

     // Identifies which monitor a specific window belongs to
     [DllImport("user32.dll")]
     public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

     // Retrieves the state (maximized or minimized) of a specific window
     [DllImport("user32.dll", SetLastError = true)]
     [return: MarshalAs(UnmanagedType.Bool)]
     public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

     [Serializable]
     [StructLayout(LayoutKind.Sequential)]
     public struct WINDOWPLACEMENT
     {
          public int length;
          public int flags;
          public int showCmd;
          public Point ptMinPosition;
          public Point ptMaxPosition;
          public System.Windows.Rect rcNormalPosition;
     }

     // Checks if a window is flagged as visible
     [DllImport("user32.dll")]
     [return: MarshalAs(UnmanagedType.Bool)]
     public static extern bool IsWindowVisible(IntPtr hWnd);

     // Moves, resizes, or changes the Z-order of a window
     [DllImport("user32.dll")]
     public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
         int X, int Y, int cx, int cy, uint uFlags);

     // Retrieves information about a window
     [DllImport("user32.dll")]
     public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

     // Retrieves the title bar text of a window
     [DllImport("user32.dll", CharSet = CharSet.Auto)]
     public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

     // Changes the parent of a window
     [DllImport("user32.dll", SetLastError = true)]
     public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

     // Changes a window's attribute (hide the main application from alt + tab)
     [DllImport("user32.dll")]
     public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

     // Finds a window based on its class name or title bar text
     [DllImport("user32.dll", SetLastError = true)]
     public static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

     // Finds a child window within a parent window
     [DllImport("user32.dll", SetLastError = true)]
     public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

     // Sends a message to a window and waits for it to respond
     [DllImport("user32.dll", CharSet = CharSet.Auto)]
     public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam,
         uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

     // Loops through all windows on the screen
     [DllImport("user32.dll")]
     [return: MarshalAs(UnmanagedType.Bool)]
     public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

     // Delegate signature for the EnumWindows callback function
     public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

     [StructLayout(LayoutKind.Sequential)]
     public struct RECT
     {
          public int Left, Top, Right, Bottom;
     }

     // Gets the screen coordinates (bounding box) of a window
     [DllImport("user32.dll")]
     public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

     // Used for debugging only (Used to get the classname that is currently blocking the wallpaper from being animated)
     [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
     public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

     #endregion


     #region Helper Functions
     /// <summary>
     /// Forces Windows to create a WorkerW window behind the desktop icons to give the application a place to render on the desktop
     /// </summary>
     /// <returns>Returns a signed integer at the level where the window will spawn with a newly created workerW process</returns>
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


     /// <summary>
     /// A Window procedure hook that is called when Windows tries to move the wallpaper window's Z-order automatically which forces it back to the bottom.
     /// </summary>
     /// <param name="hwnd">The handle (unique ID) of the window receiving the message.</param>
     /// <param name="msg">The numerical ID of the Windows message being sent (ex: WM_WINDOWPOSCHANGED).</param>
     /// <param name="wParam">Additional message-specific information (word parameter).</param>
     /// <param name="lParam">Additional message-specific information (long parameter).</param>
     /// <param name="handled">A reference boolean; set to true if the message has been fully processed.</param>
     /// <returns>Returns an IntPtr.Zero if the message is handled, or calls the next hook in the chain.</returns>
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


     /// <summary>
     /// Modifies the window's extended style (GWL_EXSTYLE) to include the ToolWindow flag, which tells Windows to hide this specific window from alt + tab
     /// </summary>
     /// <param name="hwnd">The handle (unique ID) of the window to hide.</param>
     public static void HideFromAltTab(IntPtr hwnd)
     {
          const int GWL_EXSTYLE = -20;
          const int WS_EX_TOOLWINDOW = 0x00000080;

          IntPtr exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
          IntPtr newStyle = new IntPtr(exStyle.ToInt64() | WS_EX_TOOLWINDOW);
          SetWindowLongPtr(hwnd, GWL_EXSTYLE, newStyle);
     }


     /// <summary>
     /// Retrieves the internal Win32 class name of the specified window. This is used to filter out system-level layers like WorkerW or Progman
     /// </summary>
     /// <param name="hWnd">The handle (unique ID) of the window to query.</param>
     /// <returns>Returns a string containing the class name, or an empty string if the call fails.</returns>
     public static string GetWindowClassName(IntPtr hWnd)
     {
          StringBuilder sb = new StringBuilder(256);
          if (GetClassName(hWnd, sb, sb.Capacity) > 0)
          {
               return sb.ToString();
          }
          return string.Empty;
     }
     #endregion
}
