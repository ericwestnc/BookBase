using BookBase.Interfaces;
using BookBase.Models;
using BookBase.Utilities;
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
    private readonly IIsbnTextRecognitionService _textRecognitionService;
    private readonly IBookRepository _bookRepository;

    public IsbnScannerViewModel(
        IIsbnTextRecognitionService textRecognitionService,
        IBookRepository bookRepository)
    {
        _textRecognitionService = textRecognitionService;
        _bookRepository = bookRepository;
        Title = "Scan ISBN";
        StatusMessage = "Point the camera at a book barcode.";
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

        var normalized = IsbnNormalizer.Normalize(rawValue);

        if (!IsLikelyBookBarcode(normalized, format))
        {
            // Resume detection – not a book barcode.
            return;
        }

        if (!IsbnValidator.IsValid(normalized))
        {
            StatusMessage = "Barcode detected but ISBN checksum failed. Keep scanning…";
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
                    StatusMessage = "Point the camera at a book barcode.";
                    IsDetecting = true;
                }
            }
            else
            {
                // Resume scanning.
                StatusMessage = "Point the camera at a book barcode.";
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
        StatusMessage = "Take a photo of the printed ISBN text.";
    }

    /// <summary>Switch back to live barcode scanning mode.</summary>
    [RelayCommand]
    private void SwitchToBarcodeScanMode()
    {
        IsBarcodeScanMode = true;
        IsDetecting = true;
        OcrCandidates = [];
        SelectedOcrIsbn = null;
        StatusMessage = "Point the camera at a book barcode.";
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
            var candidates = await _textRecognitionService
                .RecognizeIsbnCandidatesAsync(stream, cancellationToken);

            if (candidates.Count == 0)
            {
                StatusMessage = "No ISBN found in photo. Try again or type the ISBN manually.";
                return;
            }

            OcrCandidates = candidates;
            SelectedOcrIsbn = candidates[0];

            StatusMessage = candidates.Count == 1
                ? "ISBN found. Confirm to continue."
                : $"{candidates.Count} ISBNs found. Select one and confirm.";
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

    private static bool IsLikelyBookBarcode(string normalized, BarcodeFormat format)
    {
        return format switch
        {
            BarcodeFormat.Ean13 =>
                (normalized.StartsWith("978", StringComparison.Ordinal) ||
                 normalized.StartsWith("979", StringComparison.Ordinal)),

            BarcodeFormat.UpcA =>
                // UPC-A (12 digits) is uncommon for ISBNs but some older editions
                // have a UPC barcode equivalent.  Accept if digits only.
                normalized.Length == 12 && normalized.All(char.IsAsciiDigit),

            BarcodeFormat.Ean8 =>
                // EAN-8 is never an ISBN but accept and let checksum decide.
                normalized.Length == 8 && normalized.All(char.IsAsciiDigit),

            BarcodeFormat.UpcE =>
                // UPC-E is returned by ZXing in its compressed (8-digit) form.
                // Accept both the 8-digit compressed form and the 12-digit
                // expanded UPC-A equivalent so either representation is handled.
                (normalized.Length == 8 || normalized.Length == 12) && normalized.All(char.IsAsciiDigit),

            _ => false
        };
    }
}
