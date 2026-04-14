using Bezl.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(DefaultPadding)));
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
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(DefaultCornerRadius)));
                SaveSettings();
            }
        }
    }

    public new event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

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

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
            Frame.GoBack();
    }
}
