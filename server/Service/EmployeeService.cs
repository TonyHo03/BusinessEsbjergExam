using Infra;
using LinqToDB;
using LinqToDB.Async;
using Service.DTOs;
using Employee = Infra.Employee;

namespace Service;

public class EmployeeService(AppDb db, PasswordHasher hasher)
{
    
    private const int MinPasswordLength = 12;
    private const int MaxPasswordLength = 128;

    public async Task<List<EmployeeDto>> GetAll()
    {
        return await db.Employees.Select(e => new EmployeeDto(e)).ToListAsync();
    }

    public async Task Create(CreateEmployeeRequest request)
    {
        var employee = new Employee
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = hasher.Hash(request.Password),
            Role = request.Role,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        await db.InsertAsync(employee);
    }

    public async Task Update(Guid id, UpdateEmployeeRequest request)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(u => u.Id == id) ?? throw new KeyNotFoundException();

        employee.Name = request.Name;
        employee.Email = request.Email;
        employee.Role = request.Role;
        employee.StartDate = request.StartDate;
        employee.EndDate = request.EndDate;

        await db.UpdateAsync(employee);
    }

    public async Task SetActive(Guid id, Status status)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(u => u.Id == id) ?? throw new KeyNotFoundException();

        employee.Status = status;

        await db.UpdateAsync(employee);
    }
    

    public async Task<Employee> GetByEmail(string email)
    {
        return await db.Employees.FirstOrDefaultAsync(u => u.Email == email) ?? throw new KeyNotFoundException();
    }

    public async Task<Employee> GetById(Guid guid)
    {
        return await db.Employees.FirstOrDefaultAsync(u => u.Id == guid) ?? throw new KeyNotFoundException();
    }
    
    public async Task<EmployeeDto?> Login(string email, string password)
    {
        if (password.Length > MaxPasswordLength) return null;

        var employee = await GetByEmail(email);
        var ok = hasher.Verify(employee?.PasswordHash ?? hasher.DummyHash, password);
        return employee is not null && ok ? new EmployeeDto(employee) : null;
    }
    
}