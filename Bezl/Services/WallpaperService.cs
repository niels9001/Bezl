using System.Runtime.InteropServices;
using Microsoft.Win32;
using Bezl.Models;
using SkiaSharp;

namespace Bezl.Services;

public static partial class WallpaperService
{
    private const int SPI_SETDESKWALLPAPER = 0x0014;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDCHANGE = 0x02;

    [LibraryImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

    [LibraryImport("user32.dll")]
    private static partial nint GetDC(nint hWnd);

    [LibraryImport("user32.dll")]
    private static partial int ReleaseDC(nint hWnd, nint hDC);

    [LibraryImport("gdi32.dll")]
    private static partial int GetDeviceCaps(nint hdc, int index);

    private const int DESKTOPHORZRES = 118; // physical width
    private const int DESKTOPVERTRES = 117; // physical height

    public static void SetGradientAsWallpaper(GradientDefinition gradient)
    {
        // Get physical screen resolution (not logical/DPI-scaled)
        var screenDc = GetDC(nint.Zero);
        int screenW = GetDeviceCaps(screenDc, DESKTOPHORZRES);
        int screenH = GetDeviceCaps(screenDc, DESKTOPVERTRES);
        ReleaseDC(nint.Zero, screenDc);

        if (screenW <= 0 || screenH <= 0)
        {
            screenW = 1920;
            screenH = 1080;
        }

        // Use the real user profile path (not MSIX-virtualized) so explorer.exe can read it
        var userProfile = Environment.GetEnvironmentVariable("USERPROFILE")
            ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var wallpaperPath = Path.Combine(userProfile, "Pictures", "Bezl", "wallpaper.jpg");

        Directory.CreateDirectory(Path.GetDirectoryName(wallpaperPath)!);

        // Render and save — must fully close the file before SystemParametersInfo reads it
        using (var bitmap = GradientService.Render(gradient, screenW, screenH))
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 95))
        {
            if (data is null)
                throw new InvalidOperationException("Failed to encode gradient image");

            using (var fs = File.Create(wallpaperPath))
            {
                data.SaveTo(fs);
            }
        }

        // Set wallpaper style to "Fill" (style=10, tileWallpaper=0)
        using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
        if (key is not null)
        {
            key.SetValue("WallpaperStyle", "10");
            key.SetValue("TileWallpaper", "0");
        }

        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaperPath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
    }

    public static void SetImageAsWallpaper(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Wallpaper image not found", filePath);

        // Set wallpaper style to "Fill"
        using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
        if (key is not null)
        {
            key.SetValue("WallpaperStyle", "10");
            key.SetValue("TileWallpaper", "0");
        }

        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filePath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
    }
}
