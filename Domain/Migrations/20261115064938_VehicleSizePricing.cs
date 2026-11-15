using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class VehicleSizePricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SizeTier",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "LargePrice",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SmallPrice",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                "UPDATE [Vehicles] SET [SizeTier] = CASE WHEN [Type] IN (0, 5) THEN 0 WHEN [Type] IN (2, 3, 4) THEN 2 ELSE 1 END");
            migrationBuilder.Sql(
                "UPDATE [Services] SET [SmallPrice] = [Price], [LargePrice] = [Price]");
            migrationBuilder.Sql(
                "UPDATE [Bookings] SET [Status] = 1 WHERE [Status] = 2 AND EXISTS (SELECT 1 FROM [Washes] WHERE [Washes].[BookingId] = [Bookings].[Id] AND [Washes].[Status] = 2 AND [Washes].[IsPaid] = 0)");
            migrationBuilder.Sql(
                "UPDATE [Washes] SET [Status] = 1, [CompletedAt] = NULL WHERE [Status] = 2 AND [IsPaid] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SizeTier",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LargePrice",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "SmallPrice",
                table: "Services");
        }
    }
}
