using System.Runtime.InteropServices;
using Bezl.Models;
using SkiaSharp;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Microsoft.Graphics.Canvas;

namespace Bezl.Services;

public static partial class WindowCaptureService
{
    [LibraryImport("user32.dll")]
    private static partial nint GetForegroundWindow();

    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint FindWindow(string? lpClassName, string lpWindowName);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(nint hWnd, out RECT lpRect);

    [LibraryImport("user32.dll")]
    private static partial nint GetDC(nint hWnd);

    [LibraryImport("user32.dll")]
    private static partial int ReleaseDC(nint hWnd, nint hDC);

    [LibraryImport("gdi32.dll")]
    private static partial nint CreateCompatibleDC(nint hdc);

    [LibraryImport("gdi32.dll")]
    private static partial nint CreateCompatibleBitmap(nint hdc, int cx, int cy);

    [LibraryImport("gdi32.dll")]
    private static partial nint SelectObject(nint hdc, nint h);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool BitBlt(nint hdc, int x, int y, int cx, int cy, nint hdcSrc, int x1, int y1, uint rop);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteObject(nint ho);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteDC(nint hdc);

    [LibraryImport("gdi32.dll")]
    private static partial int GetDIBits(nint hdc, nint hbm, uint start, uint cLines, nint lpvBits, ref BITMAPINFO lpbmi, uint usage);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowTextW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int GetWindowText(nint hWnd, [Out] char[] lpString, int nMaxCount);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsIconic(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(nint hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(nint hWnd);

    private const int SW_RESTORE = 9;

    private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmGetWindowAttribute(nint hwnd, uint dwAttribute, out RECT pvAttribute, int cbAttribute);

    private const uint DWMWA_EXTENDED_FRAME_BOUNDS = 9;

    private const uint SRCCOPY = 0x00CC0020;
    private const uint DIB_RGB_COLORS = 0;
    private const int BI_RGB = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
    }

    private const int MAX_BUFFER = 200; // max padding the user can set

    [LibraryImport("user32.dll")]
    private static partial int GetSystemMetrics(int nIndex);

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;
    private const int SM_XVIRTUALSCREEN = 76;
    private const int SM_YVIRTUALSCREEN = 77;
    private const int SM_CXVIRTUALSCREEN = 78;
    private const int SM_CYVIRTUALSCREEN = 79;

    public static async Task<CaptureResult?> CaptureWithPickerAsync()
    {
        var picker = new GraphicsCapturePicker();
        var hwnd = App.WindowHandle;
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var item = await picker.PickSingleItemAsync();
        if (item is null)
            return null;

        // Minimize Bezl after the picker closes so it doesn't appear in the capture
        var presenter = App.Window.AppWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
        presenter?.Minimize();
        await Task.Delay(300);

        try
        {
            // Try to find the HWND to capture the screen region including shadow + buffer
            var targetHwnd = FindWindowByTitle(item.DisplayName);
            if (targetHwnd != nint.Zero)
            {
                // Restore and bring to front if the target window is minimized
                if (IsIconic(targetHwnd))
                {
                    ShowWindow(targetHwnd, SW_RESTORE);
                    SetForegroundWindow(targetHwnd);
                    await Task.Delay(400);
                }

                var result = CaptureScreenRegionWithBuffer(targetHwnd, item.DisplayName);
                if (result is not null)
                    return result;
            }

            // Fallback to item-based capture (without shadow/buffer)
            return await CaptureItemAsync(item);
        }
        finally
        {
            presenter?.Restore();
        }
    }

    private static nint FindWindowByTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            return nint.Zero;

        // Try exact match first
        var hwnd = FindWindow(null, title);
        if (hwnd != nint.Zero)
            return hwnd;

        // Enumerate all windows and find a partial match
        nint found = nint.Zero;
        EnumWindows((hWnd, _) =>
        {
            if (!IsWindowVisible(hWnd))
                return true;

            var buffer = new char[512];
            var len = GetWindowText(hWnd, buffer, buffer.Length);
            if (len > 0)
            {
                var windowTitle = new string(buffer, 0, len);
                if (windowTitle.Contains(title, StringComparison.OrdinalIgnoreCase) ||
                    title.Contains(windowTitle, StringComparison.OrdinalIgnoreCase))
                {
                    found = hWnd;
                    return false; // stop enumerating
                }
            }
            return true;
        }, nint.Zero);

        return found;
    }

    private static CaptureResult? CaptureScreenRegionWithBuffer(nint targetHwnd, string displayName)
    {
        if (!GetWindowRect(targetHwnd, out var windowRect))
            return null;

        // Get the VISUAL bounds (without invisible DWM borders) for equal padding
        RECT visualRect;
        int hr = DwmGetWindowAttribute(targetHwnd, DWMWA_EXTENDED_FRAME_BOUNDS,
            out visualRect, Marshal.SizeOf<RECT>());
        if (hr != 0)
            visualRect = windowRect; // fallback

        int winW = windowRect.Right - windowRect.Left;
        int winH = windowRect.Bottom - windowRect.Top;
        if (winW <= 0 || winH <= 0)
            return null;

        // Get virtual screen bounds (all monitors)
        int vsLeft = GetSystemMetrics(SM_XVIRTUALSCREEN);
        int vsTop = GetSystemMetrics(SM_YVIRTUALSCREEN);
        int vsRight = vsLeft + GetSystemMetrics(SM_CXVIRTUALSCREEN);
        int vsBottom = vsTop + GetSystemMetrics(SM_CYVIRTUALSCREEN);

        // Expand from the full window rect (includes shadow) by MAX_BUFFER
        int captureLeft = Math.Max(windowRect.Left - MAX_BUFFER, vsLeft);
        int captureTop = Math.Max(windowRect.Top - MAX_BUFFER, vsTop);
        int captureRight = Math.Min(windowRect.Right + MAX_BUFFER, vsRight);
        int captureBottom = Math.Min(windowRect.Bottom + MAX_BUFFER, vsBottom);

        int captureW = captureRight - captureLeft;
        int captureH = captureBottom - captureTop;
        if (captureW <= 0 || captureH <= 0)
            return null;

        // Use the VISUAL rect for the WindowRect (ensures equal padding on all sides)
        var windowRectInBitmap = new SKRectI(
            visualRect.Left - captureLeft,
            visualRect.Top - captureTop,
            visualRect.Right - captureLeft,
            visualRect.Bottom - captureTop);

        var screenDc = GetDC(nint.Zero);
        var memDc = CreateCompatibleDC(screenDc);
        var hBitmap = CreateCompatibleBitmap(screenDc, captureW, captureH);
        var oldBitmap = SelectObject(memDc, hBitmap);

        BitBlt(memDc, 0, 0, captureW, captureH, screenDc, captureLeft, captureTop, SRCCOPY);

        var bmi = new BITMAPINFO
        {
            bmiHeader = new BITMAPINFOHEADER
            {
                biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                biWidth = captureW,
                biHeight = -captureH, // top-down
                biPlanes = 1,
                biBitCount = 32,
                biCompression = BI_RGB
            }
        };

        var skBitmap = new SKBitmap(captureW, captureH, SKColorType.Bgra8888, SKAlphaType.Premul);
        GetDIBits(memDc, hBitmap, 0, (uint)captureH, skBitmap.GetPixels(), ref bmi, DIB_RGB_COLORS);
        MakeOpaque(skBitmap);

        SelectObject(memDc, oldBitmap);
        DeleteObject(hBitmap);
        DeleteDC(memDc);
        ReleaseDC(nint.Zero, screenDc);

        return new CaptureResult(skBitmap, windowRectInBitmap, displayName);
    }

    private static void MakeOpaque(SKBitmap bitmap)
    {
        var pixels = bitmap.GetPixels();
        int count = bitmap.Width * bitmap.Height;
        unsafe
        {
            var ptr = (byte*)pixels.ToPointer();
            for (int i = 0; i < count; i++)
            {
                ptr[i * 4 + 3] = 255; // set alpha to fully opaque
            }
        }
    }

    private static async Task<CaptureResult?> CaptureItemAsync(GraphicsCaptureItem item)
    {
        var device = CanvasDevice.GetSharedDevice();
        if (device is null)
            return null;

        IDirect3DDevice directDevice = device;

        var framePool = Direct3D11CaptureFramePool.Create(
            directDevice,
            DirectXPixelFormat.B8G8R8A8UIntNormalized,
            1,
            item.Size);

        var session = framePool.CreateCaptureSession(item);
        session.IsBorderRequired = false;

        var tcs = new TaskCompletionSource<Direct3D11CaptureFrame?>();

        framePool.FrameArrived += (pool, _) =>
        {
            var frame = pool.TryGetNextFrame();
            tcs.TrySetResult(frame);
        };

        session.StartCapture();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        cts.Token.Register(() => tcs.TrySetResult(null));

        var frame = await tcs.Task;
        session.Dispose();

        if (frame is null)
        {
            framePool.Dispose();
            return null;
        }

        var canvasBitmap = CanvasBitmap.CreateFromDirect3D11Surface(device, frame.Surface);
        var pixels = canvasBitmap.GetPixelBytes();
        var width = (int)canvasBitmap.SizeInPixels.Width;
        var height = (int)canvasBitmap.SizeInPixels.Height;

        var skBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        var gcHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            skBitmap.InstallPixels(
                new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul),
                gcHandle.AddrOfPinnedObject(),
                width * 4);

            var copy = skBitmap.Copy();
            skBitmap.Dispose();

            frame.Dispose();
            framePool.Dispose();

            // Fallback: entire bitmap IS the window (no buffer)
            var fullRect = new SKRectI(0, 0, copy.Width, copy.Height);
            return new CaptureResult(copy, fullRect, item.DisplayName);
        }
        finally
        {
            gcHandle.Free();
        }
    }

    public static CaptureResult? CaptureForegroundWindow()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == nint.Zero || hwnd == App.WindowHandle)
            return null;

        var buffer = new char[512];
        var len = GetWindowText(hwnd, buffer, buffer.Length);
        var title = len > 0 ? new string(buffer, 0, len) : "Unknown";

        return CaptureScreenRegionWithBuffer(hwnd, title);
    }
}
