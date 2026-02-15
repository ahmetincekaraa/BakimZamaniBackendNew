using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BakimZamani.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchKeywords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchKeywords",
                table: "services",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_services_Name",
                table: "services",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_services_Name",
                table: "services");

            migrationBuilder.DropColumn(
                name: "SearchKeywords",
                table: "services");
        }
    }
}

