using System.Security.Claims;
using API.Endpoints;
using API.Extensions;
using Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

EnvironmentExtensions.LoadDotEnv(args);
EnvironmentExtensions.ValidateRequiredVariables();
await EnvironmentExtensions.ValidateOpenIdAuthorityAsync();

const string BearerSchemeName = "Bearer";

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<FluentValidationExceptionHandler>();
builder.Services.AddResultQueryValidation();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[BearerSchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "OpenID Connect access token issued by the configured authority."
        };
        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, _) =>
    {
        var requiresAuthorization = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAuthorizeData>()
            .Any();

        if (requiresAuthorization)
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(BearerSchemeName, context.Document)] = []
                }
            ];
        }

        return Task.CompletedTask;
    });
});
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
        options.TokenValidationParameters.ValidIssuer = options.Authority;

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer")
                    .LogError(context.Exception,
                        "Token validation failed for {Path}", context.Request.Path);
                return Task.CompletedTask;
            }
        };
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

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.AddGroupEndpoints();
app.AddUserEndpoints();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapScalarApiReference();

app.Run();