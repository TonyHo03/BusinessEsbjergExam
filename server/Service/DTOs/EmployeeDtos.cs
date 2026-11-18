using Facet;
using Infra;

namespace Service.DTOs;

[Facet(typeof(Employee), Include = [nameof(Employee.Id), nameof(Employee.Name), nameof(Employee.Email), nameof(Employee.Role), nameof(Employee.Status)])]
public partial class EmployeeDto {}

[Facet(typeof(Employee),
    Include =
    [
        nameof(Employee.Name), nameof(Employee.Email), nameof(Employee.Role), nameof(Employee.StartDate),
        nameof(Employee.EndDate)
    ])]
public partial class CreateEmployeeRequest
{
    public string Password { get; set; } = "";
}

[Facet(typeof(Employee), Include = [nameof(Employee.Name), nameof(Employee.Email), nameof(Employee.Role), nameof(Employee.StartDate), nameof(Employee.EndDate)])]
public partial class UpdateEmployeeRequest {}