using Domain.Enums;
using Domain.Models;

namespace API.DTO;

public class UserDto : Base
{
    public required UserType UserType { get; set; }
    public string DisplayName { get; set; } = "User";
    public string Biography { get; set; } = "";
}