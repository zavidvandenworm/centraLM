using Domain.Enums;

namespace Domain.Models;

public class GroupMember: Base
{
    public required User User { get; set; }
    public required string UserId { get; set; }
    
    public required Group Group { get; set; }
    public required string GroupId { get; set; }
    public required GroupMemberType GroupMemberType { get; set; }
}