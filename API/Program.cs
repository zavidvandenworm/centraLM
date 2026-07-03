using API.Endpoints;
using Application;
using Application.Pipelines;
using dotenv.net;
using Mediator;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DotEnv.Load();
}

builder.Services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationPipeline<,>));
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins([Environment.GetEnvironmentVariable("PUBLIC_FRONTEND_URL")!])
        .AllowAnyMethod().AllowCredentials()
    );
});
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.Authority = Environment.GetEnvironmentVariable("PUBLIC_OPENID_AUTHORITY");
        options.ClientId = Environment.GetEnvironmentVariable("PUBLIC_OPENID_CLIENTID");
        options.ClientSecret = Environment.GetEnvironmentVariable("OPENID_CLIENT_SECRET");
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });

builder.Services.AddAuthorization();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.AddGroupEndpoints();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapScalarApiReference(options =>
{
    options.AddHttpAuthentication("BearerAuth", null!);
});

app.UseHttpsRedirection();
app.Run();