using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BakimZamani.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagesAndCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmenitiesJson",
                table: "salons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "salons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "salons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "salons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxCustomersPerHour",
                table: "salons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SlotDurationMinutes",
                table: "salons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "salons",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "service_packages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    SalonId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_packages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_packages_salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "package_services",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    PackageId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    ServiceId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_package_services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_package_services_service_packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "service_packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_package_services_services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_package_services_PackageId_ServiceId",
                table: "package_services",
                columns: new[] { "PackageId", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_package_services_ServiceId",
                table: "package_services",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_service_packages_IsActive",
                table: "service_packages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_service_packages_SalonId",
                table: "service_packages",
                column: "SalonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "package_services");

            migrationBuilder.DropTable(
                name: "service_packages");

            migrationBuilder.DropColumn(
                name: "AmenitiesJson",
                table: "salons");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "salons");

            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "salons");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "salons");

            migrationBuilder.DropColumn(
                name: "MaxCustomersPerHour",
                table: "salons");

            migrationBuilder.DropColumn(
                name: "SlotDurationMinutes",
                table: "salons");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "salons");
        }
    }
}

