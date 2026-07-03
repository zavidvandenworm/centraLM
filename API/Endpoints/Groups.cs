using System.Security.Claims;
using API.DTO;
using API.Extensions;
using Application.Groups.Commands;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class Groups
{
    public static void AddGroupEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/group").RequireAuthorization();
        
        group.MapPost("/", async ([FromBody] CreateGroupDto createGroupDto, ClaimsPrincipal claims, IMediator mediator) =>
        {
            var userId = claims.GetId();
            if (userId.IsFailed)
            {
                return Results.Unauthorized();
            }
            
            var cmd = createGroupDto.Adapt<CreateGroupCommand>() with
            {
                ParentGroupId = createGroupDto.ParentGroupId, 
                UserId = claims.GetId().Value
            };
            
            var result = await mediator.Send(cmd);
            return result.IsFailed ? 
                Results.BadRequest(result.Errors) : 
                Results.Ok(result.Value.Adapt<GroupListingDto>());
        }).Produces<GroupListingDto>();

        group.MapGet("/",
            async (int skip, int limit, string? parentGroupId, ClaimsPrincipal claims, IMediator mediator) =>
            {
                var userId = claims.GetId();
                if (userId.IsFailed)
                {
                    return Results.Unauthorized();
                }

                var cmd = new GetGroupsQuery(userId.Value, skip, limit, parentGroupId);
                var result = await mediator.Send(cmd);

                return result.IsFailed
                    ? Results.BadRequest(result.Errors)
                    : Results.Ok(result.Value.Adapt<List<GroupListingDto>>());
            });
    }
}