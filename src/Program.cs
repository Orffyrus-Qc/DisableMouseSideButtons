using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DisableMouseSideButtons;

internal static class Program
{
    private const int WhMouseLl = 14;
    private const int WmXButtonDown = 0x020B;
    private const int WmXButtonUp = 0x020C;
    private const int WmXButtonDblClk = 0x020D;
    private const int WmNcXButtonDown = 0x00AB;
    private const int WmNcXButtonUp = 0x00AC;
    private const int WmNcXButtonDblClk = 0x00AD;

    private static readonly LowLevelMouseProc HookProc = OnMouse;
    private static IntPtr _hook;
    private static bool _blocking = true;
    private static NotifyIcon? _tray;
    private static ToolStripMenuItem? _toggleItem;
    private static Mutex? _mutex;

    [STAThread]
    private static void Main()
    {
        _mutex = new Mutex(true, @"Local\DisableMouseSideButtons", out bool createdNew);
        if (!createdNew)
            return;

        ApplicationConfiguration.Initialize();

        _toggleItem = new ToolStripMenuItem("Disable side buttons", null, (_, _) => ToggleBlocking())
        {
            Checked = true
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add(_toggleItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => Application.Exit());

        _tray = new NotifyIcon
        {
            Icon = SystemIcons.Shield,
            Visible = true,
            Text = "Mouse side buttons disabled",
            ContextMenuStrip = menu
        };
        _tray.DoubleClick += (_, _) => ToggleBlocking();

        if (!InstallHook())
        {
            MessageBox.Show(
                "Could not intercept mouse side buttons.\nWin32 error: " + Marshal.GetLastWin32Error(),
                "Disable Mouse Side Buttons",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            Cleanup();
            return;
        }

        Application.ApplicationExit += (_, _) => Cleanup();
        Application.Run();
    }

    private static void ToggleBlocking()
    {
        _blocking = !_blocking;
        if (_toggleItem is not null)
            _toggleItem.Checked = _blocking;
        if (_tray is not null)
            _tray.Text = _blocking ? "Mouse side buttons disabled" : "Mouse side buttons enabled";
    }

    private static bool InstallHook()
    {
        if (_hook != IntPtr.Zero)
            return true;

        _hook = SetWindowsHookEx(WhMouseLl, HookProc, GetModuleHandle(IntPtr.Zero), 0);
        return _hook != IntPtr.Zero;
    }

    private static void RemoveHook()
    {
        if (_hook == IntPtr.Zero)
            return;
        UnhookWindowsHookEx(_hook);
        _hook = IntPtr.Zero;
    }

    private static void Cleanup()
    {
        RemoveHook();
        if (_tray is not null)
        {
            _tray.Visible = false;
            _tray.Dispose();
            _tray = null;
        }

        if (_mutex is not null)
        {
            try { _mutex.ReleaseMutex(); } catch (ApplicationException) { }
            _mutex.Dispose();
            _mutex = null;
        }
    }

    private static IntPtr OnMouse(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _blocking)
        {
            int msg = (int)wParam;
            if (msg is WmXButtonDown or WmXButtonUp or WmXButtonDblClk
                    or WmNcXButtonDown or WmNcXButtonUp or WmNcXButtonDblClk)
            {
                return (IntPtr)1;
            }
        }

        return CallNextHookEx(_hook, nCode, wParam, lParam);
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(IntPtr lpModuleName);
}
