using System.ComponentModel.DataAnnotations;

namespace API.DTO;

public class CreateGroupDto
{
    [MinLength(1), MaxLength(100)]
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? ParentGroupId { get; set; }
}