using System.Security.Claims;
using API.Endpoints;
using Application;
using Application.Pipelines;
using dotenv.net;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment()) DotEnv.Load();

builder.Services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationPipeline<,>));
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins(Environment.GetEnvironmentVariable("PUBLIC_FRONTEND_URL")!)
        .AllowAnyMethod().AllowCredentials().AllowAnyHeader()
    );
});
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = Environment.GetEnvironmentVariable("PUBLIC_OPENID_AUTHORITY");
        options.Audience = Environment.GetEnvironmentVariable("PUBLIC_OPENID_CLIENTID");
    });

builder.Services.AddAuthorization(opts =>
{
    var requirements = opts.DefaultPolicy.Requirements.ToList();
    requirements.Add(new ClaimsAuthorizationRequirement(ClaimTypes.NameIdentifier, null));
    requirements.Add(new DenyAnonymousAuthorizationRequirement());

    opts.DefaultPolicy =
        new AuthorizationPolicy(requirements, opts.DefaultPolicy.AuthenticationSchemes);
});
var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.AddGroupEndpoints();
app.AddUserEndpoints();

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem()
            .ExecuteAsync(context)));

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapScalarApiReference();

app.Run();