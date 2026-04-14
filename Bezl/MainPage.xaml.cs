using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Bezl.Models;
using Bezl.ViewModels;

namespace Bezl;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; } = new();

    public MainPage()
    {
        InitializeComponent();
    }

    // --- Gradient card handlers ---

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

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SettingsPage));
    }
}
