using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Wildlife_DAL.Data;
using Wildlife_BLL;
using Wildlife_BLL.Interfaces;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) || d.ServiceType == typeof(AppDbContext))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add new InMemoryDatabase
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Mock the ImageService
            var mockImageClient = new Mock<IImageClient>();
            mockImageClient.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("https://test-image-url.com/image.jpg");

            services.AddScoped<IImageClient>(sp => mockImageClient.Object);
            services.AddScoped<ImageService>();

            // Build the service provider
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
