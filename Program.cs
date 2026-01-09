using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SakaeELearning.WebAPI.Data;
using SakaeELearning.WebAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// SERVIÇOS
// ========================================

// Database
// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("{password}"))
{
    if (string.IsNullOrEmpty(dbPassword))
    {
         throw new InvalidOperationException("DB_PASSWORD environment variable is not set.");
    }
    connectionString = connectionString.Replace("{password}", dbPassword);
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString ?? builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity com endpoints de API (login, register, etc)
builder.Services
    .AddIdentityApiEndpoints<User>(options => 
    {
        // Configurações de Senha
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false; 
        options.Password.RequiredLength = 6; 

        // Configurações de Login
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false; // CRUCIAL para login funcionar sem email sender

        // Configurações de Usuário
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization();

// CORS - Permite o frontend acessar a API
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",    // Vite dev server
                "http://localhost:5173",    // Vite alternativo
                "http://localhost:4173",    // Vite preview (production build)
                "http://127.0.0.1:3000",
                "https://sakae-e-learning-wh4qw.ondigitalocean.app" // Production
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // Necessário para cookies/tokens
    });
});

// Controllers
builder.Services.AddControllers();

// Swagger
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

var app = builder.Build();

// ========================================
// PIPELINE
// ========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sakae E-Learning API v1");
    });
}

// CORS deve vir ANTES de Authentication/Authorization
app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Endpoints nativos do Identity (register, login, refresh, etc)
app.MapIdentityApi<User>().WithTags("Identity Auth");

// Controllers
app.MapControllers();

// ========================================
// MINIMAL APIs CUSTOMIZADAS
// ========================================

// Rota de teste
app.MapGet("/", () => "Sakae E-Learning API is running!");

// Logout (Minimal API)
app.MapPost("/logout", async (SignInManager<User> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok(new { success = true, message = "Logout realizado!" });
}).RequireAuthorization().WithTags("Identity Auth");

app.Run();
