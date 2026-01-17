using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class ActuallyAddFirstAdminUserIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstAdminUserId",
                table: "Tenants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstAdminUserId",
                table: "Tenants");
        }
    }
}
