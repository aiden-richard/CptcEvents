using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CptcEvents.Migrations
{
    /// <inheritdoc />
    public partial class SeedCptcGroupColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id",
                column: "ConcurrencyStamp",
                value: "1390a2fb-d7b7-445f-9be6-298161c55a5b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "staff-role-id",
                column: "ConcurrencyStamp",
                value: "ff67b34e-e596-4764-a9f0-ce5d07280b85");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "student-role-id",
                column: "ConcurrencyStamp",
                value: "e2fa5184-c221-419a-ba3a-b59bdd7e4199");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6ecc7137-ed2f-4c46-bf28-269d0d78b2c7", "AQAAAAIAAYagAAAAEIU2jyByxYA/kt8RlKhguT0WTLN/wmEJtWm2VINlKCNoBxBf18GVCmsY/Dll8jc7iQ==", "b4ad8bdf-7064-473e-b171-7be54832fd2d" });

            migrationBuilder.UpdateData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 1,
                column: "Color",
                value: "#502a7f");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id",
                column: "ConcurrencyStamp",
                value: "a93c7ebe-b801-4843-9a26-38943f4a45d7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "staff-role-id",
                column: "ConcurrencyStamp",
                value: "fb5c9cd4-010f-4285-84ca-99ddd7e541c2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "student-role-id",
                column: "ConcurrencyStamp",
                value: "c0dc756a-f9f7-4eaa-9525-658db4bae84f");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "50af8439-ef29-4b16-895b-38e55a2009b8", "AQAAAAIAAYagAAAAEFyFKOLx/AJhkWoFwpz60MkKvaleHmtSdqjyppyonMZ3cF4RnsiQiR8tHLnmM8dmcg==", "8131ece5-5ab8-4238-a17d-51c3afc15fa8" });

            migrationBuilder.UpdateData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 1,
                column: "Color",
                value: "#007bff");
        }
    }
}
