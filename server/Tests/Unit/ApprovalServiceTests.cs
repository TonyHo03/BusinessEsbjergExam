using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class ApprovalServiceTests
{
    private static readonly Guid RequestId = Guid.NewGuid();
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid AccountingUserId = Guid.NewGuid();

    // 1.–5.12.2026: 5 dage, 37 t fra uge 1–5
    private static readonly HolidayRequestSnapshot PendingRequest = new(
        RequestId, OwnerId, "Sofie Lund", RequestStatus.Pending,
        new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 5), 5, 37m,
        HolidayAccount.Weeks1To5, DateTimeOffset.UtcNow, null);

    // Sofie er fuldtid og startede 1.9.2025, så hun har 24,96 dage fra 2025/26 og 6,24 fra 2026/27 den 1.12.2026
    private static readonly Employee Sofie = new()
        { Id = OwnerId, Name = "Sofie Lund", Role = Roles.Employee, StartDate = new DateOnly(2025, 9, 1) };

    private static readonly RuleParameter[] Parameters =
    [
        new() { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.08", EffectiveFrom = new DateOnly(2020, 1, 1) },
        new() { Key = ParameterKeys.Week6HoursPerMonth, Value = "3.083", EffectiveFrom = new DateOnly(2020, 1, 1) }
    ];

    private static LedgerData Data(params LedgerEntry[] entries) =>
        new(Sofie, [TestSchedules.FullTime()], entries, Parameters);

    private static ApprovalService CreateService(StubStore repository) => new(repository, TimeProvider.System);

    private static Task<DecisionResult> Decide(StubStore store, RequestAction action, string? reason = null) =>
        CreateService(store).DecideAsync(RequestId, action, reason, AccountingUserId, actorHasAccountingRights: true, default);

    [Fact]
    public async Task ListPending_FlagsTheCallersOwnRequests()
    {
        var service = CreateService(new StubStore(PendingRequest, Data()));

        Assert.True((await service.ListPendingAsync(OwnerId, default)).Single().IsOwnRequest);
        Assert.False((await service.ListPendingAsync(AccountingUserId, default)).Single().IsOwnRequest);
    }

    [Fact]
    public async Task Approving_subtracts_the_holiday_from_the_balance()
    {
        var store = new StubStore(PendingRequest, Data());

        var result = await Decide(store, RequestAction.Approve);

        Assert.Equal(DecisionOutcome.Done, result.Outcome);
        var entry = Assert.Single(store.SavedLedger);
        Assert.Equal(LedgerEntryType.Usage, entry.Type);
        Assert.Equal(2025, entry.EarningYear); // ældste periode først
        Assert.Equal(-5m, entry.Days);
        Assert.Equal(-37m, entry.Hours);
        Assert.Equal(RequestId, entry.RequestId);
    }

    [Fact]
    public async Task Approving_without_enough_days_is_refused_and_nothing_is_saved()
    {
        // Alt er brugt fra begge perioder, og gælden er allerede 5 dage
        var store = new StubStore(PendingRequest, Data(
            new LedgerEntry { Account = HolidayAccount.Weeks1To5, EarningYear = 2025, Type = LedgerEntryType.Usage, Days = -24.96m },
            new LedgerEntry { Account = HolidayAccount.Weeks1To5, EarningYear = 2026, Type = LedgerEntryType.Usage, Days = -11.24m }));

        var result = await Decide(store, RequestAction.Approve);

        Assert.Equal(DecisionOutcome.NotAllowed, result.Outcome);
        Assert.Empty(store.SavedAudits);
        Assert.Empty(store.SavedLedger);
    }

    [Fact]
    public async Task Rejecting_does_not_touch_the_balance()
    {
        var store = new StubStore(PendingRequest, Data());

        var result = await Decide(store, RequestAction.Reject, "Travl periode");

        Assert.Equal(DecisionOutcome.Done, result.Outcome);
        Assert.Empty(store.SavedLedger);
    }

    [Fact]
    public async Task Cancelling_approved_holiday_gives_the_days_back()
    {
        var approved = PendingRequest with { Status = RequestStatus.Approved, StartDate = new DateOnly(2099, 12, 1) };
        var usage = new LedgerEntry
        {
            Id = Guid.NewGuid(), EmployeeId = OwnerId, Account = HolidayAccount.Weeks1To5, EarningYear = 2025,
            Type = LedgerEntryType.Usage, Days = -5m, Hours = -37m, RequestId = RequestId
        };
        var store = new StubStore(approved, Data(usage));

        var result = await Decide(store, RequestAction.Cancel);

        Assert.Equal(DecisionOutcome.Done, result.Outcome);
        var reversal = Assert.Single(store.SavedLedger);
        Assert.Equal((2025, 5m, 37m), (reversal.EarningYear, reversal.Days, reversal.Hours));
        Assert.Equal(0m, usage.Days + reversal.Days);
    }

    [Fact]
    public async Task Cancelling_a_pending_request_does_not_touch_the_balance()
    {
        var store = new StubStore(PendingRequest, Data());

        var result = await Decide(store, RequestAction.Cancel);

        Assert.Equal(DecisionOutcome.Done, result.Outcome);
        Assert.Empty(store.SavedLedger);
    }

    private sealed class StubStore(HolidayRequestSnapshot? request, LedgerData data, bool saveSucceeds = true) : IApprovalRepository
    {
        public List<ApprovalAudit> SavedAudits { get; } = [];
        public List<LedgerEntry> SavedLedger { get; } = [];

        public Task<IReadOnlyList<HolidayRequestSnapshot>> ListPendingAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<HolidayRequestSnapshot>>(request is null ? [] : [request]);

        public Task<HolidayRequestSnapshot?> GetAsync(Guid requestId, CancellationToken ct) =>
            Task.FromResult(request?.Id == requestId ? request : null);

        public Task<LedgerData> GetLedgerDataAsync(Guid employeeId, CancellationToken ct) => Task.FromResult(data);

        public Task<bool> TrySaveDecisionAsync(Guid requestId, RequestStatus expected, RequestStatus next, ApprovalAudit audit,
            IReadOnlyList<LedgerEntry> ledgerEntries, CancellationToken ct)
        {
            if (saveSucceeds)
            {
                SavedAudits.Add(audit);
                SavedLedger.AddRange(ledgerEntries);
            }
            return Task.FromResult(saveSucceeds);
        }
    }
}