using CptcEvents.Authorization.Handlers;
using CptcEvents.Authorization.Requirements;
using CptcEvents.Authorization;
using CptcEvents.Data;
using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers to work correctly behind our reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | 
        ForwardedHeaders.XForwardedProto | 
        ForwardedHeaders.XForwardedHost;
    
    // Clear KnownProxies and KnownNetworks to trust all proxies.
    // This is safe because the app runs in a private network (Docker/container)
    // that is ONLY accessible through our reverse proxy - there is no direct
    // external access to this application. Network isolation is our security boundary.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    // Suppress the PendingModelChangesWarning to allow migrations with non-deterministic seed data
    options.ConfigureWarnings(w => 
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IInviteService, InviteService>();
builder.Services.AddScoped<IInstructorCodeService, InstructorCodeService>();
builder.Services.AddScoped<IGroupAuthorizationService, GroupAuthorizationService>();

// Add authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, GroupMemberHandler>();
builder.Services.AddScoped<IAuthorizationHandler, GroupModeratorHandler>();
builder.Services.AddScoped<IAuthorizationHandler, GroupOwnerHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GroupMember", policy =>
        policy.Requirements.Add(new GroupMemberRequirement(groupIdRoute: "groupId"))
    );
    options.AddPolicy("GroupModerator", policy =>
        policy.Requirements.Add(new GroupModeratorRequirement(groupIdRoute: "groupId"))
    );
    options.AddPolicy("GroupOwner", policy =>
        policy.Requirements.Add(new GroupOwnerRequirement(groupIdRoute: "groupId"))
    );
});


// Wire up SendGrid email sender for Identity confirmation emails
// SendGrid is currently disabled due to free trial ending
// builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, SendGridEmailSender>();

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Seed DB with admin account and many default events generated from CPTC academic calendar
try
{
    await SeedDB(app);
}
catch (Exception ex)
{
    var logger = app.Services.GetService<ILogger<Program>>();
    if (logger != null)
    {
        logger.LogError(ex, "An error occurred while seeding the database during application startup.");
    }
    else
    {
        Console.Error.WriteLine($"An error occurred while seeding the database during application startup: {ex}");
    }
    throw; // Re-throw to prevent app from starting in a bad state
}

app.Run();

async Task SeedDB(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    // Apply pending migrations
    await context.Database.MigrateAsync();

    // Get admin user configuration from appsettings
    var adminEmail = configuration.GetValue<string>("AdminUser:Email");
    var adminUsername = configuration.GetValue<string>("AdminUser:UserName");
    var adminPassword = configuration.GetValue<string>("AdminUser:Password");
    var adminFirstName = configuration.GetValue<string>("AdminUser:FirstName") ?? "Admin";
    var adminLastName = configuration.GetValue<string>("AdminUser:LastName") ?? "User";

    static bool IsMissingOrPlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }
        // Treat known placeholder values from appsettings as missing to avoid unsafe defaults.
        return string.Equals(value, "Set in secrets", StringComparison.OrdinalIgnoreCase);
    }
    var missingFields = new List<string>();
    if (IsMissingOrPlaceholder(adminEmail))
    {
        missingFields.Add("AdminUser:Email");
    }
    if (IsMissingOrPlaceholder(adminUsername))
    {
        missingFields.Add("AdminUser:UserName");
    }
    if (IsMissingOrPlaceholder(adminPassword))
    {
        missingFields.Add("AdminUser:Password");
    }

    if (missingFields.Count > 0)
    {
        var logger = app.Services.GetService<ILogger<Program>>();
        logger?.LogWarning(
            "Admin user configuration is missing or uses placeholder values for: {MissingFields}. Skipping admin user creation.",
            string.Join(", ", missingFields));
        return;
    }

        // Ensure required roles exist
        var requiredRoles = new[] { "Admin", "Staff", "Student" };
        foreach (var roleName in requiredRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleCreateResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleCreateResult.Succeeded)
            {
                var logger = app.Services.GetService<ILogger<Program>>();
                logger?.LogError("Failed to create {RoleName} role: {Errors}", roleName, string.Join(", ", roleCreateResult.Errors.Select(e => e.Description)));
                throw new InvalidOperationException("Failed to create default roles during startup.");
            }
        }
    }

    // Find or create the admin user
    ApplicationUser? adminUser = await userManager.FindByEmailAsync(adminEmail!);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminUsername,
            NormalizedUserName = adminUsername!.ToUpperInvariant(),
            Email = adminEmail,
            NormalizedEmail = adminEmail!.ToUpperInvariant(),
            EmailConfirmed = true,
            FirstName = adminFirstName,
            LastName = adminLastName
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword!);
        if (!createResult.Succeeded)
        {
            var logger = app.Services.GetService<ILogger<Program>>();
            logger?.LogError("Failed to create seeded admin user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
            throw new InvalidOperationException("Failed to create admin user during startup.");
        }
    }

    // Update admin user properties if they've changed in configuration
    bool userNeedsUpdate = false;
    if (adminUser.FirstName != adminFirstName)
    {
        adminUser.FirstName = adminFirstName;
        userNeedsUpdate = true;
    }
    if (adminUser.LastName != adminLastName)
    {
        adminUser.LastName = adminLastName;
        userNeedsUpdate = true;
    }
    if (adminUser.UserName != adminUsername)
    {
        adminUser.UserName = adminUsername;
        adminUser.NormalizedUserName = adminUsername!.ToUpper();
        userNeedsUpdate = true;
    }

    // Update the password if it doesn't match
    var passwordVerificationResult = userManager.PasswordHasher.VerifyHashedPassword(adminUser, adminUser.PasswordHash!, adminPassword!);
    if (passwordVerificationResult == PasswordVerificationResult.Failed)
    {
        adminUser.PasswordHash = userManager.PasswordHasher.HashPassword(adminUser, adminPassword!);
        userNeedsUpdate = true;
    }

    if (userNeedsUpdate)
    {
        await userManager.UpdateAsync(adminUser);
    }

    // Verify the admin user has the Admin role
    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // Ensure the default group exists
    var systemEventsGroup = await context.Groups.FirstOrDefaultAsync(g => g.Name == "System Events");
    if (systemEventsGroup == null)
    {
        var logger = app.Services.GetService<ILogger<Program>>();
        logger?.LogInformation("Creating default 'System Events' group.");
        
        systemEventsGroup = new Group
        {
            Name = "System Events",
            Description = "Default group for CPTC events and announcements",
            OwnerId = adminUser.Id,
            CreatedAt = DateTime.UtcNow,
            PrivacyLevel = PrivacyLevel.Public,
            Color = "#502a7f"
        };
        context.Groups.Add(systemEventsGroup);
        await context.SaveChangesAsync();

        // Add admin user as owner of the group
        var adminMembership = new GroupMember
        {
            GroupId = systemEventsGroup.Id,
            UserId = adminUser.Id,
            Role = RoleType.Owner,
            JoinedAt = DateTime.UtcNow
        };
        context.GroupMemberships.Add(adminMembership);
        await context.SaveChangesAsync();
    }

    // Define all default events
    var defaultEvents = new[]
    {
        // Summer 2025 Quarter Events
        new Event { Title = "Summer 2025 - Priority Registration", Description = "Priority registration for Summer 2025 quarter", DateOfEvent = new DateOnly(2025, 5, 19), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Summer 2025 - Continuing Student Registration", Description = "Registration period for continuing students (May 20-23)", DateOfEvent = new DateOnly(2025, 5, 20), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Summer 2025 - Open Registration", Description = "Open registration for all admitted students (May 27 - July 2)", DateOfEvent = new DateOnly(2025, 5, 27), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Summer 2025 - Tuition & Fees Deadline", Description = "Deadline for tuition and fees payment", DateOfEvent = new DateOnly(2025, 6, 17), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Summer 2025 - First Day of Quarter", Description = "First day of Summer 2025 quarter", DateOfEvent = new DateOnly(2025, 7, 1), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Summer 2025 - Last Day to Drop (100% Refund)", Description = "Last day to drop with 100% refund", DateOfEvent = new DateOnly(2025, 7, 8), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Summer 2025 - Last Day to Withdraw (50% Refund)", Description = "Last day to withdraw with 50% refund", DateOfEvent = new DateOnly(2025, 7, 29), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Summer 2025 - Last Day to Withdraw (W Grade)", Description = "Last day to withdraw with W grade", DateOfEvent = new DateOnly(2025, 8, 19), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Summer 2025 - Graduation Application Deadline", Description = "Deadline for graduation application", DateOfEvent = new DateOnly(2025, 7, 25), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Summer 2025 - Last Day of Quarter", Description = "Last day of Summer 2025 quarter", DateOfEvent = new DateOnly(2025, 9, 2), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Summer 2025 - Official Grades Posted", Description = "Official grades on transcript (ccLink)", DateOfEvent = new DateOnly(2025, 9, 8), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },

        // Fall 2025 Quarter Events
        new Event { Title = "Fall 2025 - Priority Registration", Description = "Priority registration for Fall 2025 quarter", DateOfEvent = new DateOnly(2025, 5, 19), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Fall 2025 - Continuing Student Registration", Description = "Registration period for continuing students (May 20-23)", DateOfEvent = new DateOnly(2025, 5, 20), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Fall 2025 - Open Registration", Description = "Open registration for all admitted students (May 27 - Sept 30)", DateOfEvent = new DateOnly(2025, 5, 27), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Fall 2025 - Tuition & Fees Deadline", Description = "Deadline for tuition and fees payment", DateOfEvent = new DateOnly(2025, 9, 15), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Fall 2025 - First Day of Quarter", Description = "First day of Fall 2025 quarter", DateOfEvent = new DateOnly(2025, 9, 29), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Fall 2025 - Last Day to Drop (100% Refund)", Description = "Last day to drop with 100% refund", DateOfEvent = new DateOnly(2025, 10, 3), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Fall 2025 - Last Day to Withdraw (50% Refund)", Description = "Last day to withdraw with 50% refund", DateOfEvent = new DateOnly(2025, 10, 28), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Fall 2025 - Last Day to Withdraw (W Grade)", Description = "Last day to withdraw with W grade", DateOfEvent = new DateOnly(2025, 11, 19), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Fall 2025 - Graduation Application Deadline", Description = "Deadline for graduation application", DateOfEvent = new DateOnly(2025, 10, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Fall 2025 - Last Day of Quarter", Description = "Last day of Fall 2025 quarter", DateOfEvent = new DateOnly(2025, 12, 12), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Fall 2025 - Official Grades Posted", Description = "Official grades on transcript (ccLink)", DateOfEvent = new DateOnly(2025, 12, 18), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },

        // Winter 2026 Quarter Events
        new Event { Title = "Winter 2026 - Priority Registration", Description = "Priority registration for Winter 2026 quarter", DateOfEvent = new DateOnly(2025, 11, 17), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Winter 2026 - Continuing Student Registration", Description = "Registration period for continuing students (Nov 18-21)", DateOfEvent = new DateOnly(2025, 11, 18), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Winter 2026 - Open Registration", Description = "Open registration for all admitted students (Nov 24 - Jan 6)", DateOfEvent = new DateOnly(2025, 11, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Winter 2026 - Tuition & Fees Deadline", Description = "Deadline for tuition and fees payment", DateOfEvent = new DateOnly(2025, 12, 18), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Winter 2026 - First Day of Quarter", Description = "First day of Winter 2026 quarter", DateOfEvent = new DateOnly(2026, 1, 5), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Winter 2026 - Last Day to Drop (100% Refund)", Description = "Last day to drop with 100% refund", DateOfEvent = new DateOnly(2026, 1, 9), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Winter 2026 - Last Day to Withdraw (50% Refund)", Description = "Last day to withdraw with 50% refund", DateOfEvent = new DateOnly(2026, 2, 2), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Winter 2026 - Last Day to Withdraw (W Grade)", Description = "Last day to withdraw with W grade", DateOfEvent = new DateOnly(2026, 2, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Winter 2026 - Graduation Application Deadline", Description = "Deadline for graduation application", DateOfEvent = new DateOnly(2026, 1, 30), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Winter 2026 - Last Day of Quarter", Description = "Last day of Winter 2026 quarter", DateOfEvent = new DateOnly(2026, 3, 18), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Winter 2026 - Official Grades Posted", Description = "Official grades on transcript (ccLink)", DateOfEvent = new DateOnly(2026, 3, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },

        // Spring 2026 Quarter Events
        new Event { Title = "Spring 2026 - Priority Registration", Description = "Priority registration for Spring 2026 quarter", DateOfEvent = new DateOnly(2026, 2, 2), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Spring 2026 - Continuing Student Registration", Description = "Registration period for continuing students (Feb 3-6)", DateOfEvent = new DateOnly(2026, 2, 3), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Spring 2026 - Open Registration", Description = "Open registration for all admitted students (Feb 9 - Mar 31)", DateOfEvent = new DateOnly(2026, 2, 9), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Spring 2026 - Tuition & Fees Deadline", Description = "Deadline for tuition and fees payment", DateOfEvent = new DateOnly(2026, 3, 16), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Spring 2026 - First Day of Quarter", Description = "First day of Spring 2026 quarter", DateOfEvent = new DateOnly(2026, 3, 30), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Spring 2026 - Last Day to Drop (100% Refund)", Description = "Last day to drop with 100% refund", DateOfEvent = new DateOnly(2026, 4, 3), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Spring 2026 - Last Day to Withdraw (50% Refund)", Description = "Last day to withdraw with 50% refund", DateOfEvent = new DateOnly(2026, 4, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Spring 2026 - Last Day to Withdraw (W Grade)", Description = "Last day to withdraw with W grade", DateOfEvent = new DateOnly(2026, 5, 18), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Spring 2026 - Graduation Application Deadline", Description = "Deadline for graduation application", DateOfEvent = new DateOnly(2026, 4, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Spring 2026 - Last Day of Quarter", Description = "Last day of Spring 2026 quarter", DateOfEvent = new DateOnly(2026, 6, 9), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Spring 2026 - Official Grades Posted", Description = "Official grades on transcript (ccLink)", DateOfEvent = new DateOnly(2026, 6, 15), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },

        // Financial Aid Deadlines
        new Event { Title = "Summer 2025 - Financial Aid Application Deadline", Description = "Deadline for CPTC Financial Aid application process", DateOfEvent = new DateOnly(2025, 5, 23), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Fall 2025 - Financial Aid Application Deadline", Description = "Deadline for CPTC Financial Aid application process", DateOfEvent = new DateOnly(2025, 6, 27), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Winter 2026 - Financial Aid Application Deadline", Description = "Deadline for CPTC Financial Aid application process", DateOfEvent = new DateOnly(2025, 11, 14), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id },
        new Event { Title = "Spring 2026 - Financial Aid Application Deadline", Description = "Deadline for CPTC Financial Aid application process", DateOfEvent = new DateOnly(2026, 2, 20), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = systemEventsGroup.Id, CreatedByUserId = adminUser.Id }
    };

    // Ensure default CPTC academic calendar events exist
    var eventTitles = defaultEvents.Select(e => e.Title).ToArray();

    var existingEventTitles = await context.Events
        .Where(e => e.GroupId == systemEventsGroup.Id)
        .Select(e => e.Title)
        .ToListAsync();

    if (existingEventTitles.Count < eventTitles.Length)
    {
        var logger = app.Services.GetService<ILogger<Program>>();
        var missingCount = eventTitles.Length - existingEventTitles.Count;
        logger?.LogInformation("Seeding {MissingCount} missing CPTC calendar events.", missingCount);

        // Add only missing events
        var missingEvents = defaultEvents
            .Where(e => !existingEventTitles.Contains(e.Title))
            .ToList();

        if (missingEvents.Count > 0)
        {
            context.Events.AddRange(missingEvents);
            await context.SaveChangesAsync();
        }
    }
}
