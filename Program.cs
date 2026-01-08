using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SakaeELearning.WebAPI.Data;
using SakaeELearning.WebAPI.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

//DB - Database
builder
    .Services.AddDbContext<AppDbContext>(
    options=> options
    .UseSqlServer(builder.Configuration
    .GetConnectionString("DefaultConnection")));

//builder.Services.AddAuthentication();
builder
    .Services
    .AddAuthorization();

builder.Services
    .AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Sakae E-Learning API",
        Description = "API for Sakae E-Learning platform with Identity endpoints"
    });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sakae E-Learning API v1");
    });
}

app.MapGet("/", (ClaimsPrincipal user) => user.Identity!.Name)
    .RequireAuthorization();

app.MapIdentityApi<User>();

app.UseAuthorization();

app.MapControllers();

app.Run();

