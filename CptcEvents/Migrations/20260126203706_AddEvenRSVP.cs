using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CptcEvents.Migrations
{
    /// <inheritdoc />
    public partial class AddEvenRSVP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventRsvps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRsvps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRsvps_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventRsvps_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id",
                column: "ConcurrencyStamp",
                value: "6c4dd18c-9346-454b-be3b-eb079e89b0a6");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "staff-role-id",
                column: "ConcurrencyStamp",
                value: "ad46fdc8-be6e-4a26-b637-0dd6578e7984");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "student-role-id",
                column: "ConcurrencyStamp",
                value: "20274bea-9795-4c65-a591-7f99a9784856");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "18b10a7e-f077-4ae7-9cbd-0f2ba31ca4dd", "AQAAAAIAAYagAAAAEKUohHSj6nzqDqpT3TkPPzfJaP9LuIQt553VdEvlQXwMoj9TV8+gsE3WtXVRxl1otg==", "ccecebf8-9ebe-47fb-860d-6f9c2f10e926" });

            migrationBuilder.CreateIndex(
                name: "IX_EventRsvps_EventId_UserId",
                table: "EventRsvps",
                columns: new[] { "EventId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventRsvps_UserId",
                table: "EventRsvps",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRsvps");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id",
                column: "ConcurrencyStamp",
                value: "e56eb8c2-de88-407d-bfbc-719558124dd0");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "staff-role-id",
                column: "ConcurrencyStamp",
                value: "d2059282-9d0e-4f02-82c0-d1aced0acfaf");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "student-role-id",
                column: "ConcurrencyStamp",
                value: "1ded1cf6-58a7-49c2-96fb-30f568445303");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d010a14b-4080-42bf-8a25-511a307e1e34", "AQAAAAIAAYagAAAAEATRQ3swF839PpP13gfhiRrosB+1np/lSILrXjk3Sc+z4PmnI8LI1BBer/dPRu8KWw==", "26ec9a03-d72e-4067-8b3d-16e8d44eeaba" });
        }
    }
}
