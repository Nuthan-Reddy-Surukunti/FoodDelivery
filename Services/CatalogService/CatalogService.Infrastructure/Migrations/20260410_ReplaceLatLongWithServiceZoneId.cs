using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceLatLongWithServiceZoneId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing composite index on Latitude and Longitude
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_Latitude_Longitude",
                table: "Restaurants");

            // Drop the Latitude and Longitude columns
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Restaurants");

            // Add the new ServiceZoneId column
            migrationBuilder.AddColumn<string>(
                name: "ServiceZoneId",
                table: "Restaurants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "zone_default");

            // Create index on the new ServiceZoneId column for faster queries
            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_ServiceZoneId",
                table: "Restaurants",
                column: "ServiceZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the ServiceZoneId index
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_ServiceZoneId",
                table: "Restaurants");

            // Drop the ServiceZoneId column
            migrationBuilder.DropColumn(
                name: "ServiceZoneId",
                table: "Restaurants");

            // Re-add the Latitude and Longitude columns with default values
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Restaurants",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Restaurants",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            // Recreate the composite index
            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Latitude_Longitude",
                table: "Restaurants",
                columns: new[] { "Latitude", "Longitude" });
        }
    }
}
