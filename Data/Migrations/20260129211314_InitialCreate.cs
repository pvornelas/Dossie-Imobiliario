using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DossieImobiliario.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessosImobiliarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumeroProcesso = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Cliente = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Imovel = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessosImobiliarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosProcesso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProcessoImobiliarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoDocumento = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    NomeOriginal = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    NomeSalvo = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CaminhoRelativo = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    EnviadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosProcesso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosProcesso_ProcessosImobiliarios_ProcessoImobiliarioId",
                        column: x => x.ProcessoImobiliarioId,
                        principalTable: "ProcessosImobiliarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosProcesso_ProcessoImobiliarioId",
                table: "DocumentosProcesso",
                column: "ProcessoImobiliarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessosImobiliarios_Cliente",
                table: "ProcessosImobiliarios",
                column: "Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessosImobiliarios_NumeroProcesso",
                table: "ProcessosImobiliarios",
                column: "NumeroProcesso",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentosProcesso");

            migrationBuilder.DropTable(
                name: "ProcessosImobiliarios");
        }
    }
}
