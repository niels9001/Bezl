using SkiaSharp;

namespace ScreenShotter.Models;

public sealed class CaptureResult : IDisposable
{
    /// <summary>The full captured bitmap including surrounding desktop buffer.</summary>
    public SKBitmap Bitmap { get; }

    /// <summary>The window's rect within the full bitmap (excluding the buffer area).</summary>
    public SKRectI WindowRect { get; }

    public string WindowTitle { get; }

    public CaptureResult(SKBitmap bitmap, SKRectI windowRect, string windowTitle)
    {
        Bitmap = bitmap;
        WindowRect = windowRect;
        WindowTitle = windowTitle;
    }

    public void Dispose() => Bitmap.Dispose();
}
