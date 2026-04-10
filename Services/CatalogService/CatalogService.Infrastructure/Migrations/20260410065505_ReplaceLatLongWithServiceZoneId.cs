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
            // Drop the composite index on Latitude and Longitude
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_Latitude_Longitude",
                table: "Restaurants");

            // Drop the old location columns
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
                defaultValue: "default-zone");

            // Create index on the new column
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

            // Re-add the latitude and longitude columns
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

            // Re-create the composite index
            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Latitude_Longitude",
                table: "Restaurants",
                columns: new[] { "Latitude", "Longitude" });
        }
    }
}
