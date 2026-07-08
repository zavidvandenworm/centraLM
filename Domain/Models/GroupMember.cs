using Domain.Enums;

namespace Domain.Models;

public class GroupMember : Base
{
    public User User { get; set; } = null!;
    public required string UserId { get; set; }

    public Group Group { get; set; } = null!;
    public required string GroupId { get; set; }
    public required GroupMemberType GroupMemberType { get; set; }
}