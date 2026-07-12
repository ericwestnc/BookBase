using BookBase.Helpers;
using BookBase.ViewModels;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace BookBase.Views;

/// <summary>
/// Camera-based ISBN scanning page.  Hosts a ZXing barcode reader for the
/// primary scanning mode and delegates barcode detection events to
/// <see cref="IsbnScannerViewModel"/>.
/// </summary>
public partial class IsbnScannerPage : ContentPage
{
    private readonly TimeSpan _barcodeHintDelay = TimeSpan.FromSeconds(7);
    private readonly TimeSpan _barcodeRetryDelay = TimeSpan.FromMilliseconds(750);

    public IsbnScannerPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<IsbnScannerViewModel>();

        // Configure ZXing to detect the formats most likely to contain ISBNs.
        BarcodeReaderView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.Ean13
                      | BarcodeFormat.UpcA
                      | BarcodeFormat.Ean8
                      | BarcodeFormat.UpcE,
            Multiple = false,
            AutoRotate = true,
            TryHarder = true
        };
    }

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = RequestCameraPermissionAndStartAsync();
        ScheduleBarcodeTroubleHint();
    }

    private async Task RequestCameraPermissionAndStartAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status == PermissionStatus.Granted)
            {
                if (BindingContext is IsbnScannerViewModel vm && vm.IsBarcodeScanMode)
                {
                    BarcodeReaderView.IsDetecting = true;
                }
            }
            else
            {
                // Camera permission denied – show a friendly explanation.
                bool openSettings = await Shell.Current.DisplayAlert(
                    "Camera Required",
                    "BookBase requires camera access to scan book barcodes and ISBNs. Please grant camera permission in Settings.",
                    "Open Settings",
                    "Cancel");

                if (openSettings)
                {
                    AppInfo.ShowSettingsUI();
                }
                else
                {
                    await Shell.Current.GoToAsync("..");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IsbnScannerPage] Camera permission request failed: {ex}");
            await Shell.Current.GoToAsync("..");
        }
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        BarcodeReaderView.IsDetecting = false;
    }

    /// <summary>
    /// Raised by ZXing on a background thread whenever a barcode is
    /// detected.  Dispatches the result to the ViewModel on the UI thread.
    /// </summary>
    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var first = e.Results.FirstOrDefault();
        if (first is null)
        {
            return;
        }

        // Pause detection to avoid multiple firings for the same barcode.
        BarcodeReaderView.IsDetecting = false;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (BindingContext is IsbnScannerViewModel vm)
            {
                TryPerformHapticFeedback();
                await vm.HandleBarcodeDetectedAsync(first.Value, first.Format);

                // Resume detection only when the ViewModel explicitly keeps
                // detecting enabled (invalid/ignored result paths).
                var shouldResumeDetection = vm.IsDetecting && vm.IsBarcodeScanMode;
                if (shouldResumeDetection)
                {
                    await Task.Delay(_barcodeRetryDelay);
                    BarcodeReaderView.IsDetecting = vm.IsBarcodeScanMode;
                    vm.IsDetecting = vm.IsBarcodeScanMode;
                    ScheduleBarcodeTroubleHint();
                }
            }
        });
    }

    private void OnToggleFlashlightClicked(object? sender, EventArgs e)
    {
        BarcodeReaderView.IsTorchOn = !BarcodeReaderView.IsTorchOn;
        FlashlightButton.Text = BarcodeReaderView.IsTorchOn ? "Flashlight On" : "Flashlight";
    }

    private void ScheduleBarcodeTroubleHint()
    {
        Dispatcher.StartTimer(_barcodeHintDelay, () =>
        {
            if (BindingContext is IsbnScannerViewModel vm && vm.IsBarcodeScanMode && vm.IsDetecting)
            {
                vm.ShowBarcodeTroubleHint();
            }

            return false;
        });
    }

    private static void TryPerformHapticFeedback()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (FeatureNotSupportedException)
        {
            System.Diagnostics.Debug.WriteLine("[IsbnScannerPage] Haptic feedback is not supported on this device.");
        }
    }
}
