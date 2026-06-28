namespace Gym.Application.Validation;

public static class LoginIdentifierRules
{
    public const int MaxLength = 100;

    public static string Normalize(string loginIdentifier)
    {
        if (string.IsNullOrWhiteSpace(loginIdentifier))
            throw new ArgumentException("Login identifier is required.", nameof(loginIdentifier));

        return loginIdentifier.Trim();
    }

    public static bool TryNormalize(string? loginIdentifier, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(loginIdentifier))
            return false;

        normalized = loginIdentifier.Trim();
        return true;
    }

    public static void Validate(string loginIdentifier)
    {
        var normalized = Normalize(loginIdentifier);

        if (normalized.Length > MaxLength)
            throw new ArgumentException($"Login identifier cannot exceed {MaxLength} characters.", nameof(loginIdentifier));
    }
}
