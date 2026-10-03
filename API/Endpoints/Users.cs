using System.Security.Claims;
using API.DTO;
using API.Extensions;
using Application.Extensions;
using Application.Users.Commands;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class Users
{
    public static void AddUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users").RequireAuthorization();

        group.MapPost("me/register", async ([FromBody] RegisterMeDto registerMeDto, ClaimsPrincipal claims, IMediator mediator) =>
        {
            var subIss = claims.GetSubIss();
            var result = await mediator.Send(new RegisterMeQuery(subIss.Subject, subIss.Issuer, registerMeDto.DisplayName, registerMeDto.Biography));
            return result.IsSuccess
                ? Results.Ok(result.Value.Adapt<UserDto>())
                : result.ToProblemDetails();
        }).Produces<UserDto>().ProducesValidationProblem();

        group.MapGet("me", async (ClaimsPrincipal claims, IMediator mediator) =>
        {
            var subIss = claims.GetSubIss();
            var result = await mediator.Send(new GetMeQuery(subIss.Subject, subIss.Issuer));
            return result.IsSuccess ? Results.Ok(result.Value.Adapt<UserDto>()) : Results.NotFound();
        }).Produces<UserDto>();
    }
}