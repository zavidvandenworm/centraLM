using API.DTO;
using API.EndpointFilters;
using API.Models;
using Application.Groups.Commands;
using Application.Services;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class Groups
{
    public static void AddGroupEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/group").RequireAuthorization().RequireUser();
        group.MapPost("/", async ([FromBody] CreateGroupDto createGroupDto, IMediator mediator, AuthUser user) =>
        {
            var cmd = createGroupDto.Adapt<CreateGroupCommand>() with
            {
                ParentGroupId = createGroupDto.ParentGroupId,
                UserId = user.Id
            };

            var result = await mediator.Send(cmd);
            return result.IsFailed ?
                Results.BadRequest(result.Errors) :
                Results.Ok(result.Value.Adapt<GroupListingDto>());
        }).Produces<GroupListingDto>();

        group.MapGet("/",
            async (int skip, int limit, string? parentGroupId, IMediator mediator, AuthUser user) =>
            {
                var cmd = new GetGroupsQuery(user.Id, skip, limit, parentGroupId);
                var result = await mediator.Send(cmd);

                return result.IsFailed
                    ? Results.BadRequest(result.Errors)
                    : Results.Ok(result.Value.Adapt<List<GroupListingDto>>());
            });
    }
}