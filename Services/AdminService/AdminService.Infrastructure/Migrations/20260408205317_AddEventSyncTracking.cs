using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventSyncTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add LastSyncedAt column to Orders table
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncEventId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            // Add LastSyncedAt column to Restaurants table
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Restaurants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncEventId",
                table: "Restaurants",
                type: "uniqueidentifier",
                nullable: true);

            // Add LastSyncedAt column to MenuItems table
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "MenuItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncEventId",
                table: "MenuItems",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SyncEventId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "SyncEventId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "SyncEventId",
                table: "MenuItems");
        }
    }
}
