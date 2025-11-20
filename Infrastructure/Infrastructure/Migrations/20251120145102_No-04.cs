using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class No04 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SalaId",
                table: "Asesorias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Salas_Numero",
                table: "Salas",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asesorias_SalaId",
                table: "Asesorias",
                column: "SalaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Asesorias_Salas_SalaId",
                table: "Asesorias",
                column: "SalaId",
                principalTable: "Salas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asesorias_Salas_SalaId",
                table: "Asesorias");

            migrationBuilder.DropIndex(
                name: "IX_Salas_Numero",
                table: "Salas");

            migrationBuilder.DropIndex(
                name: "IX_Asesorias_SalaId",
                table: "Asesorias");

            migrationBuilder.DropColumn(
                name: "SalaId",
                table: "Asesorias");
        }
    }
}
