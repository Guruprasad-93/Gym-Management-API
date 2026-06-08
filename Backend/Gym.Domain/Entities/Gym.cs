namespace Gym.Domain.Entities;

public class Gym
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? LogoUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Gym() { }

    public static Gym Hydrate(
        Guid id,
        string name,
        string? address,
        string? phone,
        string? email,
        string? logoUrl,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt) =>
        new()
        {
            Id = id,
            Name = name,
            Address = address,
            Phone = phone,
            Email = email,
            LogoUrl = logoUrl,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

    public static Gym Create(string name, string? address = null, string? phone = null, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Gym name is required.", nameof(name));

        return new Gym
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Address = address?.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim()?.ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
