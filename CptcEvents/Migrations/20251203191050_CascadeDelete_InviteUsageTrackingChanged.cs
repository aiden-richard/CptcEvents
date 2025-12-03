using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CptcEvents.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDelete_InviteUsageTrackingChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "GroupInvites");

            migrationBuilder.AddColumn<int>(
                name: "GroupId1",
                table: "GroupMemberships",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_GroupId1",
                table: "GroupMemberships",
                column: "GroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMemberships_Groups_GroupId1",
                table: "GroupMemberships",
                column: "GroupId1",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMemberships_Groups_GroupId1",
                table: "GroupMemberships");

            migrationBuilder.DropIndex(
                name: "IX_GroupMemberships_GroupId1",
                table: "GroupMemberships");

            migrationBuilder.DropColumn(
                name: "GroupId1",
                table: "GroupMemberships");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "GroupInvites",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
