using LinqToDB.Mapping;

namespace Infra;

public enum HolidayAccount
{
    [MapValue("Weeks1to5")] Weeks1To5, // Ferieloven: uge 1–5
    [MapValue("Week6")] Week6          // KL 05.11: 6. ferieuge
}