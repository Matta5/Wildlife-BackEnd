using Wildlife_BLL.Interfaces;
using Wildlife_DAL.Data;
using Wildlife_BLL;
using Wildlife_DAL;
using Microsoft.EntityFrameworkCore;
using dotenv.net;


var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

DotEnv.Load(); // This loads environment variables from .env
var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
    ?? throw new InvalidOperationException("Connection string 'DEFAULT_CONNECTION' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
