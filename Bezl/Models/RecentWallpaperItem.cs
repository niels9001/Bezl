using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Bezl.Models;

public partial class RecentWallpaperItem : ObservableObject
{
    [ObservableProperty]
    private string _filePath;

    [ObservableProperty]
    private BitmapImage? _thumbnail;

    [ObservableProperty]
    private bool _isCurrentWallpaper;

    public RecentWallpaperItem(string filePath, bool isCurrent = false)
    {
        _filePath = filePath;
        _isCurrentWallpaper = isCurrent;
    }

    public async Task LoadThumbnailAsync()
    {
        try
        {
            if (!File.Exists(FilePath)) return;

            var bitmap = new BitmapImage
            {
                DecodePixelHeight = 100,
                DecodePixelWidth = 200
            };

            using var stream = File.OpenRead(FilePath);
            var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);
            memStream.Position = 0;

            var raStream = memStream.AsRandomAccessStream();
            await bitmap.SetSourceAsync(raStream);

            Thumbnail = bitmap;
        }
        catch { }
    }
}
