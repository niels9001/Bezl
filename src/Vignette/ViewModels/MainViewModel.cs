using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Vignette.Helpers;
using Vignette.Models;
using Vignette.Services;
using SkiaSharp;
using Windows.UI;

namespace Vignette.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    // Screenshot settings
    [ObservableProperty]
    private int _borderPadding = 16;

    [ObservableProperty]
    private double _cornerRadius = 0;

    // Preview image
    [ObservableProperty]
    private BitmapImage? _previewImage;

    // State
    [ObservableProperty]
    private bool _hasCapture;

    [ObservableProperty]
    private bool _isCapturing;

    [ObservableProperty]
    private string _statusText = "Ready — click \"Capture Window\" to start";

    // Gradient collection
    [ObservableProperty]
    private GradientItem? _selectedGradient;

    public ObservableCollection<GradientItem> Gradients { get; } = [];

    private CaptureResult? _captureResult;
    private SKBitmap? _composedBitmap;

    public MainPageViewModel()
    {
        LoadGradients();
    }

    private async void LoadGradients()
    {
        var definitions = GradientStore.Load();
        foreach (var def in definitions)
        {
            var item = new GradientItem(def);
            Gradients.Add(item);
        }

        // Render all previews
        foreach (var item in Gradients)
        {
            await item.RenderPreviewAsync();
        }

        if (Gradients.Count > 0)
        {
            SelectedGradient = Gradients[0];
            SelectedGradient.IsSelected = true;
        }
    }

    private void SaveGradients()
    {
        GradientStore.Save(Gradients.Select(g => g.ToDefinition()));
    }

    partial void OnBorderPaddingChanged(int value) => _ = RecomposeAsync();
    partial void OnCornerRadiusChanged(double value) => _ = RecomposeAsync();

    [RelayCommand]
    private void SelectGradient(GradientItem item)
    {
        if (SelectedGradient is not null)
            SelectedGradient.IsSelected = false;

        SelectedGradient = item;
        item.IsSelected = true;
    }

    [RelayCommand]
    private async Task EditGradientAsync(GradientItem item)
    {
        var dialog = new GradientEditorDialog(item.ToDefinition())
        {
            XamlRoot = App.Window.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && dialog.Result is not null)
        {
            item.UpdateFrom(dialog.Result);
            SelectGradient(item);
            SaveGradients();
            StatusText = $"Gradient \"{item.Name}\" updated";
        }
    }

    [RelayCommand]
    private async Task AddGradientAsync()
    {
        var dialog = GradientEditorDialog.CreateNew();
        dialog.XamlRoot = App.Window.Content.XamlRoot;

        var result = await dialog.ShowAsync();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && dialog.Result is not null)
        {
            var item = new GradientItem(dialog.Result);
            await item.RenderPreviewAsync();
            Gradients.Add(item);
            SelectGradient(item);
            SaveGradients();
            StatusText = $"Gradient \"{item.Name}\" added";
        }
    }

    [RelayCommand]
    private void DeleteGradient(GradientItem item)
    {
        if (Gradients.Count <= 1)
        {
            StatusText = "Cannot delete the last gradient";
            return;
        }

        var index = Gradients.IndexOf(item);
        Gradients.Remove(item);

        if (SelectedGradient == item)
        {
            var newIndex = Math.Min(index, Gradients.Count - 1);
            SelectGradient(Gradients[newIndex]);
        }

        SaveGradients();
        StatusText = $"Gradient \"{item.Name}\" deleted";
    }

    [RelayCommand]
    private void SetAsWallpaper()
    {
        if (SelectedGradient is null) return;

        try
        {
            WallpaperService.SetGradientAsWallpaper(SelectedGradient.ToDefinition());
            StatusText = $"Wallpaper set to \"{SelectedGradient.Name}\"!";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to set wallpaper: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CaptureWindowAsync()
    {
        try
        {
            IsCapturing = true;
            StatusText = "Select a window to capture...";

            var result = await WindowCaptureService.CaptureWithPickerAsync();
            if (result is null)
            {
                StatusText = "Capture cancelled";
                return;
            }

            _captureResult?.Dispose();
            _captureResult = result;
            HasCapture = true;
            StatusText = $"Captured: {result.WindowTitle}";

            await RecomposeAsync();
        }
        catch (Exception ex)
        {
            StatusText = $"Capture failed: {ex.Message}";
        }
        finally
        {
            IsCapturing = false;
        }
    }

    [RelayCommand]
    private async Task CopyToClipboardAsync()
    {
        if (_composedBitmap is null) return;

        try
        {
            await ImageExtensions.CopyToClipboardAsync(_composedBitmap);
            StatusText = "Copied to clipboard!";
        }
        catch (Exception ex)
        {
            StatusText = $"Copy failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveToFileAsync()
    {
        if (_composedBitmap is null) return;

        try
        {
            await ImageExtensions.SaveToFileAsync(_composedBitmap, App.WindowHandle);
            StatusText = "Screenshot saved!";
        }
        catch (Exception ex)
        {
            StatusText = $"Save failed: {ex.Message}";
        }
    }

    private async Task RecomposeAsync()
    {
        if (_captureResult is null) return;

        try
        {
            _composedBitmap?.Dispose();
            _composedBitmap = ScreenshotComposer.Compose(
                _captureResult.Bitmap,
                _captureResult.WindowRect,
                BorderPadding,
                (float)CornerRadius);

            PreviewImage = await _composedBitmap.ToWinUIBitmapAsync();
        }
        catch (Exception ex)
        {
            StatusText = $"Preview error: {ex.Message}";
        }
    }
}
