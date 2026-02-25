using System.Runtime.InteropServices;

namespace Napominator;



public class GlobalMouseHook : IDisposable
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);



    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private IntPtr _hookID = IntPtr.Zero;
    public event Action OnMouseClick;

    private bool _disposed = false;
    private LowLevelMouseProc _proc;

    public void Start()
    {
        if (_hookID != IntPtr.Zero) return;

        _proc = HookCallback;
        _hookID = SetHook(_proc);

        if (_hookID == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to install hook. Error: {Marshal.GetLastWin32Error()}");
        }
    }
    public void Stop()
    {
        if (_hookID != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
        }
    }
    IntPtr SetHook(LowLevelMouseProc proc)
    {

        using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
        {
            var moduleName = curProcess.MainModule?.ModuleName;
            if (string.IsNullOrEmpty(moduleName))
            {
                // Fallback for scenarios where MainModule is null (services, etc.)
                moduleName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
            }

            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(moduleName), 0);
        }
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
        {
            try
            {
                OnMouseClick?.Invoke();
            }
            catch
            {
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }    

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
    }

    ~GlobalMouseHook()
    {
        Dispose();
    }



}
