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
                      | BarcodeFormat.Code128,
            Multiple = false,
            AutoRotate = true,
            TryHarder = true
        };
    }

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is IsbnScannerViewModel vm && vm.IsBarcodeScanMode)
        {
            BarcodeReaderView.IsDetecting = true;
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
                await vm.HandleBarcodeDetectedAsync(first.Value, first.Format);

                // Re-enable detection only when the ViewModel says to (i.e.,
                // the value was not a valid ISBN).
                if (vm.IsDetecting == false && vm.IsBarcodeScanMode)
                {
                    await Task.Delay(1500); // brief pause before retrying
                    BarcodeReaderView.IsDetecting = vm.IsBarcodeScanMode;
                    vm.IsDetecting = vm.IsBarcodeScanMode;
                }
            }
        });
    }
}
