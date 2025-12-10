using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CptcEvents.Migrations
{
    /// <inheritdoc />
    public partial class AddDenialTrackingToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeniedPublic",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeniedPublic",
                table: "Events");
        }
    }
}
