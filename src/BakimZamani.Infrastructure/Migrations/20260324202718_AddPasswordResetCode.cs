using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BakimZamani.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetCode",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetCodeExpiry",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetCode",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeExpiry",
                table: "users");
        }
    }
}
