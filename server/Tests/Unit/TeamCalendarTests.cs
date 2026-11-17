using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class TeamCalendarTests
{
    // December 2026: man 7. – søn 27.
    private static readonly DateOnly From = new(2026, 12, 7);
    private static readonly DateOnly To = new(2026, 12, 27);

    private static readonly Employee Mette = Employee("Mette Jensen", new DateOnly(2025, 9, 1));
    private static readonly Employee Jonas = Employee("Jonas Nielsen", new DateOnly(2024, 1, 1));
    private static readonly Employee Sofie = Employee("Sofie Andersen", new DateOnly(2026, 12, 15));                     // starter midt i perioden
    private static readonly Employee Peter = Employee("Peter Madsen", new DateOnly(2020, 1, 1), new DateOnly(2026, 12, 10)); // stopper midt i perioden
    private static readonly Employee Lars = Employee("Lars Holm", new DateOnly(2020, 1, 1), new DateOnly(2026, 11, 30));     // stoppet før perioden

    private static readonly Employee[] Employees = [Mette, Jonas, Sofie, Peter, Lars];

    private static readonly HolidayRequest[] Requests =
    [
        Request(Mette, 14, 18, RequestStatus.Approved),
        Request(Mette, 21, 22, RequestStatus.Rejected),   // afvist: vises ikke
        Request(Jonas, 16, 23, RequestStatus.Approved),
        Request(Jonas, 7, 8, RequestStatus.Pending),      // venter: vises ikke
        Request(Peter, 3, 8, RequestStatus.Approved)      // starter før perioden
    ];

    private static readonly IReadOnlySet<DateOnly> DaysOff = new HashSet<DateOnly> { new(2026, 12, 24), new(2026, 12, 25) };

    private static TeamCalendarResponse Build() => TeamCalendar.Build(Employees, Requests, DaysOff, From, To);

    private static TeamDay Day(TeamCalendarResponse calendar, int day) =>
        calendar.Days.Single(d => d.Date == new DateOnly(2026, 12, day));

    [Fact]
    public void Only_approved_holiday_is_shown()
    {
        var calendar = Build();

        var mette = calendar.Members.Single(m => m.EmployeeId == Mette.Id);
        var jonas = calendar.Members.Single(m => m.EmployeeId == Jonas.Id);

        Assert.Equal(new TeamAbsence(new DateOnly(2026, 12, 14), new DateOnly(2026, 12, 18)), Assert.Single(mette.Absences));
        Assert.Equal(new TeamAbsence(new DateOnly(2026, 12, 16), new DateOnly(2026, 12, 23)), Assert.Single(jonas.Absences));
    }

    [Fact]
    public void Holiday_that_starts_before_the_period_is_included()
    {
        var peter = Build().Members.Single(m => m.EmployeeId == Peter.Id);

        Assert.Single(peter.Absences);
    }

    [Fact]
    public void Everyone_employed_in_the_period_is_listed_even_without_holiday()
    {
        var names = Build().Members.Select(m => m.Name).ToList();

        Assert.Contains("Sofie Andersen", names);
        Assert.Contains("Peter Madsen", names);
        Assert.DoesNotContain("Lars Holm", names); // stoppede før perioden
    }

    [Fact]
    public void Members_are_sorted_by_name()
    {
        var names = Build().Members.Select(m => m.Name).ToList();

        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public void Counts_who_is_absent_and_present_on_a_day()
    {
        // Ons 16.12: Mette og Jonas har ferie. Sofie er startet, Peter er stoppet
        var day = Day(Build(), 16);

        Assert.True(day.IsWorkingDay);
        Assert.Equal(2, day.Absent);
        Assert.Equal(1, day.Present);
    }

    [Fact]
    public void Pending_holiday_does_not_count_as_absent()
    {
        // Man 7.12: Peter har godkendt ferie, Jonas' ansøgning venter stadig
        var day = Day(Build(), 7);

        Assert.Equal(1, day.Absent);
        Assert.Equal(2, day.Present); // Mette og Jonas. Sofie er ikke startet endnu
    }

    [Fact]
    public void Weekends_are_not_working_days()
    {
        var saturday = Day(Build(), 12);

        Assert.False(saturday.IsWorkingDay);
        Assert.Equal(0, saturday.Absent);
        Assert.Equal(0, saturday.Present);
    }

    [Fact]
    public void Public_holidays_and_company_days_off_are_not_working_days()
    {
        Assert.False(Day(Build(), 24).IsWorkingDay);
        Assert.False(Day(Build(), 25).IsWorkingDay);
    }

    [Fact]
    public void Every_day_in_the_period_is_included()
    {
        Assert.Equal(21, Build().Days.Count); // 7.–27. december
    }

    [Fact]
    public void End_before_start_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => TeamCalendar.Build(Employees, Requests, DaysOff, To, From));
    }

    private static Employee Employee(string name, DateOnly start, DateOnly? end = null) =>
        new() { Id = Guid.NewGuid(), Name = name, Email = $"{name.Split(' ')[0].ToLower()}@businessesbjerg.com", StartDate = start, EndDate = end };

    private static HolidayRequest Request(Employee employee, int startDay, int endDay, RequestStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            Account = HolidayAccount.Weeks1To5,
            StartDate = new DateOnly(2026, 12, startDay),
            EndDate = new DateOnly(2026, 12, endDay),
            Status = status
        };
}