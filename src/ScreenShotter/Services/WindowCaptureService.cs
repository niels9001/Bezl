using System.Runtime.InteropServices;
using ScreenShotter.Models;
using SkiaSharp;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Microsoft.Graphics.Canvas;

namespace ScreenShotter.Services;

public static partial class WindowCaptureService
{
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

    private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

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

    public static async Task<CaptureResult?> CaptureWithPickerAsync()
    {
        var picker = new GraphicsCapturePicker();
        var hwnd = App.WindowHandle;
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var item = await picker.PickSingleItemAsync();
        if (item is null)
            return null;

        // Try to find the HWND to capture the screen region including window shadow
        var targetHwnd = FindWindowByTitle(item.DisplayName);
        if (targetHwnd != nint.Zero)
        {
            var result = CaptureScreenRegion(targetHwnd, item.DisplayName);
            if (result is not null)
                return result;
        }

        // Fallback to item-based capture (without shadow)
        return await CaptureItemAsync(item);
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

    private static CaptureResult? CaptureScreenRegion(nint targetHwnd, string displayName)
    {
        if (!GetWindowRect(targetHwnd, out var rect))
            return null;

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0)
            return null;

        var screenDc = GetDC(nint.Zero);
        var memDc = CreateCompatibleDC(screenDc);
        var hBitmap = CreateCompatibleBitmap(screenDc, width, height);
        var oldBitmap = SelectObject(memDc, hBitmap);

        BitBlt(memDc, 0, 0, width, height, screenDc, rect.Left, rect.Top, SRCCOPY);

        // Read pixels into an SKBitmap
        var bmi = new BITMAPINFO
        {
            bmiHeader = new BITMAPINFOHEADER
            {
                biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                biWidth = width,
                biHeight = -height, // top-down
                biPlanes = 1,
                biBitCount = 32,
                biCompression = BI_RGB
            }
        };

        var skBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        GetDIBits(memDc, hBitmap, 0, (uint)height, skBitmap.GetPixels(), ref bmi, DIB_RGB_COLORS);

        // Ensure fully opaque (screen capture has A=0 for BGRA)
        MakeOpaque(skBitmap);

        SelectObject(memDc, oldBitmap);
        DeleteObject(hBitmap);
        DeleteDC(memDc);
        ReleaseDC(nint.Zero, screenDc);

        return new CaptureResult(skBitmap, displayName);
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

            return new CaptureResult(copy, item.DisplayName);
        }
        finally
        {
            gcHandle.Free();
        }
    }
}
