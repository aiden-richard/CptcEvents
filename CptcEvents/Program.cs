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
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseForwardedHeaders();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseForwardedHeaders();
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

// Create default roles if they don't exist
try
{
    await CreateRolesAsync(app);
}
catch (Exception ex)
{
    var logger = app.Services.GetService<ILogger<Program>>();
    if (logger != null)
    {
        logger.LogError(ex, "An error occurred while creating default roles during application startup.");
    }
    else
    {
        Console.Error.WriteLine($"An error occurred while creating default roles during application startup: {ex}");
    }
    throw; // Re-throw to prevent app from starting in a bad state
}

app.Run();

async Task CreateRolesAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    // Apply pending migrations
    await context.Database.MigrateAsync();

    // Get admin user configuration from appsettings
    var adminEmail = configuration.GetValue<string>("AdminUser:Email") ?? "admin@cptc.edu";
    var adminUsername = configuration.GetValue<string>("AdminUser:UserName") ?? adminEmail;
    var adminPassword = configuration.GetValue<string>("AdminUser:Password") ?? "Admin123!";
    var adminFirstName = configuration.GetValue<string>("AdminUser:FirstName") ?? "Admin";
    var adminLastName = configuration.GetValue<string>("AdminUser:LastName") ?? "User";

    // Find or create the admin user
    ApplicationUser? adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        // If the seeded admin user doesn't exist, log an error
        var logger = app.Services.GetService<ILogger<Program>>();
        logger?.LogError("Seeded admin user not found in database. Database seeding may have failed.");
        return;
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
        adminUser.NormalizedUserName = adminUsername.ToUpper();
        userNeedsUpdate = true;
    }

    // Update the password if it doesn't match
    var passwordVerificationResult = userManager.PasswordHasher.VerifyHashedPassword(adminUser, adminUser.PasswordHash!, adminPassword);
    if (passwordVerificationResult == PasswordVerificationResult.Failed)
    {
        adminUser.PasswordHash = userManager.PasswordHasher.HashPassword(adminUser, adminPassword);
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

    // Verify the default group exists
    var cptcDatesGroup = await context.Groups.FirstOrDefaultAsync(g => g.Name == "Cptc Dates");
    if (cptcDatesGroup == null)
    {
        var logger = app.Services.GetService<ILogger<Program>>();
        logger?.LogError("Default 'Cptc Dates' group not found in database. Database seeding may have failed.");
    }
}
