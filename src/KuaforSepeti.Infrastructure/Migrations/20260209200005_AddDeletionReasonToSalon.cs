using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KuaforSepeti.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletionReasonToSalon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletionReason",
                table: "salons",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletionReason",
                table: "salons");
        }
    }
}
