using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClyvoVet.API.Infrastructure.Data.Migrations
{
    public partial class AutenticacaoTutor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SENHA",
                table: "TUTOR",
                type: "VARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropTable(
                name: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Password = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Phone = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.DropColumn(
                name: "SENHA",
                table: "TUTOR");
        }
    }
}
