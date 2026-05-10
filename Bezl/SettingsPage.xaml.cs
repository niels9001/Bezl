using Bezl.Services;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

namespace Bezl;

public sealed partial class SettingsPage : Page, INotifyPropertyChanged
{
    private int _defaultPadding;
    private double _defaultCornerRadius;

    public double DefaultPadding
    {
        get => _defaultPadding;
        set
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DefaultPadding)));
                return;
            }

            var coerced = (int)Math.Clamp(value, 0, 200);
            if (_defaultPadding != coerced)
            {
                _defaultPadding = coerced;
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
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DefaultCornerRadius)));
                return;
            }

            var coerced = Math.Clamp(value, 0, 100);
            if (_defaultCornerRadius != coerced)
            {
                _defaultCornerRadius = coerced;
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
            DefaultPadding = (int)DefaultPadding,
            DefaultCornerRadius = DefaultCornerRadius
        });
    }

    private async void GitHubCard_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await Windows.System.Launcher.LaunchUriAsync(new System.Uri("https://github.com/niels9001/Bezl"));
    }
}
