using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShowTimeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Points",
                table: "tbl_users");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "tbl_show_times",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "tbl_show_times",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "tbl_show_times",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "tbl_show_times",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "tbl_show_times");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "tbl_show_times");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "tbl_show_times");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "tbl_show_times");

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "tbl_users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
