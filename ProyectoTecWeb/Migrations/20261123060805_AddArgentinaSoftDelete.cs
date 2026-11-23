using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoTecWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddArgentinaSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "partidas_argentina",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "partidas_argentina",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "logros_argentina",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "logros_argentina",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "leaderboard_argentina",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "leaderboard_argentina",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "jugadores_argentina",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "jugadores_argentina",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "partidas_argentina");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "partidas_argentina");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "logros_argentina");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "logros_argentina");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "leaderboard_argentina");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "leaderboard_argentina");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "jugadores_argentina");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "jugadores_argentina");
        }
    }
}
