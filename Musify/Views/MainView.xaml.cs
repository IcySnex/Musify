using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Musify.Helpers;
using Windows.Foundation;
using WinRT.Interop;
using Microsoft.Extensions.Options;
using Musify.Models;
using Musify.Services;
using Microsoft.UI.Xaml.Input;
using System;

namespace Musify.Views;

public sealed partial class MainView : Window
{
    readonly ILogger<MainView> logger;
    readonly Config config;
    readonly JsonConverter jsonConverter;

    readonly IntPtr hWnd;
    readonly WindowId id;
    readonly AppWindow window;
    readonly InputNonClientPointerSource nonClientInputSrc;


    public MainView(
        ILogger<MainView> logger,
        IOptions<Config> config,
        JsonConverter jsonConverter)
    {
        this.logger = logger;
        this.config = config.Value;
        this.jsonConverter = jsonConverter;

        hWnd = WindowNative.GetWindowHandle(this);
        id = Win32Interop.GetWindowIdFromWindow(hWnd);
        window = AppWindow.GetFromWindowId(id);
        nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(id);

        SystemBackdrop = MicaController.IsSupported() ? new MicaBackdrop() : new DesktopAcrylicBackdrop();
        ExtendsContentIntoTitleBar = true;
        nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, [new(4, 8, 32, 32)]);

        InitializeComponent();

        logger.LogInformation("MainView has been created and intialized");
    }


    void OnClosed(object _, WindowEventArgs _1)
    {
        string jsonConfig = jsonConverter.ToString(config);
        File.WriteAllText("config.json", jsonConfig);

        logger.LogInformation("[MainView-Closed] Closed main window.");
    }

    void OnRootLayoutPreviewKeyDown(object _, KeyRoutedEventArgs e)
    {
        if (LoadingPopupRootLayout.IsHitTestVisible)
            e.Handled = true;
    }

    void OnLoadingPopupCancelButtonClicked(object _, RoutedEventArgs _1)
    {
        onCancelButtonClicked?.Invoke();
        HideLoadingPopup();
    }


    public void SetIcon(
        string path)
    {
        window.SetIcon(path);

        logger?.LogInformation("[MainView-SetIcon] Set app icon to path: {path}", path);
    }


    public void SetSize(
        int width,
        int height)
    {
        window.Resize(new(width + 16, height + 39));

        logger.LogInformation("[MainView-SetSize] Set window size: {width}x{height}", width, height);
    }

    public void SetSize(
        int width,
        int height,
        Window window)
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(window);
        WindowId id = Win32Interop.GetWindowIdFromWindow(hWnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(id);

        appWindow.Resize(new(width + 16, height + 39));

        logger.LogInformation("[MainView-SetSize] Set window size for external window: {width}x{height}", width, height);
    }


    public void SetMinSize(
        int width,
        int height)
    {
        IntPtr dpi = Win32.GetDpiForWindow(hWnd);

        Win32.MinWidth = (int)(width * (float)dpi / 96);
        Win32.MinHeight = (int)(height * (float)dpi / 96);

        Win32.NewWndProc = new Win32.WinProc(Win32.NewWindowProc);
        Win32.OldWndProc = IntPtr.Size == 8 ? Win32.SetWindowLongPtr(hWnd, -16 | 0x4 | 0x8, Win32.NewWndProc) : Win32.SetWindowLong(hWnd, -16 | 0x4 | 0x8, Win32.NewWndProc);

        logger.LogInformation("[MainView-SetMinSize] Set minimum window size: {width}x{height}", width, height);
    }


    public IAsyncOperation<ContentDialogResult> AlertAsync(
        ContentDialog dialog)
    {
        foreach (Popup popup in VisualTreeHelper.GetOpenPopupsForXamlRoot(Content.XamlRoot))
            if (popup.Child is ContentDialog openDialog)
                openDialog.Hide();

        dialog.XamlRoot = Content.XamlRoot;
        dialog.PrimaryButtonStyle = (Style)App.Current.Resources["AccentButtonStyle"];
        dialog.Style = (Style)App.Current.Resources["DefaultContentDialogStyle"];

        logger.LogInformation("[MainView-AlertAsync] ContentDialog was requested");
        return dialog.ShowAsync();
    }

    public IAsyncOperation<ContentDialogResult> AlertAsync(
        object content,
        string? title = null,
        string? closeButton = "Ok",
        string? primaryButton = null)
    {
        ContentDialog dialog = new()
        {
            Content = content,
            Title = title,
            CloseButtonText = closeButton,
            PrimaryButtonText = primaryButton
        };
        return AlertAsync(dialog);
    }


    public LoggerView? LoggerView = null;

    public void CreateLoggerView()
    {
        if (LoggerView is not null)
        {
            LoggerView.Activate();
            return;
        }

        LoggerView = new();

        void handler(object? s, string e) =>
            LoggerView.ContentBlock.Text += e;

        App.Sink.OnNewLog += handler;
        LoggerView.Closed += (s, e) =>
        {
            App.Sink.OnNewLog -= handler;
            LoggerView = null;
        };

        SetSize(700, 400, LoggerView);
        LoggerView.Activate();

        logger.LogInformation("[MainView-CreateLoggerView] Created new LoggerView and hooked handler");
    }


    Action? onCancelButtonClicked;

    public IProgress<string> ShowLoadingPopup(
        Action? onCancelButtonClicked = null)
    {
        LoadingPopupRootLayout.Opacity = 1;
        LoadingPopupRootLayout.IsHitTestVisible = true;
        this.onCancelButtonClicked = onCancelButtonClicked;

        return new Progress<string>(message => LoadingPopupTitleBlock.Text = message);
    }

    public void HideLoadingPopup()
    {
        LoadingPopupRootLayout.Opacity = 0;
        LoadingPopupRootLayout.IsHitTestVisible = false;
        onCancelButtonClicked = null;
    }


    public void Initialize(
        object target)
    {
        InitializeWithWindow.Initialize(target, hWnd);

        logger.LogInformation("[MainView-Initialize] Target was initialized with window");
    }
}