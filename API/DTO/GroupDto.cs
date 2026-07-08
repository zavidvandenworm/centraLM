using Domain.Models;

namespace API.DTO;

public class GroupListingDto : Base
{
    public required string Name { get; set; }
    public required string Description { get; set; }
}