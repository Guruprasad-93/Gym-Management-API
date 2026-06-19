namespace Gym.Application.DTOs.Menus;

public class MenuDto
{
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

public class TenantMenuDto : MenuDto
{
    public Guid GymId { get; set; }
    public int? GymMenuId { get; set; }
}

public class GymMenuSummaryDto
{
    public Guid GymId { get; set; }
    public string GymName { get; set; } = string.Empty;
    public int TotalMenus { get; set; }
    public int EnabledMenus { get; set; }
}

public class SetGymMenuEnabledDto
{
    public bool IsEnabled { get; set; }
}

public class BulkSetGymMenusDto
{
    public IReadOnlyList<int> MenuIds { get; set; } = Array.Empty<int>();
    public bool IsEnabled { get; set; }
}

public class MyMenusResponseDto
{
    public IReadOnlyList<MenuDto> Menus { get; set; } = Array.Empty<MenuDto>();
    public IReadOnlyList<string> EnabledMenuCodes { get; set; } = Array.Empty<string>();
}
