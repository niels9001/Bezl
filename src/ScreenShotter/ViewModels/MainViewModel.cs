using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using ScreenShotter.Helpers;
using ScreenShotter.Models;
using ScreenShotter.Services;
using SkiaSharp;
using Windows.UI;

namespace ScreenShotter.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    // Screenshot settings
    [ObservableProperty]
    private int _borderPadding = 40;

    [ObservableProperty]
    private double _cornerRadius = 0;

    // Gradient settings
    [ObservableProperty]
    private Color _gradientStartColor = Color.FromArgb(255, 255, 154, 0);

    [ObservableProperty]
    private Color _gradientEndColor = Color.FromArgb(255, 208, 0, 108);

    [ObservableProperty]
    private double _gradientAngle = 135;

    [ObservableProperty]
    private string _selectedPresetName = "Sunset";

    // Preview image
    [ObservableProperty]
    private BitmapImage? _previewImage;

    // Gradient preview
    [ObservableProperty]
    private BitmapImage? _gradientPreview;

    // State
    [ObservableProperty]
    private bool _hasCapture;

    [ObservableProperty]
    private bool _isCapturing;

    [ObservableProperty]
    private string _statusText = "Ready — click \"Capture Window\" to start";

    private SKBitmap? _capturedBitmap;
    private SKBitmap? _composedBitmap;

    public GradientDefinition[] GradientPresets => GradientDefinition.Presets;

    public MainPageViewModel()
    {
        _ = UpdateGradientPreviewAsync();
    }

    partial void OnBorderPaddingChanged(int value) => _ = RecomposeAsync();
    partial void OnCornerRadiusChanged(double value) => _ = RecomposeAsync();
    partial void OnGradientStartColorChanged(Color value) => _ = UpdateGradientPreviewAndRecomposeAsync();
    partial void OnGradientEndColorChanged(Color value) => _ = UpdateGradientPreviewAndRecomposeAsync();
    partial void OnGradientAngleChanged(double value) => _ = UpdateGradientPreviewAndRecomposeAsync();

    [RelayCommand]
    private void SelectPreset(GradientDefinition preset)
    {
        SelectedPresetName = preset.Name;
        GradientStartColor = preset.StartColor;
        GradientEndColor = preset.EndColor;
        GradientAngle = preset.AngleDegrees;
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

            _capturedBitmap?.Dispose();
            _capturedBitmap = result.Bitmap;
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
    private void SetAsWallpaper()
    {
        try
        {
            var gradient = new GradientDefinition("Custom", GradientStartColor, GradientEndColor, GradientAngle);
            WallpaperService.SetGradientAsWallpaper(gradient);
            StatusText = "Gradient set as desktop wallpaper!";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to set wallpaper: {ex.Message}";
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

    private async Task UpdateGradientPreviewAndRecomposeAsync()
    {
        await UpdateGradientPreviewAsync();
        await RecomposeAsync();
    }

    private async Task UpdateGradientPreviewAsync()
    {
        try
        {
            using var gradientBitmap = GradientService.RenderFromColors(
                GradientStartColor, GradientEndColor, GradientAngle, 300, 200);
            GradientPreview = await gradientBitmap.ToWinUIBitmapAsync();
        }
        catch
        {
            // Silently ignore rendering errors during initialization
        }
    }

    private async Task RecomposeAsync()
    {
        if (_capturedBitmap is null) return;

        try
        {
            _composedBitmap?.Dispose();
            _composedBitmap = ScreenshotComposer.Compose(
                _capturedBitmap,
                GradientStartColor,
                GradientEndColor,
                GradientAngle,
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
