using Infra;
using Infra.Entities;
using Service.DTOs;

namespace Service;

public class UserService(AppDb db, PasswordHasher hasher)
{
    
    private const int MinPasswordLength = 12;
    private const int MaxPasswordLength = 128;

    public User GetByEmail(string email)
    {
        return db.Users.FirstOrDefault(u => u.Email == email) ?? throw new KeyNotFoundException();
    }
    
    public UserDto? Login(string email, string password)
    {
        if (password.Length > MaxPasswordLength) return null;

        var user = GetByEmail(email);
        var ok = hasher.Verify(user?.PasswordHash ?? hasher.DummyHash, password);
        return user is not null && ok ? new UserDto(user) : null;
    }
    
}