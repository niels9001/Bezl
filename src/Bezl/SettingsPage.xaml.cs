using Bezl.Services;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

namespace Bezl;

public sealed partial class SettingsPage : Page, INotifyPropertyChanged
{
    private int _defaultPadding;
    private double _defaultCornerRadius;

    public int DefaultPadding
    {
        get => _defaultPadding;
        set
        {
            if (_defaultPadding != value)
            {
                _defaultPadding = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DefaultPadding)));
                SaveSettings();
            }
        }
    }

    public double DefaultCornerRadius
    {
        get => _defaultCornerRadius;
        set
        {
            if (_defaultCornerRadius != value)
            {
                _defaultCornerRadius = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DefaultCornerRadius)));
                SaveSettings();
            }
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public SettingsPage()
    {
        var settings = SettingsService.Load();
        _defaultPadding = settings.DefaultPadding;
        _defaultCornerRadius = settings.DefaultCornerRadius;
        InitializeComponent();
    }

    private void SaveSettings()
    {
        SettingsService.Save(new AppSettings
        {
            DefaultPadding = DefaultPadding,
            DefaultCornerRadius = DefaultCornerRadius
        });
    }

    private async void GitHubCard_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await Windows.System.Launcher.LaunchUriAsync(new System.Uri("https://github.com/niels9001/Bezl"));
    }
}
