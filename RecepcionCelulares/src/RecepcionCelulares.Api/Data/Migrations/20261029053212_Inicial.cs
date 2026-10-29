using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecepcionCelulares.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NegocioId = table.Column<int>(type: "int", nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Dni = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Negocios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Ruc = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    UltimoCorrelativo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Negocios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Repuestos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NegocioId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repuestos", x => x.Id);
                    table.CheckConstraint("CK_Repuesto_Stock", "[Stock] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Ordenes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NegocioId = table.Column<int>(type: "int", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Imei = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Falla = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DeclaracionRiesgoAceptada = table.Column<bool>(type: "bit", nullable: false),
                    PrecioServicio = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Adelanto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordenes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ordenes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NegocioId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Negocios_NegocioId",
                        column: x => x.NegocioId,
                        principalTable: "Negocios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdenRepuestos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenServicioId = table.Column<int>(type: "int", nullable: false),
                    RepuestoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenRepuestos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenRepuestos_Ordenes_OrdenServicioId",
                        column: x => x.OrdenServicioId,
                        principalTable: "Ordenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdenRepuestos_Repuestos_RepuestoId",
                        column: x => x.RepuestoId,
                        principalTable: "Repuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_NegocioId_Dni",
                table: "Clientes",
                columns: new[] { "NegocioId", "Dni" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_ClienteId",
                table: "Ordenes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_NegocioId_Numero",
                table: "Ordenes",
                columns: new[] { "NegocioId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenRepuestos_OrdenServicioId",
                table: "OrdenRepuestos",
                column: "OrdenServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenRepuestos_RepuestoId",
                table: "OrdenRepuestos",
                column: "RepuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NegocioId",
                table: "Usuarios",
                column: "NegocioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdenRepuestos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Ordenes");

            migrationBuilder.DropTable(
                name: "Repuestos");

            migrationBuilder.DropTable(
                name: "Negocios");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
