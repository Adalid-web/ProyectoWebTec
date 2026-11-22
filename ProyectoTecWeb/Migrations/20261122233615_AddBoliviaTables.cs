using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProyectoTecWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddBoliviaTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "jugadores_bolivia",
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
                    table.PrimaryKey("PK_jugadores_bolivia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "logros_bolivia",
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
                    table.PrimaryKey("PK_logros_bolivia", x => x.id);
                    table.ForeignKey(
                        name: "FK_logros_bolivia_jugadores_bolivia_jugador_id",
                        column: x => x.jugador_id,
                        principalTable: "jugadores_bolivia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "partidas_bolivia",
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
                    table.PrimaryKey("PK_partidas_bolivia", x => x.id);
                    table.CheckConstraint("ck_partidas_bolivia_duracion", "duracion_segundos > 0");
                    table.CheckConstraint("ck_partidas_bolivia_puntaje", "puntaje >= 0");
                    table.ForeignKey(
                        name: "FK_partidas_bolivia_jugadores_bolivia_jugador_id",
                        column: x => x.jugador_id,
                        principalTable: "jugadores_bolivia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_jugadores_bolivia_alias",
                table: "jugadores_bolivia",
                column: "alias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_logros_bolivia_jugador_id",
                table: "logros_bolivia",
                column: "jugador_id");

            migrationBuilder.CreateIndex(
                name: "ix_partidas_bolivia_jugador_id",
                table: "partidas_bolivia",
                column: "jugador_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "logros_bolivia");

            migrationBuilder.DropTable(
                name: "partidas_bolivia");

            migrationBuilder.DropTable(
                name: "jugadores_bolivia");
        }
    }
}
