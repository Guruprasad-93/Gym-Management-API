namespace Gym.Domain.Entities;

/// <summary>
/// Application user account.
/// </summary>
public class User
{
    public const int MaxNameLength = 100;
    public const int MaxEmailLength = 256;
    public const int MaxPasswordLength = 500;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Hashed password value. Plain text must never be persisted.
    /// </summary>
    public string Password { get; private set; } = string.Empty;

    public Guid? GymId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int TokenVersion { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private User()
    {
    }

    public static User Create(string name, string email, string passwordHash, Guid? gymId = null)
    {
        ValidateName(name);
        ValidateEmail(email);
        ValidatePassword(passwordHash);

        return new User
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Password = passwordHash,
            GymId = gymId,
            CreatedDate = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string name, string email)
    {
        ValidateName(name);
        ValidateEmail(email);

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
    }

    public void ChangePassword(string passwordHash)
    {
        ValidatePassword(passwordHash);
        Password = passwordHash;
    }

    public void SetGymId(Guid? gymId) => GymId = gymId;

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public static User Hydrate(
        Guid id,
        string name,
        string email,
        string password,
        Guid? gymId,
        DateTime createdDate,
        bool isActive = true,
        int tokenVersion = 0,
        string? passwordResetToken = null,
        DateTime? passwordResetTokenExpiresAt = null) =>
        new()
        {
            Id = id,
            Name = name,
            Email = email,
            Password = password,
            GymId = gymId,
            IsActive = isActive,
            TokenVersion = tokenVersion,
            PasswordResetToken = passwordResetToken,
            PasswordResetTokenExpiresAt = passwordResetTokenExpiresAt,
            CreatedDate = createdDate
        };

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters.", nameof(name));
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        if (email.Length > MaxEmailLength)
            throw new ArgumentException($"Email cannot exceed {MaxEmailLength} characters.", nameof(email));

        if (!email.Contains('@'))
            throw new ArgumentException("Email format is invalid.", nameof(email));
    }

    private static void ValidatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        if (passwordHash.Length > MaxPasswordLength)
            throw new ArgumentException($"Password cannot exceed {MaxPasswordLength} characters.", nameof(passwordHash));
    }
}
