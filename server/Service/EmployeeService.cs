using Infra;
using Service.DTOs;
using Employee = Infra.Employee;

namespace Service;

public class EmployeeService(AppDb db, PasswordHasher hasher)
{
    
    private const int MinPasswordLength = 12;
    private const int MaxPasswordLength = 128;

    public Employee GetByEmail(string email)
    {
        return db.Employees.FirstOrDefault(u => u.Email == email) ?? throw new KeyNotFoundException();
    }
    
    public EmployeeDto? Login(string email, string password)
    {
        if (password.Length > MaxPasswordLength) return null;

        var employee = GetByEmail(email);
        var ok = hasher.Verify(employee?.PasswordHash ?? hasher.DummyHash, password);
        return employee is not null && ok ? new EmployeeDto(employee) : null;
    }
    
}