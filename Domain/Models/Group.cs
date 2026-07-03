namespace Domain.Models;

public class Group: Base
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public required string Path { get; set; }
    public Group? ParentGroup { get; set; }
    public string? ParentGroupId { get; set; }

    public List<GroupMember> GroupMembers { get; set; } = [];
    
    public List<Group> Children { get; set; } = [];
    public List<Project> Projects { get; set; } = [];
}