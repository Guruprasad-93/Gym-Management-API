using System.Text.RegularExpressions;

namespace Gym.Application.Validation;

public static partial class PhoneNumberRules
{
    /// <summary>E.164: + followed by 7–15 digits (country code + national number).</summary>
    private static readonly Regex E164Pattern = E164Regex();

    public const string InvalidMessage =
        "Enter a valid international phone number in E.164 format (e.g. +91 9876543210).";

    public static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        var trimmed = phone.Trim();
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
            return string.Empty;

        return trimmed.StartsWith('+') ? $"+{digits}" : $"+{digits}";
    }

    public static bool IsValid(string? phone)
    {
        var normalized = Normalize(phone);
        return !string.IsNullOrEmpty(normalized) && E164Pattern.IsMatch(normalized);
    }

    public static bool IsValidOptional(string? phone) =>
        string.IsNullOrWhiteSpace(phone) || IsValid(phone);

    [GeneratedRegex(@"^\+[1-9]\d{6,14}$", RegexOptions.Compiled)]
    private static partial Regex E164Regex();
}
