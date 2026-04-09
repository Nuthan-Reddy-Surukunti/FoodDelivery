using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryAgentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryAgents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryAgents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAgents_AuthUserId",
                table: "DeliveryAgents",
                column: "AuthUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAgents_IsActive_IsEmailVerified",
                table: "DeliveryAgents",
                columns: new[] { "IsActive", "IsEmailVerified" });

            // Make DeliveryAgentId nullable on existing DeliveryAssignments, clear any values
            // that would violate the foreign key constraint, then add the FK.
            migrationBuilder.AlterColumn<Guid>(
                name: "DeliveryAgentId",
                table: "DeliveryAssignments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            // Clear any existing DeliveryAgentId values that do not have a matching DeliveryAgents row.
            migrationBuilder.Sql("UPDATE [DeliveryAssignments] SET [DeliveryAgentId] = NULL WHERE [DeliveryAgentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryAssignments_DeliveryAgents_DeliveryAgentId",
                table: "DeliveryAssignments",
                column: "DeliveryAgentId",
                principalTable: "DeliveryAgents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryAssignments_DeliveryAgents_DeliveryAgentId",
                table: "DeliveryAssignments");

            migrationBuilder.DropTable(
                name: "DeliveryAgents");
        }
    }
}
