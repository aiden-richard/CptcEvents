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

// Seed DB with admin account and many system events
// If in the development environment, also seed with test events
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

#region Seed Database

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

    #endregion

    #region Dev Seeding

    if (app.Environment.IsDevelopment())
    {
        // Define test events to seed into the database for testing and demonstration purposes. These events are based on the CPTC academic calendar and are associated with the "System Events" group.
        List<Event> testEvents = DevSeedEvents.Create(systemEventsGroup, adminUser);

        // Ensure default CPTC academic calendar events exist
        var eventTitles = testEvents.Select(e => e.Title).ToArray();

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
            var missingEvents = testEvents
                .Where(e => !existingEventTitles.Contains(e.Title))
                .ToList();

            if (missingEvents.Count > 0)
            {
                context.Events.AddRange(missingEvents);
                await context.SaveChangesAsync();
            }
        }
    }
  
    #endregion
}