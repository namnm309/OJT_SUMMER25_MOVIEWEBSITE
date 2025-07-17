using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_seat_log_tbl_bookings_BookingId",
                table: "tbl_seat_log");

            migrationBuilder.DropIndex(
                name: "IX_tbl_seat_log_BookingId",
                table: "tbl_seat_log");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "tbl_seat_log");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BookingId",
                table: "tbl_seat_log",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_tbl_seat_log_BookingId",
                table: "tbl_seat_log",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_seat_log_tbl_bookings_BookingId",
                table: "tbl_seat_log",
                column: "BookingId",
                principalTable: "tbl_bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
