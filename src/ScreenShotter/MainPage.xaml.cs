using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScreenShotter.Models;
using ScreenShotter.ViewModels;

namespace ScreenShotter;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; } = new();

    public MainPage()
    {
        InitializeComponent();
    }

    private void PresetButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is GradientDefinition preset)
        {
            ViewModel.SelectPresetCommand.Execute(preset);
        }
    }
}
