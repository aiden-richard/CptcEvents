using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CptcEvents.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstructorCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrivacyLevel = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsApprovedPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsDeniedPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsAllDay = table.Column<bool>(type: "bit", nullable: false),
                    DateOfEvent = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Events_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InvitedUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    InviteCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OneTimeUse = table.Column<bool>(type: "bit", nullable: false),
                    TimesUsed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupInvites_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupInvites_AspNetUsers_InvitedUserId",
                        column: x => x.InvitedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupInvites_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InviteId = table.Column<int>(type: "int", nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMemberships_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupMemberships_GroupInvites_InviteId",
                        column: x => x.InviteId,
                        principalTable: "GroupInvites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupMemberships_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "admin-role-id", "e56eb8c2-de88-407d-bfbc-719558124dd0", "Admin", "ADMIN" },
                    { "staff-role-id", "d2059282-9d0e-4f02-82c0-d1aced0acfaf", "Staff", "STAFF" },
                    { "student-role-id", "1ded1cf6-58a7-49c2-96fb-30f568445303", "Student", "STUDENT" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "ProfilePictureUrl", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "admin-user-id", 0, "d010a14b-4080-42bf-8a25-511a307e1e34", "admin@cptc.edu", true, "Admin", "User", false, null, "ADMIN@CPTC.EDU", "ADMIN@CPTC.EDU", "AQAAAAIAAYagAAAAEATRQ3swF839PpP13gfhiRrosB+1np/lSILrXjk3Sc+z4PmnI8LI1BBer/dPRu8KWw==", null, false, null, "26ec9a03-d72e-4067-8b3d-16e8d44eeaba", false, "admin@cptc.edu" });

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

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatedByUserId",
                table: "Events",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_GroupId",
                table: "Events",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvites_CreatedById",
                table: "GroupInvites",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvites_GroupId",
                table: "GroupInvites",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvites_InviteCode",
                table: "GroupInvites",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvites_InvitedUserId",
                table: "GroupInvites",
                column: "InvitedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_ApplicationUserId",
                table: "GroupMemberships",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_GroupId",
                table: "GroupMemberships",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_InviteId",
                table: "GroupMemberships",
                column: "InviteId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_UserId",
                table: "GroupMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_OwnerId",
                table: "Groups",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "GroupMemberships");

            migrationBuilder.DropTable(
                name: "InstructorCodes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "GroupInvites");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
