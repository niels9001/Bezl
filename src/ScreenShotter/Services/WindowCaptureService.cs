using System.Runtime.InteropServices;
using ScreenShotter.Models;
using SkiaSharp;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Microsoft.Graphics.Canvas;

namespace ScreenShotter.Services;

public static class WindowCaptureService
{
    public static async Task<CaptureResult?> CaptureWithPickerAsync()
    {
        var picker = new GraphicsCapturePicker();
        var hwnd = App.WindowHandle;
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var item = await picker.PickSingleItemAsync();
        if (item is null)
            return null;

        return await CaptureItemAsync(item);
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

        // Wait for a frame with timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        cts.Token.Register(() => tcs.TrySetResult(null));

        var frame = await tcs.Task;
        session.Dispose();

        if (frame is null)
        {
            framePool.Dispose();
            return null;
        }

        // Convert the captured frame to SKBitmap via Win2D
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

            // Make a copy since we're about to free the pinned handle
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
