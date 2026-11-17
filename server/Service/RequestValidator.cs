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
    ShortBlock
}

public record ValidationIssue(IssueCode Code, IssueSeverity Severity, string Message);

public record RequestValidation(IReadOnlyList<ValidationIssue> Issues, bool IsMainHoliday)
{
    public bool IsValid => Issues.All(i => i.Severity != IssueSeverity.Error);
}

/// <summary>
/// Tjekker en ferieansøgning mod reglerne. Returnerer alle problemer på én gang,
/// så brugeren kan se dem samlet i stedet for ét ad gangen.
/// </summary>
public static class RequestValidator
{
    private const int MinimumBlockDays = 5;

    public static RequestValidation Validate(
        HolidayAccount account,
        RequestCalculation calc,
        BalanceOutlook outlook,
        ParameterResolver parameters,
        DateOnly today)
    {
        var issues = new List<ValidationIssue>();
        var start = calc.Days[0].Date;
        var isMainHoliday = MainHolidayRule.IsMainHoliday(account, calc, parameters);

        if (calc.WorkingDays == 0)
        {
            issues.Add(Error(IssueCode.NoWorkingDays, "Perioden indeholder ingen arbejdsdage."));
            return new RequestValidation(issues, isMainHoliday);
        }

        if (start <= today)
            issues.Add(Error(IssueCode.StartInPast, "Ferien skal starte efter i dag."));

        // Varsel: 3 måneder for hovedferie, 1 måned for øvrig ferie
        var noticeMonths = parameters.GetInt(
            isMainHoliday ? ParameterKeys.NoticeMainHolidayMonths : ParameterKeys.NoticeOtherMonths, today);
        var earliestStart = today.AddMonths(noticeMonths);

        if (start < earliestStart)
            issues.Add(Error(IssueCode.NotEnoughNotice,
                $"{(isMainHoliday ? "Hovedferie" : "Ferie")} skal varsles {noticeMonths} måned(er) før. " +
                $"Tidligste start er {earliestStart:dd.MM.yyyy}."));

        // Saldo: må gå op til 1 uge i minus (feriegæld) på uge 1–5
        var cost = calc.TotalHours;
        var available = outlook.AvailableAtStart;

        if (cost > available + outlook.DebtLimit)
            issues.Add(Error(IssueCode.InsufficientBalance, outlook.DebtLimit > 0
                ? $"Ansøgningen koster {cost:0.##} t. Du har {Math.Max(available, 0):0.##} t og kan højst låne {outlook.DebtLimit:0.##} t (1 uge)."
                : $"Ansøgningen koster {cost:0.##} t, men der er kun {Math.Max(available, 0):0.##} t til rådighed."));
        else if (cost > available)
            issues.Add(new ValidationIssue(IssueCode.UsesHolidayDebt, IssueSeverity.Warning,
                $"Ansøgningen bruger {cost - Math.Max(available, 0):0.##} t feriegæld – ferie du først optjener senere."));

        // Saldoen skal være 0 eller positiv den 31. august
        if (account == HolidayAccount.Weeks1To5 && outlook.AvailableAtYearEnd - cost < 0)
            issues.Add(Error(IssueCode.NegativeOn31August,
                $"Saldoen ville være {outlook.AvailableAtYearEnd - cost:0.##} t den {outlook.YearEnd:dd.MM.yyyy}. " +
                "Den skal være 0 eller positiv på den dato."));

        // Ferie bør så vidt muligt holdes i blokke af mindst 5 dage
        if (calc.WorkingDays < MinimumBlockDays)
            issues.Add(new ValidationIssue(IssueCode.ShortBlock, IssueSeverity.Warning,
                "Ferie bør så vidt muligt holdes i blokke af mindst 5 dage."));

        return new RequestValidation(issues, isMainHoliday);
    }

    private static ValidationIssue Error(IssueCode code, string message) =>
        new(code, IssueSeverity.Error, message);
}