namespace Warehouse.Domain.Entities;

public enum UserRole
{
    User,
    Warehouseman,
    Administrator
}

public class User
{
    public int Id { get; private set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; }

    public User()
    {
    }

    public User(
        string username,
        string passwordHash,
        string email,
        UserRole role)
    {
        Username = username;
        PasswordHash = passwordHash;
        Email = email;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }
}