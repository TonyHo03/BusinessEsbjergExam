using Infra;
using Service;

namespace Tests.Unit;

public class ApprovalServiceTests
{
    private static readonly Guid RequestId = Guid.NewGuid();
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid AccountingUserId = Guid.NewGuid();

    private static readonly HolidayRequestSnapshot PendingRequest = new(
        RequestId, OwnerId, "Sofie Lund", RequestStatus.Pending,
        new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 5), 5, 37m,
        HolidayAccount.Weeks1To5, DateTimeOffset.UtcNow, null);

    private static ApprovalService CreateService(StubStore repository) => new(repository, TimeProvider.System);


    [Fact]
    public async Task ListPending_FlagsTheCallersOwnRequests()
    {
        var service = CreateService(new StubStore(PendingRequest));

        Assert.True((await service.ListPendingAsync(OwnerId, default)).Single().IsOwnRequest);
        Assert.False((await service.ListPendingAsync(AccountingUserId, default)).Single().IsOwnRequest);
    }

    private sealed class StubStore(HolidayRequestSnapshot? request, bool saveSucceeds = true) : IApprovalRepository
    {
        public List<ApprovalAudit> Saved { get; } = [];

        public Task<IReadOnlyList<HolidayRequestSnapshot>> ListPendingAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<HolidayRequestSnapshot>>(request is null ? [] : [request]);

        public Task<HolidayRequestSnapshot?> GetAsync(Guid requestId, CancellationToken ct) =>
            Task.FromResult(request?.Id == requestId ? request : null);

        public Task<bool> TrySaveDecisionAsync(
            Guid requestId, RequestStatus expected, RequestStatus next, ApprovalAudit audit, CancellationToken ct)
        {
            if (saveSucceeds) Saved.Add(audit);
            return Task.FromResult(saveSucceeds);
        }
    }
}