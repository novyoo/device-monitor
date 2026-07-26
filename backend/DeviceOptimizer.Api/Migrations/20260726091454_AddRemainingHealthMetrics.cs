using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceOptimizer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingHealthMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSlowToUpdate",
                table: "Devices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastDaysSinceOsUpdate",
                table: "Devices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastDiskErrorCount",
                table: "Devices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastRamUsagePercent",
                table: "Devices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastSuddenShutdownCount",
                table: "Devices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RamUsageBaselinePercent",
                table: "Devices",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DaysSinceOsUpdate",
                table: "CheckIns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiskErrorCount",
                table: "CheckIns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RamUsagePercent",
                table: "CheckIns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SuddenShutdownCount",
                table: "CheckIns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSlowToUpdate",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastDaysSinceOsUpdate",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastDiskErrorCount",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastRamUsagePercent",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastSuddenShutdownCount",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RamUsageBaselinePercent",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DaysSinceOsUpdate",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "DiskErrorCount",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "RamUsagePercent",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "SuddenShutdownCount",
                table: "CheckIns");
        }
    }
}
