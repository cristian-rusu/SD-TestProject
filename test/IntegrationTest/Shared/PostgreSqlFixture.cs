using Testcontainers.PostgreSql;

namespace IntegrationTest.Shared;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("webshop")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    internal WebshopApiFactory Factory { get; private set; } = null!;
    internal string ConnectionString => _database.GetConnectionString();
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        Factory = new WebshopApiFactory(_database.GetConnectionString());
        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (Factory is not null) await Factory.DisposeAsync();
        await _database.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSQL webshop";
}
