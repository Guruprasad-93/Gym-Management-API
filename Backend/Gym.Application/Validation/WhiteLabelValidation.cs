using System.Text.RegularExpressions;
using Gym.Application.DTOs.WhiteLabel;

namespace Gym.Application.Validation;

public static partial class WhiteLabelValidation
{
    [GeneratedRegex(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")]
    private static partial Regex HexColorRegex();

    public static void ValidateSettings(UpsertWhiteLabelSettingsDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.BrandName))
            throw new ArgumentException("BrandName is required.");
        if (!string.IsNullOrWhiteSpace(dto.PrimaryColor) && !HexColorRegex().IsMatch(dto.PrimaryColor))
            throw new ArgumentException("PrimaryColor must be a valid hex color.");
        if (!string.IsNullOrWhiteSpace(dto.SecondaryColor) && !HexColorRegex().IsMatch(dto.SecondaryColor))
            throw new ArgumentException("SecondaryColor must be a valid hex color.");
    }
}
