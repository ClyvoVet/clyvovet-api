using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClyvoVet.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TUTOR",
                columns: table => new
                {
                    ID_TUTOR = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NOME = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: false),
                    TELEFONE = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: true),
                    CPF = table.Column<string>(type: "VARCHAR2(14)", maxLength: 14, nullable: false),
                    SENHA = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TUTOR", x => x.ID_TUTOR);
                });

            migrationBuilder.CreateTable(
                name: "PET",
                columns: table => new
                {
                    ID_PET = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_TUTOR = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NOME = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: false),
                    ESPECIE = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: false),
                    RACA = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    DATA_NASC = table.Column<DateTime>(type: "DATE", nullable: true),
                    PESO_KG = table.Column<decimal>(type: "NUMBER(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PET", x => x.ID_PET);
                    table.ForeignKey(
                        name: "FK_PET_TUTOR",
                        column: x => x.ID_TUTOR,
                        principalTable: "TUTOR",
                        principalColumn: "ID_TUTOR");
                });

            migrationBuilder.CreateTable(
                name: "CONSULTA",
                columns: table => new
                {
                    ID_CONSULTA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_PET = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DATA_CONSULTA = table.Column<DateTime>(type: "DATE", nullable: false),
                    VETERINARIO = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                    OBSERVACOES = table.Column<string>(type: "VARCHAR2(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTA", x => x.ID_CONSULTA);
                    table.ForeignKey(
                        name: "FK_CONSULTA_PET",
                        column: x => x.ID_PET,
                        principalTable: "PET",
                        principalColumn: "ID_PET");
                });

            migrationBuilder.CreateTable(
                name: "MEDICACAO",
                columns: table => new
                {
                    ID_MEDICACAO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_PET = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NOME = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: false),
                    DOSE = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    FREQUENCIA = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    DATA_INICIO = table.Column<DateTime>(type: "DATE", nullable: true),
                    DATA_FIM = table.Column<DateTime>(type: "DATE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MEDICACAO", x => x.ID_MEDICACAO);
                    table.ForeignKey(
                        name: "FK_MEDICACAO_PET",
                        column: x => x.ID_PET,
                        principalTable: "PET",
                        principalColumn: "ID_PET");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTA_ID_PET",
                table: "CONSULTA",
                column: "ID_PET");

            migrationBuilder.CreateIndex(
                name: "IX_MEDICACAO_ID_PET",
                table: "MEDICACAO",
                column: "ID_PET");

            migrationBuilder.CreateIndex(
                name: "IX_PET_ID_TUTOR",
                table: "PET",
                column: "ID_TUTOR");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONSULTA");

            migrationBuilder.DropTable(
                name: "MEDICACAO");

            migrationBuilder.DropTable(
                name: "PET");

            migrationBuilder.DropTable(
                name: "TUTOR");
        }
    }
}
