namespace Gym.Infrastructure.Persistence.Models;

internal sealed class MenuRow
{
    public int MenuId { get; set; }
    public string MenuCode { get; set; } = string.Empty;
    public string MenuName { get; set; } = string.Empty;
    public int? ParentMenuId { get; set; }
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

internal sealed class GymMenuRow
{
    public int? GymMenuId { get; set; }
    public Guid GymId { get; set; }
    public int MenuId { get; set; }
    public string MenuCode { get; set; } = string.Empty;
    public string MenuName { get; set; } = string.Empty;
    public int? ParentMenuId { get; set; }
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? EnabledOn { get; set; }
    public Guid? EnabledBy { get; set; }
}
