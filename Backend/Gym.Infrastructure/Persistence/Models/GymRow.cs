namespace Gym.Infrastructure.Persistence.Models;

internal sealed class GymRow
{
    public Guid GymId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public long? BannerFileId { get; set; }
    public string? ReceiptHeaderText { get; set; }
    public string? InvoiceFooterText { get; set; }
}
