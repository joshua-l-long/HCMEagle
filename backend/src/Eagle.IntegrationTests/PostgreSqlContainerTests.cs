using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Eagle.IntegrationTests;

[Trait("Category", "Integration")]
public class PostgreSqlContainerTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgresContainer = null!;

    public async Task InitializeAsync()
    {
        // Create PostgreSQL container
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("eagle_test")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();

        // Start the container
        await _postgresContainer.StartAsync();
    }

    [Fact]
    public async Task Container_Should_Start_Successfully()
    {
        // Arrange
        var connectionString = _postgresContainer.GetConnectionString();

        // Assert
        Assert.NotNull(connectionString);
        Assert.Contains("eagle_test", connectionString);
        Assert.Contains("testuser", connectionString);
    }

    [Fact]
    public async Task Container_Should_Accept_Connections()
    {
        // Arrange
        var connectionString = _postgresContainer.GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Assert
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task Container_Should_Execute_Queries()
    {
        // Arrange
        var connectionString = _postgresContainer.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Act
        await using var command = new NpgsqlCommand("SELECT 1 + 1 AS result", connection);
        var result = await command.ExecuteScalarAsync();

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task Container_Should_Create_Tables()
    {
        // Arrange
        var connectionString = _postgresContainer.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Act - Create a test table
        var createTableSql = @"
            CREATE TABLE test_users (
                id SERIAL PRIMARY KEY,
                username VARCHAR(100) NOT NULL,
                email VARCHAR(255) NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );
        ";
        await using var createCommand = new NpgsqlCommand(createTableSql, connection);
        await createCommand.ExecuteNonQueryAsync();

        // Insert test data
        var insertSql = "INSERT INTO test_users (username, email) VALUES (@username, @email) RETURNING id;";
        await using var insertCommand = new NpgsqlCommand(insertSql, connection);
        insertCommand.Parameters.AddWithValue("username", "testuser");
        insertCommand.Parameters.AddWithValue("email", "test@example.com");
        var userId = await insertCommand.ExecuteScalarAsync();

        // Assert
        Assert.NotNull(userId);
        Assert.True((int)userId > 0);

        // Verify data was inserted
        var selectSql = "SELECT COUNT(*) FROM test_users;";
        await using var selectCommand = new NpgsqlCommand(selectSql, connection);
        var count = await selectCommand.ExecuteScalarAsync();
        Assert.Equal(1L, count);
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
