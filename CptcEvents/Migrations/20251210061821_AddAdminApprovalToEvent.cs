using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CptcEvents.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminApprovalToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApprovedPublic",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApprovedPublic",
                table: "Events");
        }
    }
}
