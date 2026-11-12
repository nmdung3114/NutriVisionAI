using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NutriVision.Infrastructure.Persistence;

/// <summary>Dùng cho `dotnet ef migrations`: migration được sinh cho SQL Server (provider chính của hệ thống).</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("NUTRIVISION_SQLSERVER")
                 ?? "Server=localhost,1433;Database=NutriVisionDb;User Id=sa;Password=placeholder;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(cs, o => o.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .Options;
        return new ApplicationDbContext(options);
    }
}
