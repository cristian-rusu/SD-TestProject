using Company.Catalog.Domain.Products;
using Company.Catalog.Infrastructure;
using Company.Inventory.Infrastructure;
using Company.Ordering.Infrastructure;
using Company.Catalog.Infrastructure.Persistence.EntityFramework.Configuration;
using Company.Inventory.Infrastructure.Persistence.EntityFramework.Configuration;
using Company.Ordering.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Architecture;

public sealed class PersistenceModelTests
{
    private const string Connection = "Host=localhost;Database=model_validation;Username=postgres;Password=postgres";

    [Fact]
    public void AllCommandAndQueryModelsKeepTablesAndRelationshipsInsideTheirModule()
    {
        using CatalogDbContext catalog = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(Connection).Options);
        using InventoryDbContext inventory = new InventoryDbContext(new DbContextOptionsBuilder<InventoryDbContext>().UseNpgsql(Connection).Options);
        using OrderingDbContext ordering = new OrderingDbContext(new DbContextOptionsBuilder<OrderingDbContext>().UseNpgsql(Connection).Options);
        using PostgresCatalogQueryDbContext catalogQueries = new PostgresCatalogQueryDbContext(new DbContextOptionsBuilder<PostgresCatalogQueryDbContext>().UseNpgsql(Connection).Options);
        using PostgresInventoryQueryDbContext inventoryQueries = new PostgresInventoryQueryDbContext(new DbContextOptionsBuilder<PostgresInventoryQueryDbContext>().UseNpgsql(Connection).Options);
        using PostgresOrderingQueryDbContext orderingQueries = new PostgresOrderingQueryDbContext(new DbContextOptionsBuilder<PostgresOrderingQueryDbContext>().UseNpgsql(Connection).Options);

        (DbContext Context, string Schema)[] models =
        [
            (catalog, "catalog"), (inventory, "inventory"), (ordering, "ordering"),
            (catalogQueries, "catalog"), (inventoryQueries, "inventory"), (orderingQueries, "ordering")
        ];
        foreach ((DbContext context, string schema) in models)
        {
            Assert.All(context.Model.GetEntityTypes(), entity =>
            {
                Assert.Equal(schema, entity.GetSchema());
                Assert.Equal(context.GetType().Assembly.GetName().Name!.Split('.')[1], entity.ClrType.Namespace!.Split('.')[1]);
                Assert.All(entity.GetForeignKeys(), foreignKey => Assert.Equal(schema, foreignKey.PrincipalEntityType.GetSchema()));
            });
        }
    }

    [Fact]
    public void CatalogModelOwnsOnlyCatalogSchemaAndHasMigration()
    {
        using CatalogDbContext db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(Connection).Options);
        Assert.All(db.Model.GetEntityTypes(), type => Assert.Equal("catalog", type.GetSchema()));
        Assert.Equal("products", db.Model.FindEntityType(typeof(Product))!.GetTableName());
        Assert.Single(db.Database.GetMigrations());
    }

    [Fact]
    public void InventoryModelOwnsOnlyInventorySchemaAndHasMigration()
    {
        using InventoryDbContext db = new InventoryDbContext(new DbContextOptionsBuilder<InventoryDbContext>().UseNpgsql(Connection).Options);
        Assert.All(db.Model.GetEntityTypes(), type => Assert.Equal("inventory", type.GetSchema()));
        Assert.Equal(2, db.Model.GetEntityTypes().Count());
        Assert.Single(db.Database.GetMigrations());
    }

    [Fact]
    public void OrderingModelOwnsOnlyOrderingSchemaAndHasMigration()
    {
        using OrderingDbContext db = new OrderingDbContext(new DbContextOptionsBuilder<OrderingDbContext>().UseNpgsql(Connection).Options);
        Assert.All(db.Model.GetEntityTypes(), type => Assert.Equal("ordering", type.GetSchema()));
        Assert.Equal(2, db.Model.GetEntityTypes().Count());
        Assert.Single(db.Database.GetMigrations());
    }
}
