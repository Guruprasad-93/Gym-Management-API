namespace Gym.Domain.Entities;

public class Privilege : BaseEntity
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 500;
    public const int MaxCategoryLength = 100;

    public string PrivilegeName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Category { get; private set; } = "General";
    public DateTime CreatedDate { get; private set; }

    public ICollection<RolePrivilege> RolePrivileges { get; private set; } = new List<RolePrivilege>();

    private Privilege() { }

    public static Privilege Create(string privilegeName, string? description, string category)
    {
        ValidatePrivilegeName(privilegeName);
        ValidateCategory(category);

        return new Privilege
        {
            PrivilegeName = privilegeName.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            Category = category.Trim(),
            CreatedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string privilegeName, string? description, string category)
    {
        ValidatePrivilegeName(privilegeName);
        ValidateCategory(category);

        PrivilegeName = privilegeName.Trim().ToUpperInvariant();
        Description = description?.Trim();
        Category = category.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public static Privilege Hydrate(
        int id,
        string privilegeName,
        string? description,
        string category,
        DateTime createdDate,
        DateTime createdAt,
        DateTime? updatedAt) =>
        new()
        {
            Id = id,
            PrivilegeName = privilegeName,
            Description = description,
            Category = category,
            CreatedDate = createdDate,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

    private static void ValidatePrivilegeName(string privilegeName)
    {
        if (string.IsNullOrWhiteSpace(privilegeName))
            throw new ArgumentException("Privilege name is required.", nameof(privilegeName));

        if (privilegeName.Length > MaxNameLength)
            throw new ArgumentException($"Privilege name cannot exceed {MaxNameLength} characters.", nameof(privilegeName));
    }

    private static void ValidateCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category is required.", nameof(category));

        if (category.Length > MaxCategoryLength)
            throw new ArgumentException($"Category cannot exceed {MaxCategoryLength} characters.", nameof(category));
    }
}
