using Facet;
using Infra.Entities;

namespace Service.DTOs;

[Facet(typeof(User), Include = [nameof(User.Id), nameof(User.Name), nameof(User.Email), nameof(User.Role)])]
public partial class UserDto {}