using BookBase.Interfaces;
using BookBase.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZXing.Net.Maui;

namespace BookBase.ViewModels;

/// <summary>
/// Drives the ISBN scanner page.  Supports two modes:
/// <list type="bullet">
///   <item>
///     <term>Barcode scan</term>
///     <description>Live camera view decoded by ZXing.</description>
///   </item>
///   <item>
///     <term>OCR (Read Printed ISBN)</term>
///     <description>
///       Takes a photo via <see cref="MediaPicker"/> then runs on-device
///       text recognition to extract and validate ISBN candidates.
///     </description>
///   </item>
/// </list>
/// After a confirmed ISBN is found the ViewModel navigates back to the
/// previous page, passing the value via the <c>scannedIsbn</c> Shell query
/// parameter so <see cref="AddEditBookViewModel"/> can perform the lookup.
/// </summary>
public sealed partial class IsbnScannerViewModel : BaseViewModel
{
    private readonly IBarcodeRecognitionService _barcodeRecognitionService;
    private readonly IPrintedIsbnRecognitionService _printedIsbnRecognitionService;
    private readonly IBookRepository _bookRepository;

    public IsbnScannerViewModel(
        IBarcodeRecognitionService barcodeRecognitionService,
        IPrintedIsbnRecognitionService printedIsbnRecognitionService,
        IBookRepository bookRepository)
    {
        _barcodeRecognitionService = barcodeRecognitionService;
        _printedIsbnRecognitionService = printedIsbnRecognitionService;
        _bookRepository = bookRepository;
        Title = "Scan ISBN";
        StatusMessage = "Point your camera at the barcode.";
    }

    // ------------------------------------------------------------------ //
    //  Observable state                                                    //
    // ------------------------------------------------------------------ //

    /// <summary>True while the ZXing camera view should detect barcodes.</summary>
    [ObservableProperty]
    private bool isDetecting = true;

    /// <summary>True while in barcode-scan mode; false when in OCR mode.</summary>
    [ObservableProperty]
    private bool isBarcodeScanMode = true;

    /// <summary>User-facing status or instruction text.</summary>
    [ObservableProperty]
    private string statusMessage = string.Empty;

    /// <summary>ISBN candidates returned by OCR that await user confirmation.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOcrCandidates))]
    private IReadOnlyList<string> ocrCandidates = [];

    /// <summary>True when there is at least one OCR candidate to display.</summary>
    public bool HasOcrCandidates => OcrCandidates.Count > 0;

    /// <summary>The ISBN the user tapped in the OCR candidate list.</summary>
    [ObservableProperty]
    private string? selectedOcrIsbn;

    // ------------------------------------------------------------------ //
    //  Barcode detected (called from the page's code-behind)              //
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Processes a barcode value reported by ZXing.  Validates it as an
    /// ISBN and – if valid – navigates back with the confirmed value.
    /// </summary>
    public async Task HandleBarcodeDetectedAsync(
        string rawValue,
        BarcodeFormat format,
        CancellationToken cancellationToken = default)
    {
        if (!IsBarcodeScanMode)
        {
            return;
        }

        var normalized = _barcodeRecognitionService.TryRecognizeIsbn(rawValue, format);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            StatusMessage = "Point your camera at the barcode.";
            return;
        }

        // Check for duplicates in the local library before navigating.
        Book? existing;
        try
        {
            existing = await _bookRepository.GetByIsbnAsync(normalized, cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IsbnScannerViewModel] Duplicate check failed: {ex}");
            // Proceed with navigation; the lookup will handle any issues.
            existing = null;
        }
        if (existing is not null)
        {
            IsDetecting = false;
            bool viewBook = await Shell.Current.DisplayAlert(
                "Already in Library",
                "This book already exists in your library.",
                "View Book",
                "Scan Another");

            if (viewBook)
            {
                try
                {
                    // The scanner is always navigated to from a single level above
                    // (AddEditBookPage or DashboardPage → IsbnScannerPage), so
                    // "../BookDetailsPage" reliably pops the scanner and pushes details.
                    await Shell.Current.GoToAsync($"../BookDetailsPage?bookId={existing.Id}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[IsbnScannerViewModel] Navigation to BookDetailsPage failed: {ex}");
                    // Resume scanning so the user is not left in a broken state.
                    StatusMessage = "Point your camera at the barcode.";
                    IsDetecting = true;
                }
            }
            else
            {
                // Resume scanning.
                StatusMessage = "Point your camera at the barcode.";
                IsDetecting = true;
            }

            return;
        }

        // Valid ISBN, not a duplicate – stop detection and navigate back.
        IsDetecting = false;
        await ConfirmIsbnAsync(normalized, cancellationToken);
    }

    // ------------------------------------------------------------------ //
    //  Commands                                                           //
    // ------------------------------------------------------------------ //

    /// <summary>Switch to OCR "Read Printed ISBN" mode.</summary>
    [RelayCommand]
    private void SwitchToOcrMode()
    {
        IsBarcodeScanMode = false;
        IsDetecting = false;
        OcrCandidates = [];
        SelectedOcrIsbn = null;
        StatusMessage = "Take a photo of the printed ISBN number (such as the copyright page or the number below the barcode).";
    }

    /// <summary>Switch back to live barcode scanning mode.</summary>
    [RelayCommand]
    private void SwitchToBarcodeScanMode()
    {
        IsBarcodeScanMode = true;
        IsDetecting = true;
        OcrCandidates = [];
        SelectedOcrIsbn = null;
        StatusMessage = "Point your camera at the barcode.";
    }

    /// <summary>
    /// Captures a photo via <see cref="MediaPicker"/> and runs on-device
    /// OCR to extract ISBN candidates.
    /// </summary>
    [RelayCommand]
    private async Task TakePhotoForOcrAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        StatusMessage = "Opening camera…";
        OcrCandidates = [];
        SelectedOcrIsbn = null;

        try
        {
            var photo = await MediaPicker.CapturePhotoAsync(
                new MediaPickerOptions { Title = "Photograph the ISBN" });

            if (photo is null)
            {
                StatusMessage = "No photo taken. Tap the button to try again.";
                return;
            }

            StatusMessage = "Recognizing text…";

            await using var stream = await photo.OpenReadAsync();
            var candidates = await _printedIsbnRecognitionService
                .RecognizeIsbnCandidatesAsync(stream, cancellationToken);

            if (candidates.Count == 0)
            {
                StatusMessage = "No ISBN found. Retake the photo, or enter the ISBN manually on the Add/Edit page.";
                return;
            }

            OcrCandidates = candidates;
            if (candidates.Count == 1)
            {
                SelectedOcrIsbn = candidates[0];
                StatusMessage = "ISBN found. Looking up book details…";
                await ConfirmIsbnAsync(candidates[0], cancellationToken);
                return;
            }

            SelectedOcrIsbn = candidates[0];

            StatusMessage = $"{candidates.Count} ISBNs found. Select one and confirm.";
        }
        catch (FeatureNotSupportedException)
        {
            StatusMessage = "Camera not available on this device.";
        }
        catch (PermissionException)
        {
            StatusMessage = "Camera permission is required to take a photo.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Confirm the user-selected OCR ISBN and navigate back.</summary>
    [RelayCommand(CanExecute = nameof(CanConfirmOcrIsbn))]
    private async Task ConfirmOcrIsbnAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(SelectedOcrIsbn))
        {
            await ConfirmIsbnAsync(SelectedOcrIsbn, cancellationToken);
        }
    }

    private bool CanConfirmOcrIsbn() =>
        !string.IsNullOrWhiteSpace(SelectedOcrIsbn);

    /// <summary>Cancel scanning and navigate back without a result.</summary>
    [RelayCommand]
    private static async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    // ------------------------------------------------------------------ //
    //  Helpers                                                            //
    // ------------------------------------------------------------------ //

    partial void OnSelectedOcrIsbnChanged(string? value) =>
        ConfirmOcrIsbnCommand.NotifyCanExecuteChanged();

    private static async Task ConfirmIsbnAsync(string isbn, CancellationToken cancellationToken)
    {
        await Shell.Current.GoToAsync($"..?scannedIsbn={Uri.EscapeDataString(isbn)}");
    }

    public void ShowBarcodeTroubleHint()
    {
        if (!IsBarcodeScanMode || !IsDetecting)
        {
            return;
        }

        StatusMessage = "Having trouble? Try better lighting or use Read Printed ISBN.";
    }
}
