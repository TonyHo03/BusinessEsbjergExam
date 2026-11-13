using LinqToDB.Mapping;

namespace Infra;

public enum LedgerEntryType
{
    [MapValue("Usage")] Usage,          // afholdt ferie
    [MapValue("Payout")] Payout,        // udbetalt
    [MapValue("Expiry")] Expiry,        // udløbet / sendt til feriefond
    [MapValue("Adjustment")] Adjustment // manuel rettelse fra bogholderiet
}

/// <summary>
/// Én bevægelse på en ferie-konto. Hours er med fortegn:
/// negativ trækker fra saldoen (fx afholdt ferie), positiv lægger til (fx en rettelse).
/// Optjening gemmes ikke her, den beregnes.
/// </summary>
[Table("ledger_entry")]
public class LedgerEntry
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("employee_id")] public Guid EmployeeId { get; set; }
    [Column("account")] public HolidayAccount Account { get; set; }
    [Column("earning_year")] public int EarningYear { get; set; }
    [Column("type")] public LedgerEntryType Type { get; set; }
    [Column("hours")] public decimal Hours { get; set; }
    [Column("entry_date")] public DateOnly EntryDate { get; set; }
    [Column("request_id")] public Guid? RequestId { get; set; }
}