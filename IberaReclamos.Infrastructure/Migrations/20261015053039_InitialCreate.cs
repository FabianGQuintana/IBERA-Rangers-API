using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IberaReclamos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportCategories",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    SeverityLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportCategories", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    RolName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreateBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tourists",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Evaluation = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Portal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VisitDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ClientCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PhotoKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tourists", x => x.ID);
                    table.CheckConstraint("CK_Tourist_Evaluation", "\"Evaluation\" BETWEEN 1 AND 5");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Dni = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Portal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IdRol = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                    table.CheckConstraint("CK_User_Dni_Format", "\"Dni\" ~ '^[0-9]{7,8}$'");
                    table.ForeignKey(
                        name: "FK_Users_Roles_IdRol",
                        column: x => x.IdRol,
                        principalTable: "Roles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CaptureDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SyncDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusReport = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    PhotoKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReportCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ID);
                    table.CheckConstraint("CK_Report_Latitude", "\"Latitude\" BETWEEN -90 AND 90");
                    table.CheckConstraint("CK_Report_Longitude", "\"Longitude\" BETWEEN -180 AND 180");
                    table.ForeignKey(
                        name: "FK_Reports_ReportCategories_ReportCategoryId",
                        column: x => x.ReportCategoryId,
                        principalTable: "ReportCategories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_Users_ReportedById",
                        column: x => x.ReportedById,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StatusTask = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedById = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tasks_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ReportCategories",
                columns: new[] { "ID", "CreateAt", "CreateBy", "DeleteAt", "Description", "IsDeleted", "LastModifiedAt", "LastModifiedBy", "Name", "SeverityLevel", "Status" },
                values: new object[,]
                {
                    { new Guid("8d1f2c3a-0002-4a6e-9b1a-000000000001"), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Seed", null, "Líquidos contaminantes que escurren de residuos.", false, null, null, "Lixiviados", "High", "Active" },
                    { new Guid("8d1f2c3a-0002-4a6e-9b1a-000000000002"), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Seed", null, "Basura o plásticos abandonados en el área protegida.", false, null, null, "Plásticos y residuos", "Moderate", "Active" },
                    { new Guid("8d1f2c3a-0002-4a6e-9b1a-000000000003"), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Seed", null, "Foco de incendio o quema activa.", false, null, null, "Incendio", "Emergency", "Active" },
                    { new Guid("8d1f2c3a-0002-4a6e-9b1a-000000000004"), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Seed", null, "Indicios o presencia de caza/pesca ilegal.", false, null, null, "Caza o pesca furtiva", "Critical", "Active" },
                    { new Guid("8d1f2c3a-0002-4a6e-9b1a-000000000005"), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Seed", null, "Animal herido, enfermo o muerto.", false, null, null, "Fauna herida o muerta", "High", "Active" },
                    { new Guid("8d1f2c3a-0002-4a6e-9b1a-000000000006"), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Seed", null, "Senderos, carteles o pasarelas en mal estado.", false, null, null, "Infraestructura dañada", "Low", "Active" },
                    { new Guid("8d1f2c3a-0002-4a6e-9b1a-000000000007"), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Seed", null, "Incidente que no encaja en las demás categorías.", false, null, null, "Otro", "Low", "Active" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "ID", "CreateAt", "CreateBy", "DeleteAt", "Description", "IsDeleted", "LastModifiedAt", "LastModifiedBy", "RolName", "Status" },
                values: new object[,]
                {
                    { new Guid("8d1f2c3a-0001-4a6e-9b1a-000000000001"), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Seed", null, "Administrador del sistema", false, null, null, "Admin", "Active" },
                    { new Guid("8d1f2c3a-0001-4a6e-9b1a-000000000002"), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Seed", null, "Guardaparque que reporta incidentes en el terreno", false, null, null, "Ranger", "Active" },
                    { new Guid("8d1f2c3a-0001-4a6e-9b1a-000000000003"), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Seed", null, "Personal de apoyo", false, null, null, "Helper", "Active" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportCategories_Name",
                table: "ReportCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CaptureDate",
                table: "Reports",
                column: "CaptureDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ClientId",
                table: "Reports",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportCategoryId",
                table: "Reports",
                column: "ReportCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportedById",
                table: "Reports",
                column: "ReportedById");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StatusReport",
                table: "Reports",
                column: "StatusReport");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RolName",
                table: "Roles",
                column: "RolName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedById",
                table: "Tasks",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToId",
                table: "Tasks",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ReportId",
                table: "Tasks",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StatusTask",
                table: "Tasks",
                column: "StatusTask");

            migrationBuilder.CreateIndex(
                name: "IX_Tourists_ClientCode",
                table: "Tourists",
                column: "ClientCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tourists_Kind",
                table: "Tourists",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_Tourists_Portal",
                table: "Tourists",
                column: "Portal");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Dni",
                table: "Users",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdRol",
                table: "Users",
                column: "IdRol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Tourists");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "ReportCategories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
