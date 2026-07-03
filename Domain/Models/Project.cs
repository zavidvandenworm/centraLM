namespace Domain.Models;

public class Project: Base
{
    public required Group Group { get; set; }
    public required string GroupId { get; set; }

    public List<VirtualKey> VirtualKeys { get; set; } = [];
}