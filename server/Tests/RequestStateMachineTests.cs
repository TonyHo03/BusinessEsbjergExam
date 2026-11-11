using Service.Approvals;
using Xunit;

namespace HolidayPlanner.UnitTests.Approvals;

public class RequestStateMachineTests
{
    private static readonly DateOnly Today = new(2026, 11, 10);
    private static readonly DateOnly FutureStart = Today.AddDays(30);

    private static TransitionContext Ctx(
        bool owner = false, bool accountingRights = false, DateOnly? start = null, string? reason = null) =>
        new(owner, accountingRights, start ?? FutureStart, Today, reason);

    [Fact]
    public void Approve_ByAccountingWhoIsNotTheOwner_Succeeds()
    {
        var result = RequestStateMachine.Apply(RequestStatus.Pending, RequestAction.Approve, Ctx(accountingRights: true));

        Assert.True(result.IsAllowed);
        Assert.Equal(RequestStatus.Approved, result.NewStatus);
    }

    [Fact]
    public void Approve_OwnRequest_IsDenied()
    {
        var result = RequestStateMachine.Apply(RequestStatus.Pending, RequestAction.Approve, Ctx(owner: true, accountingRights: true));

        Assert.False(result.IsAllowed);
        Assert.Equal(RequestStatus.Pending, result.NewStatus);
    }

    [Fact]
    public void Approve_ByNonAccounting_IsDenied()
    {
        var result = RequestStateMachine.Apply(RequestStatus.Pending, RequestAction.Approve, Ctx());

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public void Approve_AlreadyApproved_IsDenied()
    {
        var result = RequestStateMachine.Apply(RequestStatus.Approved, RequestAction.Approve, Ctx(accountingRights: true));

        Assert.False(result.IsAllowed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Reject_WithoutReason_IsDenied(string? reason)
    {
        var result = RequestStateMachine.Apply(RequestStatus.Pending, RequestAction.Reject, Ctx(accountingRights: true, reason: reason));

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public void Reject_WithReason_Succeeds()
    {
        var result = RequestStateMachine.Apply(
            RequestStatus.Pending, RequestAction.Reject, Ctx(accountingRights: true, reason: "Insufficient cover that week."));

        Assert.True(result.IsAllowed);
        Assert.Equal(RequestStatus.Rejected, result.NewStatus);
    }

    [Fact]
    public void Cancel_PendingByOwner_Succeeds()
    {
        var result = RequestStateMachine.Apply(RequestStatus.Pending, RequestAction.Cancel, Ctx(owner: true));

        Assert.True(result.IsAllowed);
        Assert.Equal(RequestStatus.Cancelled, result.NewStatus);
    }

    [Fact]
    public void Cancel_PendingBySomeoneElse_IsDenied()
    {
        var result = RequestStateMachine.Apply(RequestStatus.Pending, RequestAction.Cancel, Ctx());

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public void Cancel_ApprovedByOwner_IsDenied()
    {
        var result = RequestStateMachine.Apply(RequestStatus.Approved, RequestAction.Cancel, Ctx(owner: true));

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public void Cancel_ApprovedByAccountingBeforeStart_Succeeds()
    {
        var result = RequestStateMachine.Apply(RequestStatus.Approved, RequestAction.Cancel, Ctx(accountingRights: true));

        Assert.True(result.IsAllowed);
        Assert.Equal(RequestStatus.Cancelled, result.NewStatus);
    }

    [Theory]
    [InlineData(0)]   // starts today
    [InlineData(-3)]  // already under way
    public void Cancel_ApprovedOnceHolidayHasStarted_IsDenied(int startOffsetDays)
    {
        var result = RequestStateMachine.Apply(
            RequestStatus.Approved, RequestAction.Cancel, Ctx(accountingRights: true, start: Today.AddDays(startOffsetDays)));

        Assert.False(result.IsAllowed);
    }

    [Theory]
    [InlineData(RequestStatus.Rejected, RequestAction.Approve)]
    [InlineData(RequestStatus.Rejected, RequestAction.Reject)]
    [InlineData(RequestStatus.Rejected, RequestAction.Cancel)]
    [InlineData(RequestStatus.Cancelled, RequestAction.Approve)]
    [InlineData(RequestStatus.Cancelled, RequestAction.Reject)]
    [InlineData(RequestStatus.Cancelled, RequestAction.Cancel)]
    public void FinalStates_RejectEveryAction(RequestStatus status, RequestAction action)
    {
        var result = RequestStateMachine.Apply(status, action, Ctx(accountingRights: true, reason: "x"));

        Assert.False(result.IsAllowed);
        Assert.Equal(status, result.NewStatus);
    }
}