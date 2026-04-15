using Microsoft.Win32;

namespace Bezl.Services;

public static class RecentWallpaperService
{
    private const string WallpaperRegPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers";

    public static List<string> GetRecentWallpapers()
    {
        var paths = new List<string>();

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(WallpaperRegPath);
            if (key is null) return paths;

            for (int i = 0; i < 5; i++)
            {
                var value = key.GetValue($"BackgroundHistoryPath{i}") as string;
                if (!string.IsNullOrEmpty(value) && File.Exists(value))
                {
                    paths.Add(value);
                }
            }
        }
        catch { }

        return paths;
    }

    public static string? GetCurrentWallpaperPath()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(WallpaperRegPath);
            return key?.GetValue("CurrentWallpaperPath") as string;
        }
        catch
        {
            return null;
        }
    }
}
