using System.Runtime.InteropServices;
using ScreenShotter.Models;
using SkiaSharp;

namespace ScreenShotter.Services;

public static partial class WallpaperService
{
    private const int SPI_SETDESKWALLPAPER = 0x0014;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDCHANGE = 0x02;

    [LibraryImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

    [LibraryImport("user32.dll")]
    private static partial int GetSystemMetrics(int nIndex);

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    public static void SetGradientAsWallpaper(GradientDefinition gradient)
    {
        int screenW = GetSystemMetrics(SM_CXSCREEN);
        int screenH = GetSystemMetrics(SM_CYSCREEN);

        if (screenW <= 0 || screenH <= 0)
        {
            screenW = 1920;
            screenH = 1080;
        }

        using var bitmap = GradientService.Render(gradient, screenW, screenH);
        var wallpaperPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ScreenShotter", "wallpaper.bmp");

        Directory.CreateDirectory(Path.GetDirectoryName(wallpaperPath)!);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Bmp, 100);
        using var fs = File.Create(wallpaperPath);
        data.SaveTo(fs);

        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaperPath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
    }
}
