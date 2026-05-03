using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminMenuItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent guard: earlier migrations already create MenuItems in some histories.
            // This prevents AdminService startup from failing on fresh local environments.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[MenuItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MenuItems] (
        [Id] uniqueidentifier NOT NULL,
        [RestaurantId] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [IsVeg] bit NOT NULL,
        [AvailabilityStatus] nvarchar(50) NOT NULL,
        [CategoryName] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [LastSyncedAt] datetime2 NULL,
        [SyncEventId] uniqueidentifier NULL,
        CONSTRAINT [PK_MenuItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MenuItems_Restaurants_RestaurantId] FOREIGN KEY ([RestaurantId]) REFERENCES [dbo].[Restaurants] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_MenuItems_RestaurantId] ON [dbo].[MenuItems] ([RestaurantId]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[MenuItems]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[MenuItems];
END
");
        }
    }
}
