using SmartQueueApi.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
namespace SmartQueueApi.Infrastructure
{
    public static class ServiceRegistration
    {
        public static void RegisterDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>((_, options) =>
              options.UseNpgsql(
                  configuration.GetConnectionString("DefaultConnection"),
                  x =>
                  {
                      x.MigrationsHistoryTable("__migrations", "public");
                  }));
        }

        public static async Task InitialiseDatabaseAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();
        }
    }
}
