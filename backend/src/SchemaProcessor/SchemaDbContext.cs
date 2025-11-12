using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace SchemaProcessor;

public class SchemaDbContext : DbContext
{
    // Add a parameterless constructor for EF design-time tools
    public SchemaDbContext() { }

    public SchemaDbContext(DbContextOptions<SchemaDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configuration is now handled by the DesignTimeDbContextFactory for EF tools
        // and by Program.cs for runtime. This method can be left empty or removed
        // if the base constructor is always called with options.
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Dynamically find and register all entity classes from the "Entities" namespace
        var entityTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "SchemaProcessor.Entities");

        foreach (var type in entityTypes)
        {
            modelBuilder.Entity(type);
        }
    }
}
