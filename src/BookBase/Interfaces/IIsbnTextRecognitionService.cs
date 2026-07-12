namespace BookBase.Interfaces;

/// <summary>
/// Performs on-device text recognition on an image and returns any ISBN
/// candidates found within the recognized text.
/// </summary>
public interface IIsbnTextRecognitionService
{
    /// <summary>
    /// Analyses the image supplied via <paramref name="imageStream"/> and
    /// returns the set of ISBN-10 / ISBN-13 strings (digits only, no
    /// hyphens) that pass the checksum check.
    /// </summary>
    /// <param name="imageStream">
    /// A readable stream containing a JPEG or PNG image of the text to
    /// recognise.  The caller retains ownership of the stream.
    /// </param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>
    /// A (possibly empty) read-only list of validated ISBN strings, or an
    /// empty list when the platform does not support on-device OCR.
    /// </returns>
    Task<IReadOnlyList<string>> RecognizeIsbnCandidatesAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default);
}
