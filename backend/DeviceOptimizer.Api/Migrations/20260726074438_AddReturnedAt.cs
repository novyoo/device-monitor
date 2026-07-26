using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceOptimizer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnedAt",
                table: "Devices",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnedAt",
                table: "Devices");
        }
    }
}
