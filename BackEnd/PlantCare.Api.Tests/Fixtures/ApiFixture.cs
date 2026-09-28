using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PlantCare.Api.Data;
using Testcontainers.MsSql;

namespace PlantCare.Api.Tests.Fixtures;

// Spins up a real SQL Server container (Testcontainers) and points the app's AppDbContext at it,
// applying the real Migrations/ so integration tests exercise real relational/provider behavior
// (cascade rules, Restrict FKs, precision) instead of hiding it behind UseInMemoryDatabase — see
// .claude/skills/implement-full-functionality/references/testing.md.
public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Same image as BackEnd/docker-compose.yml.
    private readonly MsSqlContainer _mssql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    _mssql.GetConnectionString(),
                    sqlOptions => sqlOptions.UseNetTopologySuite()
                )
            );
        });
    }

    // xUnit v2: IAsyncLifetime.InitializeAsync/DisposeAsync return Task, NOT ValueTask (that's xUnit v3).
    public async Task InitializeAsync()
    {
        await _mssql.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(); // applies the real Migrations/ — exercises them too
    }

    public new async Task DisposeAsync()
    {
        await _mssql.DisposeAsync();
    }
}
