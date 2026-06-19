using System.Text.RegularExpressions;

namespace Gym.Application.Validation;

public static partial class PhoneNumberRules
{
    private static readonly Regex E164Pattern = E164Regex();

    /// <summary>National number digit lengths keyed by country calling code (without +).</summary>
    private static readonly Dictionary<string, (int Min, int Max)> NationalLengths = new(StringComparer.Ordinal)
    {
        ["91"] = (10, 10),   // India
        ["1"] = (10, 10),    // US/Canada
        ["44"] = (10, 10),   // UK
        ["61"] = (9, 9),     // Australia
        ["971"] = (9, 9),    // UAE
        ["65"] = (8, 8),     // Singapore
        ["49"] = (10, 11),   // Germany
        ["33"] = (9, 9),     // France
        ["81"] = (10, 10),   // Japan
        ["86"] = (11, 11),   // China
    };

    public const string InvalidMessage =
        "Enter a valid international phone number for the selected country (E.164 format).";

    public static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        var trimmed = phone.Trim();
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
            return string.Empty;

        return $"+{digits}";
    }

    public static bool IsValid(string? phone)
    {
        var normalized = Normalize(phone);
        if (string.IsNullOrEmpty(normalized) || !normalized.StartsWith('+'))
            return false;

        var digitsOnly = normalized[1..];
        if (!digitsOnly.All(char.IsDigit))
            return false;

        foreach (var entry in NationalLengths.OrderByDescending(k => k.Key.Length))
        {
            if (!digitsOnly.StartsWith(entry.Key, StringComparison.Ordinal))
                continue;

            var nationalLength = digitsOnly.Length - entry.Key.Length;
            return nationalLength >= entry.Value.Min && nationalLength <= entry.Value.Max;
        }

        return E164Pattern.IsMatch(normalized);
    }

    public static bool IsValidOptional(string? phone) =>
        string.IsNullOrWhiteSpace(phone) || IsValid(phone);

    [GeneratedRegex(@"^\+[1-9]\d{6,14}$", RegexOptions.Compiled)]
    private static partial Regex E164Regex();
}
