using System.Runtime.InteropServices;

namespace Bezl.Services;

public sealed partial class HotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID = 0xBEEF;

    // Modifier keys
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_NOREPEAT = 0x4000;

    // Virtual key codes
    private const uint VK_S = 0x53;

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(nint hWnd, int id);

    [LibraryImport("comctl32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowSubclass(nint hWnd, SubclassProc pfnSubclass, nuint uIdSubclass, nuint dwRefData);

    [LibraryImport("comctl32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RemoveWindowSubclass(nint hWnd, SubclassProc pfnSubclass, nuint uIdSubclass);

    [LibraryImport("comctl32.dll")]
    private static partial nint DefSubclassProc(nint hWnd, uint uMsg, nint wParam, nint lParam);

    private delegate nint SubclassProc(nint hWnd, uint uMsg, nint wParam, nint lParam, nuint uIdSubclass, nuint dwRefData);

    private readonly nint _hwnd;
    private readonly SubclassProc _subclassProc;
    private bool _registered;
    private bool _disposed;

    public event Action? HotkeyPressed;

    public HotkeyService(nint hwnd)
    {
        _hwnd = hwnd;
        // prevent the delegate from being GC'd
        _subclassProc = SubclassHandler;
    }

    public bool Register()
    {
        if (_registered) return true;

        _registered = RegisterHotKey(_hwnd, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT | MOD_NOREPEAT, VK_S);
        if (_registered)
        {
            SetWindowSubclass(_hwnd, _subclassProc, 0, 0);
        }
        return _registered;
    }

    public void Unregister()
    {
        if (!_registered) return;

        RemoveWindowSubclass(_hwnd, _subclassProc, 0);
        UnregisterHotKey(_hwnd, HOTKEY_ID);
        _registered = false;
    }

    private nint SubclassHandler(nint hWnd, uint uMsg, nint wParam, nint lParam, nuint uIdSubclass, nuint dwRefData)
    {
        if (uMsg == WM_HOTKEY && (int)wParam == HOTKEY_ID)
        {
            HotkeyPressed?.Invoke();
            return nint.Zero;
        }
        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Unregister();
    }
}
