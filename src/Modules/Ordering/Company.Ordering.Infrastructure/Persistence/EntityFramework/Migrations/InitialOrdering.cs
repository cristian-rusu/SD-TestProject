using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Company.Ordering.Infrastructure.Persistence.EntityFramework.Migrations;

[DbContext(typeof(OrderingDbContext))]
[Migration("202609260003_InitialOrdering")]
internal sealed class InitialOrdering : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema("ordering");
        migrationBuilder.CreateTable(
            name: "orders", schema: "ordering",
            columns: table => new
            {
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_orders", x => x.order_id));

        migrationBuilder.CreateTable(
            name: "order_lines", schema: "ordering",
            columns: table => new
            {
                order_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                accepted_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_order_lines", x => x.order_line_id);
                table.ForeignKey("FK_order_lines_orders_order_id", x => x.order_id,
                    principalSchema: "ordering", principalTable: "orders", principalColumn: "order_id",
                    onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex("IX_order_lines_order_id", "order_lines", "order_id", "ordering");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "order_lines", schema: "ordering");
        migrationBuilder.DropTable(name: "orders", schema: "ordering");
    }
}
