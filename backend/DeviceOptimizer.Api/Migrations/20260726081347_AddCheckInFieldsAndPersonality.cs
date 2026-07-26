using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceOptimizer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInFieldsAndPersonality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "LastActiveUseHours",
                table: "Devices",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastBatteryHealthPercent",
                table: "Devices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCheckInAt",
                table: "Devices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastCrashCount",
                table: "Devices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastDiskWearPercent",
                table: "Devices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastTemperatureCelsius",
                table: "Devices",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Personality",
                table: "Devices",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActiveUseHours",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastBatteryHealthPercent",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastCheckInAt",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastCrashCount",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastDiskWearPercent",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastTemperatureCelsius",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Personality",
                table: "Devices");
        }
    }
}
