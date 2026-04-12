using ScreenShotter.Models;
using SkiaSharp;
using Windows.UI;

namespace ScreenShotter.Services;

public static class GradientService
{
    public static SKBitmap Render(GradientDefinition gradient, int width, int height)
    {
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        var angleRad = (float)(gradient.AngleDegrees * Math.PI / 180.0);
        var cos = (float)Math.Cos(angleRad);
        var sin = (float)Math.Sin(angleRad);

        var halfW = width / 2f;
        var halfH = height / 2f;
        var length = (float)Math.Sqrt(width * width + height * height) / 2f;

        var start = new SKPoint(halfW - cos * length, halfH - sin * length);
        var end = new SKPoint(halfW + cos * length, halfH + sin * length);

        using var shader = SKShader.CreateLinearGradient(
            start, end,
            [ToSkColor(gradient.StartColor), ToSkColor(gradient.EndColor)],
            [0f, 1f],
            SKShaderTileMode.Clamp);

        using var paint = new SKPaint { Shader = shader };
        canvas.DrawRect(0, 0, width, height, paint);

        return bitmap;
    }

    public static SKBitmap RenderFromColors(Color startColor, Color endColor, double angle, int width, int height)
    {
        var gradient = new GradientDefinition("Custom", startColor, endColor, angle);
        return Render(gradient, width, height);
    }

    private static SKColor ToSkColor(Color c) => new(c.R, c.G, c.B, c.A);
}
