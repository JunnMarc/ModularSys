using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularSys.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCancellationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "SalesOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "SalesOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledBy",
                table: "SalesOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledBy",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "PurchaseOrders");
        }
    }
}
