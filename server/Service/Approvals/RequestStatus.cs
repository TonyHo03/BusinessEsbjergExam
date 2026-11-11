namespace Service.Approvals;

public enum RequestStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public enum RequestAction
{
    Approve,
    Reject,
    Cancel
}