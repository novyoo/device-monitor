using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceOptimizer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthAveragesAndDecisionRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Avg3BatteryHealthPercent",
                table: "Devices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Avg3CrashCount",
                table: "Devices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Avg3DiskErrorCount",
                table: "Devices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Avg3DiskWearPercent",
                table: "Devices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Avg3SuddenShutdownCount",
                table: "Devices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Avg3TemperatureCelsius",
                table: "Devices",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DecisionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    RecommendedAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActualAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecisionRecords_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DecisionRecords_DeviceId",
                table: "DecisionRecords",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DecisionRecords");

            migrationBuilder.DropColumn(
                name: "Avg3BatteryHealthPercent",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Avg3CrashCount",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Avg3DiskErrorCount",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Avg3DiskWearPercent",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Avg3SuddenShutdownCount",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Avg3TemperatureCelsius",
                table: "Devices");
        }
    }
}
