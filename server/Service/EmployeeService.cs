using System.ComponentModel.DataAnnotations;
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

    // Alle har en organisationsmail (afklaret 16/11)
    private const string EmailDomain = "@businessesbjerg.com";

    public async Task<List<EmployeeDto>> GetAll()
    {
        return await db.Employees.Select(e => new EmployeeDto(e)).ToListAsync();
    }

    public async Task<EmployeeDto> Create(CreateEmployeeRequest request)
    {
        var email = CheckEmail(request.Email);

        if (await db.Employees.AnyAsync(e => e.Email == email))
            throw new InvalidOperationException("A user with this email already exists.");

        await CheckOnlyOne(request.Role, Guid.Empty);

        var employee = new Employee
        {
            Name = request.Name,
            Email = email,
            PasswordHash = hasher.Hash(request.Password),
            Role = request.Role,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        await db.InsertAsync(employee);

        return new EmployeeDto(employee);
    }

    public async Task Update(Guid id, UpdateEmployeeRequest request)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(u => u.Id == id) ?? throw new KeyNotFoundException();

        // Ellers kunne man ende uden en accountant eller system owner
        if (employee.Role != Roles.Employee && request.Role != employee.Role)
            throw new ValidationException("The accountant and system owner accounts cannot change role.");

        await CheckOnlyOne(request.Role, employee.Id);

        employee.Name = request.Name;
        employee.Email = CheckEmail(request.Email);
        employee.Role = request.Role;

        await db.UpdateAsync(employee);
    }

    public async Task SetActive(Guid id, Status status)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(u => u.Id == id) ?? throw new KeyNotFoundException();

        if (employee.Role != Roles.Employee && status == Status.Inactive)
            throw new ValidationException("The accountant and system owner accounts cannot be deactivated.");

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

    private static string CheckEmail(string email)
    {
        var trimmed = email.Trim();
        if (trimmed.Length <= EmailDomain.Length || !trimmed.EndsWith(EmailDomain, StringComparison.OrdinalIgnoreCase))
            throw new ValidationException($"The email must end with {EmailDomain}.");
        return trimmed;
    }

    // Der må kun være én accountant-konto og én system owner-konto (afklaret 16/11)
    private async Task CheckOnlyOne(Roles role, Guid exceptId)
    {
        if (role == Roles.Employee)
            return;

        if (await db.Employees.AnyAsync(e => e.Role == role && e.Id != exceptId))
            throw new ValidationException(role == Roles.Accountant
                ? "There is already an accountant account."
                : "There is already a system owner account.");
    }
}