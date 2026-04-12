using SkiaSharp;

namespace ScreenShotter.Models;

public sealed class CaptureResult : IDisposable
{
    public SKBitmap Bitmap { get; }
    public string WindowTitle { get; }

    public CaptureResult(SKBitmap bitmap, string windowTitle)
    {
        Bitmap = bitmap;
        WindowTitle = windowTitle;
    }

    public void Dispose() => Bitmap.Dispose();
}
