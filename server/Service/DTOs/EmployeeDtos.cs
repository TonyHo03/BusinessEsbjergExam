using Facet;
using Infra;

namespace Service.DTOs;

[Facet(typeof(Employee), Include = [nameof(Employee.Id), nameof(Employee.Name), nameof(Employee.Email), nameof(Employee.Role)])]
public partial class EmployeeDto {}