using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CptcEvents.Migrations
{
    /// <inheritdoc />
    public partial class AddEventBannerImageAndExpandDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "staff-role-id");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "student-role-id");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "admin-role-id", "admin-user-id" });

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "GroupMemberships",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id");

            migrationBuilder.DeleteData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Events",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerImageUrl",
                table: "Events",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerImageUrl",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Events",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 10000,
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "admin-role-id", "6c4dd18c-9346-454b-be3b-eb079e89b0a6", "Admin", "ADMIN" },
                    { "staff-role-id", "ad46fdc8-be6e-4a26-b637-0dd6578e7984", "Staff", "STAFF" },
                    { "student-role-id", "20274bea-9795-4c65-a591-7f99a9784856", "Student", "STUDENT" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "ProfilePictureUrl", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "admin-user-id", 0, "18b10a7e-f077-4ae7-9cbd-0f2ba31ca4dd", "admin@cptc.edu", true, "Admin", "User", false, null, "ADMIN@CPTC.EDU", "ADMIN@CPTC.EDU", "AQAAAAIAAYagAAAAEKUohHSj6nzqDqpT3TkPPzfJaP9LuIQt553VdEvlQXwMoj9TV8+gsE3WtXVRxl1otg==", null, false, null, "ccecebf8-9ebe-47fb-860d-6f9c2f10e926", false, "admin@cptc.edu" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "admin-role-id", "admin-user-id" });

            migrationBuilder.InsertData(
                table: "Groups",
                columns: new[] { "Id", "Color", "CreatedAt", "Description", "Name", "OwnerId", "PrivacyLevel" },
                values: new object[] { 1, "#502a7f", new DateTime(2025, 12, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Default group for CPTC events and announcements", "Cptc Dates", "admin-user-id", 0 });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "CreatedByUserId", "DateOfEvent", "Description", "EndTime", "GroupId", "IsAllDay", "IsApprovedPublic", "IsDeniedPublic", "IsPublic", "StartTime", "Title", "Url" },
                values: new object[,]
                {
                    { 1, "admin-user-id", new DateOnly(2025, 5, 19), "Priority registration for Summer 2025 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - Priority Registration", null },
                    { 2, "admin-user-id", new DateOnly(2025, 5, 20), "Registration period for continuing students (May 20-23)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - Continuing Student Registration", null },
                    { 3, "admin-user-id", new DateOnly(2025, 5, 27), "Open registration for all admitted students (May 27 - July 2)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - Open Registration", null },
                    { 4, "admin-user-id", new DateOnly(2025, 6, 17), "Deadline for tuition and fees payment", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - Tuition & Fees Deadline", null },
                    { 5, "admin-user-id", new DateOnly(2025, 7, 1), "First day of Summer 2025 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - First Day of Quarter", null },
                    { 6, "admin-user-id", new DateOnly(2025, 7, 8), "Last day to drop with 100% refund", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - Last Day to Drop (100% Refund)", null },
                    { 7, "admin-user-id", new DateOnly(2025, 7, 29), "Last day to withdraw with 50% refund", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - Last Day to Withdraw (50% Refund)", null },
                    { 8, "admin-user-id", new DateOnly(2025, 8, 19), "Last day to withdraw with W grade", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - Last Day to Withdraw (W Grade)", null },
                    { 9, "admin-user-id", new DateOnly(2025, 7, 25), "Deadline for graduation application", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - Graduation Application Deadline", null },
                    { 10, "admin-user-id", new DateOnly(2025, 9, 2), "Last day of Summer 2025 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - Last Day of Quarter", null },
                    { 11, "admin-user-id", new DateOnly(2025, 9, 8), "Official grades on transcript (ccLink)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - Official Grades Posted", null },
                    { 12, "admin-user-id", new DateOnly(2025, 5, 19), "Priority registration for Fall 2025 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - Priority Registration", null },
                    { 13, "admin-user-id", new DateOnly(2025, 5, 20), "Registration period for continuing students (May 20-23)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - Continuing Student Registration", null },
                    { 14, "admin-user-id", new DateOnly(2025, 5, 27), "Open registration for all admitted students (May 27 - Sept 30)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - Open Registration", null },
                    { 15, "admin-user-id", new DateOnly(2025, 9, 15), "Deadline for tuition and fees payment", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - Tuition & Fees Deadline", null },
                    { 16, "admin-user-id", new DateOnly(2025, 9, 29), "First day of Fall 2025 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - First Day of Quarter", null },
                    { 17, "admin-user-id", new DateOnly(2025, 10, 3), "Last day to drop with 100% refund", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - Last Day to Drop (100% Refund)", null },
                    { 18, "admin-user-id", new DateOnly(2025, 10, 28), "Last day to withdraw with 50% refund", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - Last Day to Withdraw (50% Refund)", null },
                    { 19, "admin-user-id", new DateOnly(2025, 11, 19), "Last day to withdraw with W grade", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - Last Day to Withdraw (W Grade)", null },
                    { 20, "admin-user-id", new DateOnly(2025, 10, 24), "Deadline for graduation application", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - Graduation Application Deadline", null },
                    { 21, "admin-user-id", new DateOnly(2025, 12, 12), "Last day of Fall 2025 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - Last Day of Quarter", null },
                    { 22, "admin-user-id", new DateOnly(2025, 12, 18), "Official grades on transcript (ccLink)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - Official Grades Posted", null },
                    { 23, "admin-user-id", new DateOnly(2025, 11, 17), "Priority registration for Winter 2026 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - Priority Registration", null },
                    { 24, "admin-user-id", new DateOnly(2025, 11, 18), "Registration period for continuing students (Nov 18-21)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - Continuing Student Registration", null },
                    { 25, "admin-user-id", new DateOnly(2025, 11, 24), "Open registration for all admitted students (Nov 24 - Jan 6)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - Open Registration", null },
                    { 26, "admin-user-id", new DateOnly(2025, 12, 18), "Deadline for tuition and fees payment", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - Tuition & Fees Deadline", null },
                    { 27, "admin-user-id", new DateOnly(2026, 1, 5), "First day of Winter 2026 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - First Day of Quarter", null },
                    { 28, "admin-user-id", new DateOnly(2026, 1, 9), "Last day to drop with 100% refund", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - Last Day to Drop (100% Refund)", null },
                    { 29, "admin-user-id", new DateOnly(2026, 2, 2), "Last day to withdraw with 50% refund", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - Last Day to Withdraw (50% Refund)", null },
                    { 30, "admin-user-id", new DateOnly(2026, 2, 24), "Last day to withdraw with W grade", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - Last Day to Withdraw (W Grade)", null },
                    { 31, "admin-user-id", new DateOnly(2026, 1, 30), "Deadline for graduation application", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - Graduation Application Deadline", null },
                    { 32, "admin-user-id", new DateOnly(2026, 3, 18), "Last day of Winter 2026 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - Last Day of Quarter", null },
                    { 33, "admin-user-id", new DateOnly(2026, 3, 24), "Official grades on transcript (ccLink)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - Official Grades Posted", null },
                    { 34, "admin-user-id", new DateOnly(2026, 2, 2), "Priority registration for Spring 2026 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - Priority Registration", null },
                    { 35, "admin-user-id", new DateOnly(2026, 2, 3), "Registration period for continuing students (Feb 3-6)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - Continuing Student Registration", null },
                    { 36, "admin-user-id", new DateOnly(2026, 2, 9), "Open registration for all admitted students (Feb 9 - Mar 31)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - Open Registration", null },
                    { 37, "admin-user-id", new DateOnly(2026, 3, 16), "Deadline for tuition and fees payment", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - Tuition & Fees Deadline", null },
                    { 38, "admin-user-id", new DateOnly(2026, 3, 30), "First day of Spring 2026 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - First Day of Quarter", null },
                    { 39, "admin-user-id", new DateOnly(2026, 4, 3), "Last day to drop with 100% refund", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - Last Day to Drop (100% Refund)", null },
                    { 40, "admin-user-id", new DateOnly(2026, 4, 24), "Last day to withdraw with 50% refund", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - Last Day to Withdraw (50% Refund)", null },
                    { 41, "admin-user-id", new DateOnly(2026, 5, 18), "Last day to withdraw with W grade", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - Last Day to Withdraw (W Grade)", null },
                    { 42, "admin-user-id", new DateOnly(2026, 4, 24), "Deadline for graduation application", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - Graduation Application Deadline", null },
                    { 43, "admin-user-id", new DateOnly(2026, 6, 9), "Last day of Spring 2026 quarter", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - Last Day of Quarter", null },
                    { 44, "admin-user-id", new DateOnly(2026, 6, 15), "Official grades on transcript (ccLink)", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - Official Grades Posted", null },
                    { 45, "admin-user-id", new DateOnly(2025, 5, 23), "Deadline for CPTC Financial Aid application process", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Summer 2025 - Financial Aid Application Deadline", null },
                    { 46, "admin-user-id", new DateOnly(2025, 6, 27), "Deadline for CPTC Financial Aid application process", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Fall 2025 - Financial Aid Application Deadline", null },
                    { 47, "admin-user-id", new DateOnly(2025, 11, 14), "Deadline for CPTC Financial Aid application process", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Winter 2026 - Financial Aid Application Deadline", null },
                    { 48, "admin-user-id", new DateOnly(2026, 2, 20), "Deadline for CPTC Financial Aid application process", new TimeOnly(0, 0, 0), 1, true, true, false, true, new TimeOnly(0, 0, 0), "Spring 2026 - Financial Aid Application Deadline", null }
                });

            migrationBuilder.InsertData(
                table: "GroupMemberships",
                columns: new[] { "Id", "ApplicationUserId", "GroupId", "InviteId", "JoinedAt", "Role", "UserId" },
                values: new object[] { 1, null, 1, null, new DateTime(2025, 12, 10, 0, 0, 0, 0, DateTimeKind.Utc), 2, "admin-user-id" });
        }
    }
}
