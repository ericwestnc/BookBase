namespace BookBase.Utilities;

/// <summary>
/// Validates ISBN-10 and ISBN-13 values using their respective checksum algorithms.
/// </summary>
internal static class IsbnValidator
{
    /// <summary>
    /// Determines whether the given string is a valid ISBN-10 or ISBN-13.
    /// Digits-only strings (no hyphens or spaces) are expected; call
    /// <see cref="IsbnNormalizer.Normalize"/> first if needed.
    /// </summary>
    public static bool IsValid(string? isbn)
    {
        if (string.IsNullOrEmpty(isbn))
        {
            return false;
        }

        return isbn.Length switch
        {
            10 => IsValidIsbn10(isbn),
            13 => IsValidIsbn13(isbn),
            _ => false
        };
    }

    /// <summary>
    /// Returns true when the string is a book ISBN (EAN-13 starting with 978 or 979
    /// that also passes the ISBN-13 checksum).
    /// </summary>
    public static bool IsBookIsbn13(string? isbn)
    {
        if (string.IsNullOrEmpty(isbn) || isbn.Length != 13)
        {
            return false;
        }

        return (isbn.StartsWith("978", StringComparison.Ordinal) ||
                isbn.StartsWith("979", StringComparison.Ordinal))
               && IsValidIsbn13(isbn);
    }

    // ------------------------------------------------------------------
    // Checksum implementations
    // ------------------------------------------------------------------

    private static bool IsValidIsbn10(string isbn)
    {
        // Characters 0-8 must be digits; character 9 may be a digit or 'X'.
        for (int i = 0; i < 9; i++)
        {
            if (!char.IsAsciiDigit(isbn[i]))
            {
                return false;
            }
        }

        char lastChar = char.ToUpperInvariant(isbn[9]);
        if (!char.IsAsciiDigit(lastChar) && lastChar != 'X')
        {
            return false;
        }

        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += (isbn[i] - '0') * (10 - i);
        }

        int checkValue = lastChar == 'X' ? 10 : (lastChar - '0');
        sum += checkValue;

        return sum % 11 == 0;
    }

    private static bool IsValidIsbn13(string isbn)
    {
        for (int i = 0; i < 13; i++)
        {
            if (!char.IsAsciiDigit(isbn[i]))
            {
                return false;
            }
        }

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = isbn[i] - '0';
            sum += i % 2 == 0 ? digit : digit * 3;
        }

        int expectedCheck = (10 - (sum % 10)) % 10;
        int actualCheck = isbn[12] - '0';

        return expectedCheck == actualCheck;
    }
}
