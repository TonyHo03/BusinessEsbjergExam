using Infra;
using LinqToDB;
using LinqToDB.Async;

namespace Service;

public record DayOffDto(Guid Id, DateOnly Date, string Name, bool IsCompany);

public record DayOffInput(DateOnly Date, string Name, bool IsCompany);

public record DayOffResult(IReadOnlyList<string> Errors, DayOffDto? Created);

// Helligdage og firmafridage. De koster ingen feriedage, når man søger ferie
public class DayOffService(AppDb db)
{
    public async Task<IReadOnlyList<DayOffDto>> GetAsync(int year) =>
        (await db.DaysOff
            .Where(d => d.Date >= new DateOnly(year, 1, 1) && d.Date <= new DateOnly(year, 12, 31))
            .OrderBy(d => d.Date)
            .ToListAsync())
        .Select(ToDto)
        .ToList();

    public async Task<DayOffResult> AddAsync(DayOffInput input, DateOnly today)
    {
        var existing = await db.DaysOff.Where(d => d.Date == input.Date).ToListAsync();

        var errors = DayOffRules.Validate(input.Date, input.Name, today, existing);
        if (errors.Count > 0)
            return new DayOffResult(errors, null);

        var dayOff = new DayOff
        {
            Id = Guid.NewGuid(),
            Date = input.Date,
            Name = input.Name.Trim(),
            IsCompany = input.IsCompany
        };

        await db.InsertAsync(dayOff);

        return new DayOffResult([], ToDto(dayOff));
    }

    // Null hvis slettet, ellers en fejlbesked. KeyNotFoundException hvis dagen ikke findes
    public async Task<string?> DeleteAsync(Guid id, DateOnly today)
    {
        var dayOff = await db.DaysOff.FirstOrDefaultAsync(d => d.Id == id) ?? throw new KeyNotFoundException();

        var error = DayOffRules.CanDelete(dayOff, today);
        if (error is not null)
            return error;

        await db.DaysOff.Where(d => d.Id == id).DeleteAsync();
        return null;
    }

    private static DayOffDto ToDto(DayOff d) => new(d.Id, d.Date, d.Name, d.IsCompany);
}