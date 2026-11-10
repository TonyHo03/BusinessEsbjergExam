namespace Service.Approvals;

public readonly record struct TransitionContext(
    bool ActorIsOwner,
    bool ActorHasAccountingRights,
    DateOnly StartDate,
    DateOnly Today,
    string? Reason);

public readonly record struct TransitionResult(bool IsAllowed, RequestStatus NewStatus, string? Error)
{
    public static TransitionResult Allow(RequestStatus newStatus) => new(true, newStatus, null);
    public static TransitionResult Deny(RequestStatus current, string error) => new(false, current, error);
}

public class RequestStateMachine
{   
    public static TransitionResult Apply(RequestStatus current, RequestAction action, TransitionContext ctx)
    {
        if (current is RequestStatus.Rejected or RequestStatus.Cancelled)
        {
            return TransitionResult.Deny(current,
                $"A {current.ToString().ToLowerInvariant()} request cannot be changed");
        }

        return action switch
        {
            RequestAction.Approve => Decide(current, RequestStatus.Approved, "approve", requireReason: false, ctx),
            RequestAction.Reject => Decide(current, RequestStatus.Rejected, "reject", requireReason: true, ctx),
            RequestAction.Cancel => Cancel(current, ctx), 
            _=> TransitionResult.Deny(current, "Unknown Action")
        };
    }

    private static TransitionResult Decide(RequestStatus current, RequestStatus target, string verb, bool requireReason,
        TransitionContext ctx)
    {
        if (current != RequestStatus.Pending)
            return TransitionResult.Deny(current, $"Only pending requests can be {target.ToString().ToLowerInvariant()}");
        if (!ctx.ActorHasAccountingRights)
            return TransitionResult.Deny(current, $"Only accounting can  {verb} requests");
        if (ctx.ActorIsOwner)
            return TransitionResult.Deny(current, $"You cannot  {verb} your own request. Another approval is required");
        if (requireReason && string.IsNullOrWhiteSpace(ctx.Reason))
            return TransitionResult.Deny(current, $"A reason is required when rejecting a request");

        return TransitionResult.Allow(target);
    }

    private static TransitionResult Cancel(RequestStatus current, TransitionContext ctx)
    {
        if (current == RequestStatus.Pending)
        {
            return ctx.ActorIsOwner || ctx.ActorHasAccountingRights
                ? TransitionResult.Allow(RequestStatus.Cancelled)
                : TransitionResult.Deny(current, $"You can only cancel your own requests");
        }

        if (!ctx.ActorHasAccountingRights)
            return TransitionResult.Deny(current, "Approved leave can only be cancelled by accounting or an admin");


        if (ctx.Today >= ctx.StartDate)
            return TransitionResult.Deny(current, "This holiday has already started and cannot be cancelled");
        
        return TransitionResult.Allow(RequestStatus.Cancelled);
    }
}