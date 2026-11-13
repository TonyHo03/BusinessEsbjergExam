using System.ComponentModel.DataAnnotations;
using Infra;
using LinqToDB;
using LinqToDB.Async;

namespace Service;

public record RequestInput(
    Guid EmployeeId,
    HolidayAccount Account,
    DateOnly StartDate,
    DateOnly EndDate,
    [MaxLength(500)] string? Comment);

public record RequestPreview(
    IReadOnlyList<RequestDay> Days,
    decimal TotalHours,
    int WorkingDays,
    bool IsMainHoliday,
    bool IsValid,
    IReadOnlyList<ValidationIssue> Issues);

public record RequestDto(
    Guid Id,
    HolidayAccount Account,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Hours,
    RequestStatus Status,
    string? Comment,
    string? RejectionReason,
    DateTimeOffset CreatedAt);

public record SubmitResult(RequestPreview Preview, RequestDto? Created);

/// <summary>
/// Ferieansøgninger: beregn og valider (preview), gem (submit) og vis.
/// Al validering sker her på serveren, uanset hvad klienten har tjekket.
/// </summary>
public class RequestService(AppDb db)
{
    /// <summary>Beregner og validerer uden at gemme. Null hvis medarbejderen ikke findes.</summary>
    public async Task<RequestPreview?> PreviewAsync(RequestInput input, DateOnly today)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == input.EmployeeId);
        if (employee is null)
            return null;

        var schedules = await db.WorkSchedules.Where(s => s.EmployeeId == employee.Id).ToListAsync();
        var entries = await db.LedgerEntries.Where(e => e.EmployeeId == employee.Id).ToListAsync();
        var parameters = new ParameterResolver(await db.RuleParameters.ToListAsync());

        var daysOff = await db.DaysOff
            .Where(d => d.Date >= input.StartDate && d.Date <= input.EndDate)
            .Select(d => d.Date)
            .ToListAsync();

        // Timer i ansøgninger der venter på godkendelse er reserveret
        var reservedHours = await db.HolidayRequests
            .Where(r => r.EmployeeId == employee.Id
                        && r.Account == input.Account
                        && r.Status == RequestStatus.Pending)
            .SumAsync(r => (decimal?)r.Hours) ?? 0m; // SUM giver NULL, når der ingen rækker er

        var calc = RequestCalculator.Calculate(input.StartDate, input.EndDate, schedules, daysOff.ToHashSet());
        var balances = Balances.For(employee, schedules, parameters, entries, today);
        var validation = RequestValidator.Validate(input.Account, calc, balances, reservedHours, parameters, today);

        return new RequestPreview(
            calc.Days,
            calc.TotalHours,
            calc.WorkingDays,
            validation.IsMainHoliday,
            validation.IsValid,
            validation.Issues);
    }

    /// <summary>Gemmer ansøgningen som Pending, hvis den er gyldig. Null hvis medarbejderen ikke findes.</summary>
    public async Task<SubmitResult?> SubmitAsync(RequestInput input, DateOnly today)
    {
        var preview = await PreviewAsync(input, today);
        if (preview is null)
            return null;

        if (!preview.IsValid)
            return new SubmitResult(preview, null);

        var request = new HolidayRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = input.EmployeeId,
            Account = input.Account,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            Hours = preview.TotalHours,
            Status = RequestStatus.Pending,
            Comment = input.Comment,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await db.InsertAsync(request);

        return new SubmitResult(preview, ToDto(request));
    }

    public async Task<IReadOnlyList<RequestDto>> GetForEmployeeAsync(Guid employeeId)
    {
        var requests = await db.HolidayRequests
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();

        return requests.Select(ToDto).ToList();
    }

    private static RequestDto ToDto(HolidayRequest r) =>
        new(r.Id, r.Account, r.StartDate, r.EndDate, r.Hours, r.Status, r.Comment, r.RejectionReason, r.CreatedAt);
}