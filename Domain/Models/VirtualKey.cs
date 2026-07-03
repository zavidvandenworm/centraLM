namespace Domain.Models;

public class VirtualKey: Base
{
    public required Project Project { get; set; }
    public required string ProjectId { get; set; }
}