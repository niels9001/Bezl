using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;
using ScreenShotter.Models;
using ScreenShotter.ViewModels;
using Windows.Foundation;

namespace ScreenShotter;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; } = new();

    private bool _isDraggingHandle;
    private Point _dragStartPoint;
    private double _dragStartRadius;
    private const double MaxCornerRadius = 50;
    private const double HandleInset = 24;

    public MainPage()
    {
        InitializeComponent();
    }

    // --- Corner radius handle logic ---

    private void PreviewContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.HasCapture) return;
        SetHandlesOpacity(1.0);
    }

    private void PreviewContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (_isDraggingHandle) return;
        SetHandlesOpacity(0);
        RadiusLabel.Opacity = 0;
    }

    private void SetHandlesOpacity(double opacity)
    {
        HandleTL.Opacity = opacity;
        HandleTR.Opacity = opacity;
        HandleBL.Opacity = opacity;
        HandleBR.Opacity = opacity;
    }

    private void PreviewImage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        PositionHandles();
    }

    private void PositionHandles()
    {
        if (PreviewImage.ActualWidth <= 0 || PreviewImage.ActualHeight <= 0) return;

        // Calculate the rendered image rect within the container
        var imageRect = GetRenderedImageRect();
        double inset = HandleInset + ViewModel.CornerRadius * 0.3;
        double hs = 6; // half handle size

        Canvas.SetLeft(HandleTL, imageRect.X + inset - hs);
        Canvas.SetTop(HandleTL, imageRect.Y + inset - hs);

        Canvas.SetLeft(HandleTR, imageRect.X + imageRect.Width - inset - hs);
        Canvas.SetTop(HandleTR, imageRect.Y + inset - hs);

        Canvas.SetLeft(HandleBL, imageRect.X + inset - hs);
        Canvas.SetTop(HandleBL, imageRect.Y + imageRect.Height - inset - hs);

        Canvas.SetLeft(HandleBR, imageRect.X + imageRect.Width - inset - hs);
        Canvas.SetTop(HandleBR, imageRect.Y + imageRect.Height - inset - hs);
    }

    private Rect GetRenderedImageRect()
    {
        var containerW = PreviewContainer.ActualWidth;
        var containerH = PreviewContainer.ActualHeight;
        var imgSource = PreviewImage.Source as Microsoft.UI.Xaml.Media.Imaging.BitmapImage;

        if (imgSource is null || imgSource.PixelWidth == 0 || imgSource.PixelHeight == 0)
            return new Rect(0, 0, containerW, containerH);

        double imgAspect = (double)imgSource.PixelWidth / imgSource.PixelHeight;
        double containerAspect = containerW / containerH;

        double renderW, renderH;
        if (imgAspect > containerAspect)
        {
            renderW = containerW;
            renderH = containerW / imgAspect;
        }
        else
        {
            renderH = containerH;
            renderW = containerH * imgAspect;
        }

        double x = (containerW - renderW) / 2;
        double y = (containerH - renderH) / 2;
        return new Rect(x, y, renderW, renderH);
    }

    private void Handle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Ellipse handle) return;

        _isDraggingHandle = true;
        _dragStartPoint = e.GetCurrentPoint(PreviewContainer).Position;
        _dragStartRadius = ViewModel.CornerRadius;
        handle.CapturePointer(e.Pointer);
        e.Handled = true;

        RadiusLabel.Opacity = 1;
        RadiusLabelText.Text = $"Radius: {(int)ViewModel.CornerRadius}px";
    }

    private void Handle_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDraggingHandle) return;

        var currentPoint = e.GetCurrentPoint(PreviewContainer).Position;
        var imageRect = GetRenderedImageRect();

        // Calculate delta as diagonal movement toward/away from nearest corner
        double dx = currentPoint.X - _dragStartPoint.X;
        double dy = currentPoint.Y - _dragStartPoint.Y;

        // Determine which corner based on which handle
        double sign = 1;
        if (sender == HandleTL) sign = (dx + dy) / 2.0;     // toward center = positive
        else if (sender == HandleTR) sign = (-dx + dy) / 2.0;
        else if (sender == HandleBL) sign = (dx - dy) / 2.0;
        else if (sender == HandleBR) sign = (-dx - dy) / 2.0;

        // Scale the movement relative to the image size
        double scale = Math.Min(imageRect.Width, imageRect.Height) / 200.0;
        double newRadius = Math.Clamp(_dragStartRadius + sign / Math.Max(scale, 0.5), 0, MaxCornerRadius);

        ViewModel.CornerRadius = Math.Round(newRadius);
        RadiusLabelText.Text = $"Radius: {(int)ViewModel.CornerRadius}px";

        PositionHandles();
        e.Handled = true;
    }

    private void Handle_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Ellipse handle)
            handle.ReleasePointerCapture(e.Pointer);

        _isDraggingHandle = false;
        RadiusLabel.Opacity = 0;

        // Check if pointer is still inside preview
        var pos = e.GetCurrentPoint(PreviewContainer).Position;
        if (pos.X < 0 || pos.Y < 0 || pos.X > PreviewContainer.ActualWidth || pos.Y > PreviewContainer.ActualHeight)
            SetHandlesOpacity(0);

        e.Handled = true;
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
}
