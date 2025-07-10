using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddLayoutFieldsToCinemaRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DefaultSeatPrice",
                table: "tbl_cinema_rooms",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfColumns",
                table: "tbl_cinema_rooms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRows",
                table: "tbl_cinema_rooms",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultSeatPrice",
                table: "tbl_cinema_rooms");

            migrationBuilder.DropColumn(
                name: "NumberOfColumns",
                table: "tbl_cinema_rooms");

            migrationBuilder.DropColumn(
                name: "NumberOfRows",
                table: "tbl_cinema_rooms");
        }
    }
}
