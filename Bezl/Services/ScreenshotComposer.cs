using SkiaSharp;

namespace Bezl.Services;

public static class ScreenshotComposer
{
    public static SKBitmap Compose(
        SKBitmap fullCapture,
        SKRectI windowRect,
        int borderPadding,
        float cornerRadius)
    {
        // Calculate crop region: window rect expanded by padding, clamped to bitmap bounds
        int cropLeft = Math.Max(windowRect.Left - borderPadding, 0);
        int cropTop = Math.Max(windowRect.Top - borderPadding, 0);
        int cropRight = Math.Min(windowRect.Right + borderPadding, fullCapture.Width);
        int cropBottom = Math.Min(windowRect.Bottom + borderPadding, fullCapture.Height);

        int cropW = cropRight - cropLeft;
        int cropH = cropBottom - cropTop;
        if (cropW <= 0 || cropH <= 0)
            return fullCapture.Copy();

        var result = new SKBitmap(cropW, cropH);
        using var canvas = new SKCanvas(result);

        // Draw the cropped region
        var srcRect = new SKRectI(cropLeft, cropTop, cropRight, cropBottom);
        var destRect = new SKRect(0, 0, cropW, cropH);

        if (cornerRadius > 0)
        {
            var rrect = new SKRoundRect(destRect, cornerRadius, cornerRadius);
            canvas.ClipRoundRect(rrect, antialias: true);
        }

        canvas.DrawBitmap(fullCapture, srcRect, destRect);

        return result;
    }

    public static byte[] EncodeToPng(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
