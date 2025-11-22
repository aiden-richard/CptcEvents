using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CptcEvents.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Events",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Group",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Privacy = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Group", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserGroup",
                columns: table => new
                {
                    JoinedGroupsId = table.Column<int>(type: "INTEGER", nullable: false),
                    MembersId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserGroup", x => new { x.JoinedGroupsId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserGroup_AspNetUsers_MembersId",
                        column: x => x.MembersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserGroup_Group_JoinedGroupsId",
                        column: x => x.JoinedGroupsId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_GroupId",
                table: "Events",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserGroup_MembersId",
                table: "ApplicationUserGroup",
                column: "MembersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Group_GroupId",
                table: "Events",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Group_GroupId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "ApplicationUserGroup");

            migrationBuilder.DropTable(
                name: "Group");

            migrationBuilder.DropIndex(
                name: "IX_Events_GroupId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Events");
        }
    }
}
