using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using ScreenShotter.Helpers;
using ScreenShotter.Services;
using Windows.UI;

namespace ScreenShotter.Models;

public partial class GradientItem : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private Color _startColor;

    [ObservableProperty]
    private Color _endColor;

    [ObservableProperty]
    private double _angleDegrees;

    [ObservableProperty]
    private BitmapImage? _preview;

    [ObservableProperty]
    private bool _isSelected;

    public GradientItem(GradientDefinition definition)
    {
        _name = definition.Name;
        _startColor = definition.StartColor;
        _endColor = definition.EndColor;
        _angleDegrees = definition.AngleDegrees;
    }

    public GradientDefinition ToDefinition() =>
        new(Name, StartColor, EndColor, AngleDegrees);

    public void UpdateFrom(GradientDefinition definition)
    {
        Name = definition.Name;
        StartColor = definition.StartColor;
        EndColor = definition.EndColor;
        AngleDegrees = definition.AngleDegrees;
        _ = RenderPreviewAsync();
    }

    public async Task RenderPreviewAsync()
    {
        try
        {
            using var bitmap = GradientService.RenderFromColors(StartColor, EndColor, AngleDegrees, 200, 100);
            Preview = await bitmap.ToWinUIBitmapAsync();
        }
        catch { }
    }
}
