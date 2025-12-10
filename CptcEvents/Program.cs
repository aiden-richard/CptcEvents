using CptcEvents.Authorization.Handlers;
using CptcEvents.Authorization.Requirements;
using CptcEvents.Authorization;
using CptcEvents.Data;
using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString);
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
//builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, SendGridEmailSender>();

var app = builder.Build();

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

    // Ensure database is created and migrations are applied
    await context.Database.EnsureCreatedAsync();

    // The admin user, roles, groups, and events are now seeded via EF Core seed data
    // We just need to verify everything is in place

    string adminEmail = "admin@cptc.edu";
    ApplicationUser? adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        // If the seeded admin user doesn't exist, log an error
        var logger = app.Services.GetService<ILogger<Program>>();
        logger?.LogError("Seeded admin user not found in database. Database seeding may have failed.");
        return;
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
