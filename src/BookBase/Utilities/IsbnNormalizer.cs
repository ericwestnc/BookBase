namespace BookBase.Utilities;

internal static class IsbnNormalizer
{
    public static string Normalize(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return string.Empty;
        }

        return string.Concat(isbn.Where(c => c != '-' && !char.IsWhiteSpace(c)));
    }
}
