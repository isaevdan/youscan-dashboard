using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashboard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueOrderIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing databases may already contain colliding Order values (created
            // before the unique index existed). Deterministically reassign every
            // colliding row except the lowest-Id one per Order to the next free Order
            // values (max + 1, max + 2, ... in Id order) so the index creation below
            // cannot fail. A temp table is used so the new values are computed from a
            // stable snapshot rather than mid-UPDATE state.
            migrationBuilder.Sql("""
                CREATE TEMP TABLE __widget_order_fix AS
                SELECT w."Id" AS "Id",
                       (SELECT MAX("Order") FROM "Widgets") + ROW_NUMBER() OVER (ORDER BY w."Id") AS "NewOrder"
                FROM "Widgets" AS w
                WHERE EXISTS (
                    SELECT 1 FROM "Widgets" AS w2
                    WHERE w2."Order" = w."Order" AND w2."Id" < w."Id");

                UPDATE "Widgets"
                SET "Order" = (SELECT "NewOrder" FROM __widget_order_fix AS f WHERE f."Id" = "Widgets"."Id")
                WHERE "Id" IN (SELECT "Id" FROM __widget_order_fix);

                DROP TABLE __widget_order_fix;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Widgets_Order",
                table: "Widgets",
                column: "Order",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Widgets_Order",
                table: "Widgets");
        }
    }
}
