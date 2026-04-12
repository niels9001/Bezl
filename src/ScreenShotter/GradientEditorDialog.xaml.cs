using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScreenShotter.Helpers;
using ScreenShotter.Models;
using ScreenShotter.Services;
using Windows.UI;

namespace ScreenShotter;

public sealed partial class GradientEditorDialog : ContentDialog
{
    public GradientDefinition? Result { get; private set; }

    public GradientEditorDialog()
    {
        InitializeComponent();
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
        {
            name = "Untitled";
        }

        Result = new GradientDefinition(
            name,
            StartColorPicker.Color,
            EndColorPicker.Color,
            AngleSlider.Value);
    }

    private void OnColorOrAngleChanged(object sender, object args)
    {
        AngleLabel.Text = $"Angle: {(int)AngleSlider.Value}°";
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
