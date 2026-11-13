namespace Infra;

public interface IEmployeeDirectory
{
    Task<Guid?> FindEmployeeIdAsync(
        string identitySubject, CancellationToken ct);
}