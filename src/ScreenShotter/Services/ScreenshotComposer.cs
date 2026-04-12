using ScreenShotter.Models;
using SkiaSharp;
using Windows.UI;

namespace ScreenShotter.Services;

public static class ScreenshotComposer
{
    public static SKBitmap Compose(
        SKBitmap windowCapture,
        Color gradientStartColor,
        Color gradientEndColor,
        double gradientAngle,
        int borderPadding,
        float cornerRadius)
    {
        int totalWidth = windowCapture.Width + borderPadding * 2;
        int totalHeight = windowCapture.Height + borderPadding * 2;

        // Render gradient background
        using var gradientBg = GradientService.RenderFromColors(
            gradientStartColor, gradientEndColor, gradientAngle, totalWidth, totalHeight);

        var result = new SKBitmap(totalWidth, totalHeight);
        using var canvas = new SKCanvas(result);

        // Draw gradient background
        canvas.DrawBitmap(gradientBg, 0, 0);

        // Draw window capture with optional corner radius
        var destRect = new SKRect(borderPadding, borderPadding,
            borderPadding + windowCapture.Width, borderPadding + windowCapture.Height);

        if (cornerRadius > 0)
        {
            // Clip to rounded rect, then draw
            var rrect = new SKRoundRect(destRect, cornerRadius, cornerRadius);
            canvas.Save();
            canvas.ClipRoundRect(rrect, antialias: true);
            canvas.DrawBitmap(windowCapture, destRect);
            canvas.Restore();
        }
        else
        {
            canvas.DrawBitmap(windowCapture, destRect);
        }

        return result;
    }

    public static byte[] EncodeToPng(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
