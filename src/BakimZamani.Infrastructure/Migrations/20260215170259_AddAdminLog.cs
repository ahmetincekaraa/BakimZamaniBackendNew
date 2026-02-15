using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BakimZamani.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "salons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByAdminId",
                table: "salons",
                type: "character varying(26)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedAt",
                table: "salons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspendedByAdminId",
                table: "salons",
                type: "character varying(26)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdminLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AdminId = table.Column<string>(type: "character varying(26)", nullable: false),
                    AdminName = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    TargetEntity = table.Column<string>(type: "text", nullable: false),
                    TargetId = table.Column<string>(type: "text", nullable: true),
                    TargetName = table.Column<string>(type: "text", nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminLogs_users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_salons_ApprovedByAdminId",
                table: "salons",
                column: "ApprovedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_salons_SuspendedByAdminId",
                table: "salons",
                column: "SuspendedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminLogs_AdminId",
                table: "AdminLogs",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_salons_users_ApprovedByAdminId",
                table: "salons",
                column: "ApprovedByAdminId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_salons_users_SuspendedByAdminId",
                table: "salons",
                column: "SuspendedByAdminId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_salons_users_ApprovedByAdminId",
                table: "salons");

            migrationBuilder.DropForeignKey(
                name: "FK_salons_users_SuspendedByAdminId",
                table: "salons");

            migrationBuilder.DropTable(
                name: "AdminLogs");

            migrationBuilder.DropIndex(
                name: "IX_salons_ApprovedByAdminId",
                table: "salons");

            migrationBuilder.DropIndex(
                name: "IX_salons_SuspendedByAdminId",
                table: "salons");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "salons");

            migrationBuilder.DropColumn(
                name: "ApprovedByAdminId",
                table: "salons");

            migrationBuilder.DropColumn(
                name: "SuspendedAt",
                table: "salons");

            migrationBuilder.DropColumn(
                name: "SuspendedByAdminId",
                table: "salons");
        }
    }
}

