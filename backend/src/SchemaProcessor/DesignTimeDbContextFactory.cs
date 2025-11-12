using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SchemaProcessor;

/// <summary>
/// This factory is used by the dotnet-ef tools to create an instance of the DbContext
/// at design time (e.g., when creating migrations or updating the database).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SchemaDbContext>
{
    public SchemaDbContext CreateDbContext(string[] args)
    {
        DotNetEnv.Env.Load();
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<SchemaDbContext>();
        var connectionString = configuration["EAGLE_CONNECTION_STRING"];
        optionsBuilder.UseNpgsql(connectionString);

        return new SchemaDbContext(optionsBuilder.Options);
    }
}