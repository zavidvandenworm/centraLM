using Application.Groups.Commands;

namespace API.DTO;

public class GetGroupsDto
{
    public int Skip { get; set; }
    public int Limit { get; set; } = 25;
    public string? ParentGroupId { get; set; }
    public string? Keyword { get; set; }
    public GroupSortDirection SortByCreated { get; set; } = GroupSortDirection.Descending;
}
