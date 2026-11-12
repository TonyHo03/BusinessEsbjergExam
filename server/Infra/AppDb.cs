using LinqToDB;
using LinqToDB.Data;

namespace Infra;

public class AppDb(DataOptions<AppDb> options) : DataConnection(options.Options)
{
    public ITable<Employee> Employees => this.GetTable<Employee>();
    public ITable<RuleParameter> RuleParameters => this.GetTable<RuleParameter>();
    public ITable<WorkSchedule> WorkSchedules => this.GetTable<WorkSchedule>();
    public ITable<DayOff> DaysOff => this.GetTable<DayOff>();
    public ITable<HolidayRequest> HolidayRequests => this.GetTable<HolidayRequest>();
    public ITable<LedgerEntry> LedgerEntries => this.GetTable<LedgerEntry>();
}