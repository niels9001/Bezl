using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Vignette.Helpers;
using Vignette.Models;
using Vignette.Services;
using Windows.UI;

namespace Vignette;

public sealed partial class GradientEditorDialog : ContentDialog
{
    public GradientDefinition? Result { get; private set; }

    public GradientEditorDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdateSwatchesAndPreview();
    }

    public GradientEditorDialog(GradientDefinition existing) : this()
    {
        Title = "Edit Gradient";
        NameBox.Text = existing.Name;
        StartColorPicker.Color = existing.StartColor;
        EndColorPicker.Color = existing.EndColor;
        AngleSlider.Value = existing.AngleDegrees;
    }

    public static GradientEditorDialog CreateNew()
    {
        var dialog = new GradientEditorDialog
        {
            Title = "New Gradient"
        };
        dialog.NameBox.Text = "";
        dialog.StartColorPicker.Color = Color.FromArgb(255, 100, 100, 255);
        dialog.EndColorPicker.Color = Color.FromArgb(255, 255, 100, 100);
        dialog.AngleSlider.Value = 135;
        return dialog;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
            name = "Untitled";

        Result = new GradientDefinition(
            name,
            StartColorPicker.Color,
            EndColorPicker.Color,
            AngleSlider.Value);
    }

    private void OnColorOrAngleChanged(object sender, object args)
    {
        AngleLabel.Text = $"Angle: {(int)AngleSlider.Value}°";
        UpdateSwatchesAndPreview();
    }

    private void UpdateSwatchesAndPreview()
    {
        var startColor = StartColorPicker.Color;
        var endColor = EndColorPicker.Color;

        StartColorSwatch.Background = new SolidColorBrush(
            Microsoft.UI.ColorHelper.FromArgb(startColor.A, startColor.R, startColor.G, startColor.B));
        EndColorSwatch.Background = new SolidColorBrush(
            Microsoft.UI.ColorHelper.FromArgb(endColor.A, endColor.R, endColor.G, endColor.B));

        UpdatePreview();
    }

    private async void UpdatePreview()
    {
        try
        {
            using var bitmap = GradientService.RenderFromColors(
                StartColorPicker.Color, EndColorPicker.Color, AngleSlider.Value, 400, 120);
            GradientPreviewImage.Source = await bitmap.ToWinUIBitmapAsync();
        }
        catch { }
    }
}
