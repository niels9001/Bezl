using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using Windows.Storage.Streams;

namespace Bezl.Helpers;

public static class ImageExtensions
{
    public static async Task<BitmapImage> ToWinUIBitmapAsync(this SKBitmap skBitmap)
    {
        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        var stream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
        {
            writer.WriteBytes(data.ToArray());
            await writer.StoreAsync();
        }

        stream.Seek(0);
        var bitmapImage = new BitmapImage();
        await bitmapImage.SetSourceAsync(stream);
        return bitmapImage;
    }

    public static async Task CopyToClipboardAsync(SKBitmap bitmap)
    {
        var pngData = Bezl.Services.ScreenshotComposer.EncodeToPng(bitmap);

        var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
        var stream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
        {
            writer.WriteBytes(pngData);
            await writer.StoreAsync();
        }
        stream.Seek(0);
        dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));

        Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
    }

    public static async Task SaveToFileAsync(SKBitmap bitmap, nint windowHandle)
    {
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
        var savePicker = new Microsoft.Windows.Storage.Pickers.FileSavePicker(windowId)
        {
            SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
            SuggestedFileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}",
        };
        savePicker.FileTypeChoices.Add("PNG Image", [".png"]);

        var result = await savePicker.PickSaveFileAsync();
        if (result is null || string.IsNullOrEmpty(result.Path)) return;

        var pngData = Bezl.Services.ScreenshotComposer.EncodeToPng(bitmap);
        await File.WriteAllBytesAsync(result.Path, pngData);
    }
}
