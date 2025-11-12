using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Eagle.IntegrationTests;

[Trait("Category", "Integration")]
public class SchemaProcessorIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgresContainer = null!;
    private string _connectionString = null!;
    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("eagle_schema_test")
            .WithUsername("schemauser")
            .WithPassword("schemapass")
            .WithCleanUp(true)
            .Build();

        await _postgresContainer.StartAsync();
        _connectionString = _postgresContainer.GetConnectionString();
    }

    [Fact]
    public async Task Should_Create_Schema_With_Multiple_Tables()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var schemaSql = @"
            CREATE SCHEMA IF NOT EXISTS app_schema;

            CREATE TABLE app_schema.departments (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100) NOT NULL,
                code VARCHAR(10) UNIQUE NOT NULL
            );

            CREATE TABLE app_schema.employees (
                id SERIAL PRIMARY KEY,
                department_id INTEGER REFERENCES app_schema.departments(id),
                first_name VARCHAR(50) NOT NULL,
                last_name VARCHAR(50) NOT NULL,
                email VARCHAR(255) UNIQUE NOT NULL
            );
        ";
        await using var command = new NpgsqlCommand(schemaSql, connection);
        await command.ExecuteNonQueryAsync();

        var schemaCheckSql = @"
            SELECT COUNT(*)
            FROM information_schema.schemata
            WHERE schema_name = 'app_schema';
        ";
        await using var schemaCheck = new NpgsqlCommand(schemaCheckSql, connection);
        var schemaCount = await schemaCheck.ExecuteScalarAsync();
        Assert.Equal(1L, schemaCount);

        var tableCheckSql = @"
            SELECT COUNT(*)
            FROM information_schema.tables
            WHERE table_schema = 'app_schema';
        ";
        await using var tableCheck = new NpgsqlCommand(tableCheckSql, connection);
        var tableCount = await tableCheck.ExecuteScalarAsync();
        Assert.Equal(2L, tableCount);
    }

    [Fact]
    public async Task Should_Handle_Database_Migrations()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var migrationTableSql = @"
            CREATE TABLE IF NOT EXISTS __migrations (
                id SERIAL PRIMARY KEY,
                version VARCHAR(50) NOT NULL,
                applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );
        ";
        await using var createMigrationTable = new NpgsqlCommand(migrationTableSql, connection);
        await createMigrationTable.ExecuteNonQueryAsync();

        var insertMigrationSql = "INSERT INTO __migrations (version) VALUES (@version);";
        await using var insertMigration = new NpgsqlCommand(insertMigrationSql, connection);
        insertMigration.Parameters.AddWithValue("version", "001_initial_schema");
        await insertMigration.ExecuteNonQueryAsync();

        var countSql = "SELECT COUNT(*) FROM __migrations;";
        await using var countCommand = new NpgsqlCommand(countSql, connection);
        var count = await countCommand.ExecuteScalarAsync();
        Assert.Equal(1L, count);
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
