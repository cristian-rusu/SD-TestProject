using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Company.Inventory.Infrastructure.Persistence.EntityFramework.Migrations;

[DbContext(typeof(InventoryDbContext))]
[Migration("202609260002_InitialInventory")]
internal sealed class InitialInventory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema("inventory");
        migrationBuilder.CreateTable(
            name: "stock_items", schema: "inventory",
            columns: table => new
            {
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                available_quantity = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_stock_items", x => x.product_id));

        migrationBuilder.CreateTable(
            name: "stock_reservations", schema: "inventory",
            columns: table => new
            {
                reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_stock_reservations", x => x.reservation_id);
                table.ForeignKey("FK_stock_reservations_stock_items_product_id", x => x.product_id,
                    principalSchema: "inventory", principalTable: "stock_items", principalColumn: "product_id",
                    onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex("IX_stock_reservations_order_id", "stock_reservations", "order_id", "inventory", unique: true);
        migrationBuilder.CreateIndex("IX_stock_reservations_product_id", "stock_reservations", "product_id", "inventory");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "stock_reservations", schema: "inventory");
        migrationBuilder.DropTable(name: "stock_items", schema: "inventory");
    }
}
