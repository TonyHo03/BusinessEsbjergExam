using Infra;
using Service.Approvals;
using Service.DTOs;

namespace Service;

public enum DecisionOutcome
{
    Done,
    NotFound,
    NotAllowed,
    Conflict
}

public record DecisionResult(
    DecisionOutcome Outcome,
    RequestStatus? NewStatus = null,
    string? Error = null);

public class ApprovalService(
    IApprovalRepository repository,
    TimeProvider time)
{
    public async Task<IReadOnlyList<PendingRequestDto>> ListPendingAsync(
        Guid? currentEmployeeId,
        CancellationToken ct) =>
        (await repository.ListPendingAsync(ct))
            .Select(r => new PendingRequestDto(
                r.Id,
                r.EmployeeId,
                r.EmployeeName,
                r.StartDate,
                r.EndDate,
                r.WorkingDays,
                r.Hours,
                r.Account,
                r.SubmittedAt,
                r.Note,
                IsOwnRequest: currentEmployeeId == r.EmployeeId))
            .ToList();

    public async Task<DecisionResult> DecideAsync(
        Guid requestId,
        RequestAction action,
        string? reason,
        Guid actorId,
        bool actorHasAccountingRights,
        CancellationToken ct)
    {
        var request = await repository.GetAsync(requestId, ct);
        if (request is null)
            return new(DecisionOutcome.NotFound);

        var context = new TransitionContext(
            ActorIsOwner: actorId == request.EmployeeId,
            ActorHasAccountingRights: actorHasAccountingRights,
            StartDate: request.StartDate,
            Today: DateOnly.FromDateTime(time.GetLocalNow().DateTime),
            Reason: reason);

        var transition = RequestStateMachine.Apply(
            request.Status,
            action,
            context);

        if (!transition.IsAllowed)
            return new(DecisionOutcome.NotAllowed, Error: transition.Error);

        var audit = new ApprovalAudit
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            ActorId = actorId,
            Action = action,
            FromStatus = request.Status,
            ToStatus = transition.NewStatus,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            OccurredAt = time.GetUtcNow()
        };

        var saved = await repository.TrySaveDecisionAsync(
            requestId,
            request.Status,
            transition.NewStatus,
            audit,
            ct);

        return saved
            ? new(DecisionOutcome.Done, transition.NewStatus)
            : new(
                DecisionOutcome.Conflict,
                Error: "This request was changed by someone else. Refresh and try again.");
    }
}