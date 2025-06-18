using Microsoft.EntityFrameworkCore;
using Wildlife_BLL.Interfaces;
using Wildlife_DAL.Data;
using Wildlife_BLL;
using Wildlife_DAL;
using Wildlife_EIL;
using dotenv.net;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Wildlife_API.Hubs;
using Wildlife_API.Services;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load();
var env = DotEnv.Read();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var MyAllowSpecificOrigins = "MyAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://localhost:5173",
                                "http://localhost:5173",
                                "http://localhost:3000",
                                "http://localhost:4173")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["token"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IObservationRepository, ObservationRepository>();
builder.Services.AddScoped<ObservationService>();
builder.Services.AddScoped<ISpeciesRepository, SpeciesRepository>();
builder.Services.AddScoped<SpeciesService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddHttpClient<IIdentifyClient, IdentifyClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "WildlifeApp/1.0");
});
builder.Services.AddScoped<IdentifyService>();

builder.Services.AddHttpClient<IExternalSpeciesClient, ExternalSpeciesClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "WildlifeApp/1.0");
});

builder.Services.AddHttpClient<IImageClient, ImageClient>();
builder.Services.AddScoped<ImageService>();

// Add SignalR
builder.Services.AddSignalR();

// Register SignalR notification service
builder.Services.AddScoped<IObservationNotificationService, SignalRNotificationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hub
app.MapHub<ObservationHub>("/observationHub");

app.MapControllers();
app.Run();

public partial class Program { }