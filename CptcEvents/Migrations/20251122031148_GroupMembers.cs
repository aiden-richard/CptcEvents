using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CptcEvents.Migrations
{
    /// <inheritdoc />
    public partial class GroupMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Group_GroupId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "ApplicationUserGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Group",
                table: "Group");

            migrationBuilder.RenameTable(
                name: "Group",
                newName: "Groups");

            migrationBuilder.RenameColumn(
                name: "Privacy",
                table: "Groups",
                newName: "PrivacyLevel");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Groups",
                type: "TEXT",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Groups",
                table: "Groups",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GroupMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", maxLength: 50, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMemberships_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_OwnerId",
                table: "Groups",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_GroupId",
                table: "GroupMemberships",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_UserId",
                table: "GroupMemberships",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Groups_GroupId",
                table: "Events",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_AspNetUsers_OwnerId",
                table: "Groups",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Groups_GroupId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_AspNetUsers_OwnerId",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "GroupMemberships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Groups",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_OwnerId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Groups");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "Group");

            migrationBuilder.RenameColumn(
                name: "PrivacyLevel",
                table: "Group",
                newName: "Privacy");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Group",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Group",
                table: "Group",
                column: "Id");

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
    }
}
