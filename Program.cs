using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SakaeELearning.WebAPI.Data;
using SakaeELearning.WebAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SakaeELearning.WebAPI.Configurations;
using SakaeELearning.WebAPI.Services;
using System.Text;

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

// JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<ITokenService, TokenService>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings?.Secret ?? "default_secret_key");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCookie("ExternalCookie")
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings?.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    })
    .AddGoogle(options =>
    {
        options.SignInScheme = "ExternalCookie";
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "YOUR_CLIENT_ID";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "YOUR_CLIENT_SECRET";
    });

// CORS - Permite o frontend acessar a API
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",    // Vite dev server
                "http://localhost:3001",    // Vite alternate
                "http://localhost:3002",    // Vite alternate 2
                "http://localhost:5173",    // Vite standard
                "http://localhost:5174",    // Vite alternate
                "http://localhost:4173",    // Vite preview (production build)
                "http://127.0.0.1:3000",
                "https://sakae-e-learning-wh4qw.ondigitalocean.app/", // Production DigitalOcean
                "https://sakae-e-learning-v3.onrender.com/" // Production Render
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

// Habilitar Swagger em todos os ambientes (incluindo Produção) para facilitar debug
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sakae E-Learning API v1");
    // Opcional: Se quiser que o Swagger abra na raiz, descomente abaixo:
    // options.RoutePrefix = string.Empty;
});

// CORS deve vir ANTES de Authentication/Authorization
app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Endpoints nativos do Identity (register, login, refresh, etc)
// app.MapIdentityApi<User>().WithTags("Identity Auth");

// Controllers
app.MapControllers();

// ========================================
// MINIMAL APIs CUSTOMIZADAS
// ========================================

// Rota de teste
app.MapGet("/", () => "Sakae E-Learning API is running!");

// Logout (Minimal API)
// app.MapPost("/logout", async (SignInManager<User> signInManager) =>
// {
//     await signInManager.SignOutAsync();
//     return Results.Ok(new { success = true, message = "Logout realizado!" });
// }).RequireAuthorization().WithTags("Identity Auth");

// Bind dinâmico da porta para suportar o Railway (variável PORT)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8081";
app.Run($"http://0.0.0.0:{port}");
