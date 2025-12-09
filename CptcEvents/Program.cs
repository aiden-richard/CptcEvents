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
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IInviteService, InviteService>();
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

app.Run();
