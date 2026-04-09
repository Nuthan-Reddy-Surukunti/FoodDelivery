using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLastSyncedAtAndSyncEventId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItems");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "SyncEventId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SyncEventId",
                table: "Orders");

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CategoryId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RestaurantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ApprovalStatus",
                table: "MenuItems",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_RestaurantId",
                table: "MenuItems",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_RestaurantId_Name",
                table: "MenuItems",
                columns: new[] { "RestaurantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_Status",
                table: "MenuItems",
                column: "Status");
        }
    }
}
