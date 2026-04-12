using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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

    private void GradientCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is GradientItem item)
        {
            ViewModel.SelectGradientCommand.Execute(item);
            ViewModel.SetAsWallpaperCommand.Execute(null);
        }
    }

    private void GradientCard_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        // Context flyout is handled automatically by Button.ContextFlyout
    }

    private async void AddGradient_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.AddGradientCommand.ExecuteAsync(null);
    }

    private async void EditGradient_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is GradientItem gradient)
        {
            await ViewModel.EditGradientCommand.ExecuteAsync(gradient);
        }
    }

    private void DeleteGradient_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is GradientItem gradient)
        {
            ViewModel.DeleteGradientCommand.Execute(gradient);
        }
    }
}
