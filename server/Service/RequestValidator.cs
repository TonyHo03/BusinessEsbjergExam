using System.Text.Json.Serialization;
using Infra;

namespace Service;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IssueSeverity
{
    Error,   // ansøgningen kan ikke sendes
    Warning  // ansøgningen kan sendes, men brugeren gøres opmærksom på noget
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IssueCode
{
    NoWorkingDays,
    StartInPast,
    NotEnoughNotice,
    InsufficientBalance,
    UsesHolidayDebt,
    NegativeOn31August,
    ShortBlock,
    OverlapsExistingRequest
}

public record ValidationIssue(IssueCode Code, IssueSeverity Severity, string Message);

// En eksisterende ansøgning, der venter eller er godkendt
public record HolidayPeriod(DateOnly StartDate, DateOnly EndDate);

public record RequestValidation(IReadOnlyList<ValidationIssue> Issues, bool IsMainHoliday)
{
    public bool IsValid => Issues.All(i => i.Severity != IssueSeverity.Error);
}

public static class RequestValidator
{
    private const int MinimumBlockDays = 5;

    public static RequestValidation Validate(
        HolidayAccount account,
        RequestCalculation calc,
        BalanceOutlook outlook,
        ParameterResolver parameters,
        DateOnly today,
        IReadOnlyList<HolidayPeriod>? existing = null)
    {
        var issues = new List<ValidationIssue>();
        var start = calc.Days[0].Date;
        var isMainHoliday = MainHolidayRule.IsMainHoliday(account, calc, parameters);

        if (calc.WorkingDays == 0)
        {
            issues.Add(Error(IssueCode.NoWorkingDays, "The period contains no working days."));
            return new RequestValidation(issues, isMainHoliday);
        }

        if (start <= today)
            issues.Add(Error(IssueCode.StartInPast, "The holiday must start after today."));

        // De samme dage må ikke søges to gange – så ville de blive trukket to gange
        var end = calc.Days[^1].Date;
        var overlap = existing?.FirstOrDefault(e => e.StartDate <= end && e.EndDate >= start);
        if (overlap is not null)
            issues.Add(Error(IssueCode.OverlapsExistingRequest,
                $"You already have holiday or a request on {overlap.StartDate:dd.MM.yyyy}–{overlap.EndDate:dd.MM.yyyy} that overlaps."));

        // Varsel: 3 måneder for hovedferie, 1 måned for øvrig ferie
        var noticeMonths = parameters.GetInt(
            isMainHoliday ? ParameterKeys.NoticeMainHolidayMonths : ParameterKeys.NoticeOtherMonths, today);
        var earliestStart = today.AddMonths(noticeMonths);

        if (start < earliestStart)
            issues.Add(Error(IssueCode.NotEnoughNotice,
                $"{(isMainHoliday ? "Main holiday" : "Holiday")} requires {noticeMonths} month(s) notice. " +
                $"The earliest start is {earliestStart:dd.MM.yyyy}."));

        // Saldo: må gå op til 1 uge i minus (feriegæld) på uge 1–5
        var cost = calc.TotalHours;
        var available = outlook.AvailableAtStart;

        if (cost > available + outlook.DebtLimit)
            issues.Add(Error(IssueCode.InsufficientBalance, outlook.DebtLimit > 0
                ? $"The request costs {cost:0.##} h. You have {Math.Max(available, 0):0.##} h and can borrow at most {outlook.DebtLimit:0.##} h (1 week)."
                : $"The request costs {cost:0.##} h, but only {Math.Max(available, 0):0.##} h is available."));
        else if (cost > available)
            issues.Add(new ValidationIssue(IssueCode.UsesHolidayDebt, IssueSeverity.Warning,
                $"The request uses {cost - Math.Max(available, 0):0.##} h of holiday debt – holiday you earn later."));

        // Saldoen skal være 0 eller positiv den 31. august
        if (account == HolidayAccount.Weeks1To5 && outlook.AvailableAtYearEnd - cost < 0)
            issues.Add(Error(IssueCode.NegativeOn31August,
                $"Your balance would be {outlook.AvailableAtYearEnd - cost:0.##} h on {outlook.YearEnd:dd.MM.yyyy}. " +
                "It must be 0 or positive on that date."));

        // Ferie bør så vidt muligt holdes i blokke af mindst 5 dage
        if (calc.WorkingDays < MinimumBlockDays)
            issues.Add(new ValidationIssue(IssueCode.ShortBlock, IssueSeverity.Warning,
                "Holiday should be taken in blocks of at least 5 days where possible."));

        return new RequestValidation(issues, isMainHoliday);
    }

    private static ValidationIssue Error(IssueCode code, string message) =>
        new(code, IssueSeverity.Error, message);
}