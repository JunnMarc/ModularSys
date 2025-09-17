using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularSys.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnhancePermissionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Permissions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Permissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemPermission",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                columns: new[] { "Category", "CreatedAt", "CreatedBy", "DisplayOrder", "Icon", "IsSystemPermission" },
                values: new object[] { "General", new DateTime(2025, 9, 17, 17, 43, 39, 336, DateTimeKind.Utc).AddTicks(8274), null, 0, "Lock", false });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                columns: new[] { "Category", "CreatedAt", "CreatedBy", "DisplayOrder", "Icon", "IsSystemPermission" },
                values: new object[] { "General", new DateTime(2025, 9, 17, 17, 43, 39, 336, DateTimeKind.Utc).AddTicks(9165), null, 0, "Lock", false });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                columns: new[] { "Category", "CreatedAt", "CreatedBy", "DisplayOrder", "Icon", "IsSystemPermission" },
                values: new object[] { "General", new DateTime(2025, 9, 17, 17, 43, 39, 336, DateTimeKind.Utc).AddTicks(9168), null, 0, "Lock", false });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                columns: new[] { "Category", "CreatedAt", "CreatedBy", "DisplayOrder", "Icon", "IsSystemPermission" },
                values: new object[] { "General", new DateTime(2025, 9, 17, 17, 43, 39, 336, DateTimeKind.Utc).AddTicks(9169), null, 0, "Lock", false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsSystemPermission",
                table: "Permissions");
        }
    }
}
