using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProyectoTecWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddArgentinaTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "jugadores_argentina",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    alias = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jugadores_argentina", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "leaderboard_argentina",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leaderboard_argentina", x => x.id);
                    table.CheckConstraint("ck_leaderboard_argentina_score", "score >= 0 AND score <= 100000");
                });

            migrationBuilder.CreateTable(
                name: "logros_argentina",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    jugador_id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    fecha_otorgado = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logros_argentina", x => x.id);
                    table.ForeignKey(
                        name: "FK_logros_argentina_jugadores_argentina_jugador_id",
                        column: x => x.jugador_id,
                        principalTable: "jugadores_argentina",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "partidas_argentina",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    jugador_id = table.Column<int>(type: "integer", nullable: false),
                    puntaje = table.Column<int>(type: "integer", nullable: false),
                    duracion_segundos = table.Column<int>(type: "integer", nullable: false),
                    jugado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partidas_argentina", x => x.id);
                    table.CheckConstraint("ck_partidas_argentina_duracion", "duracion_segundos > 0");
                    table.CheckConstraint("ck_partidas_argentina_puntaje", "puntaje >= 0");
                    table.ForeignKey(
                        name: "FK_partidas_argentina_jugadores_argentina_jugador_id",
                        column: x => x.jugador_id,
                        principalTable: "jugadores_argentina",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_jugadores_argentina_alias",
                table: "jugadores_argentina",
                column: "alias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_leaderboard_argentina_score",
                table: "leaderboard_argentina",
                column: "score",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_logros_argentina_jugador_id",
                table: "logros_argentina",
                column: "jugador_id");

            migrationBuilder.CreateIndex(
                name: "ix_partidas_argentina_jugador_id",
                table: "partidas_argentina",
                column: "jugador_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "leaderboard_argentina");

            migrationBuilder.DropTable(
                name: "logros_argentina");

            migrationBuilder.DropTable(
                name: "partidas_argentina");

            migrationBuilder.DropTable(
                name: "jugadores_argentina");
        }
    }
}
