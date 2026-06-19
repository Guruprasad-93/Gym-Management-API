using System.Text.RegularExpressions;

namespace Gym.Application.Validation;

public static class LoginIdentifierRules
{
    public const int MaxLength = 20;

    private static readonly Regex Pattern = new(@"^[a-zA-Z0-9._-]+$", RegexOptions.Compiled);

    public static string Normalize(string loginIdentifier)
    {
        if (string.IsNullOrWhiteSpace(loginIdentifier))
            throw new ArgumentException("Login identifier is required.", nameof(loginIdentifier));

        return loginIdentifier.Trim().ToLowerInvariant();
    }

    public static bool TryNormalize(string? loginIdentifier, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(loginIdentifier))
            return false;

        normalized = loginIdentifier.Trim().ToLowerInvariant();
        return true;
    }

    public static void Validate(string loginIdentifier)
    {
        var normalized = Normalize(loginIdentifier);

        if (normalized.Length > MaxLength)
            throw new ArgumentException($"Login identifier cannot exceed {MaxLength} characters.", nameof(loginIdentifier));

        if (!Pattern.IsMatch(normalized))
            throw new ArgumentException("Login identifier may only contain letters, numbers, dots, underscores, and hyphens.", nameof(loginIdentifier));
    }

    public static string FromEmailLocalPart(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        var normalized = email.Trim().ToLowerInvariant();
        var at = normalized.IndexOf('@');
        var local = at > 0 ? normalized[..at] : normalized;
        local = local.Replace(" ", string.Empty);
        return local.Length <= MaxLength ? local : local[..MaxLength];
    }
}
