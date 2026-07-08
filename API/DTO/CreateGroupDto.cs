
namespace API.DTO;

public class CreateGroupDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? ParentGroupId { get; set; }
}