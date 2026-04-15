using Bezl.Services;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Bezl;

/// <summary>
/// The application window. This hosts a Frame that displays pages. Add your
/// UI and logic to MainPage.xaml / MainPage.xaml.cs instead of here so you
/// can use Page features such as navigation events and the Loaded lifecycle.
/// </summary>
public sealed partial class MainWindow : Window
{
    private HotkeyService? _hotkeyService;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");

        // Navigate the root frame to the main page on startup.
        RootFrame.Navigate(typeof(MainPage));

        // Register global hotkey (Ctrl+Shift+S) once the window has a handle
        RegisterGlobalHotkey();
    }

    private void RegisterGlobalHotkey()
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        _hotkeyService = new HotkeyService(hwnd);
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        _hotkeyService.Register();
    }

    private void OnHotkeyPressed()
    {
        App.DispatcherQueue.TryEnqueue(async () =>
        {
            if (RootFrame.Content is MainPage page)
            {
                await page.ViewModel.HandleHotkeyCaptureAsync();

                // Bring Bezl to the front after capture
                var presenter = AppWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
                presenter?.Restore();
                this.Activate();
            }
        });
    }

    private void AppTitleBar_BackRequested(Microsoft.UI.Xaml.Controls.TitleBar sender, object args)
    {
        if (RootFrame.CanGoBack)
        {
            RootFrame.GoBack();
        }
    }
}
