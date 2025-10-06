using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UpSkillDashboard.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using UpSkillDashboard.Models;
using Microsoft.Extensions.Options;
using UpSkillDashboard.Services;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register ApplicationDbContext with DI container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.CommandTimeout(60)));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})

    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddHttpClient();
builder.Services.AddScoped<GmailOTPService>();

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = GoogleDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
//})
//.AddGoogle(options =>
//{
//    IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
//    options.ClientId = googleAuthNSection["ClientId"];
//    options.ClientSecret = googleAuthNSection["ClientSecret"];
//    options.CallbackPath = "/signin-google";
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var email = "abdoyasser8348@gmail.com";
    var password = "Password123!"; // Must meet password requirements (e.g., uppercase, number, special character)

    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"User creation failed: {errors}");
        }

    }

}

//builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
//{
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequiredLength = 8;
//    options.Password.RequireUppercase = true;
//    options.Password.RequireLowercase = false;
//    options.User.RequireUniqueEmail = true;
//    options.SignIn.RequireConfirmedAccount = false;
//    options.SignIn.RequireConfirmedEmail = false;
//    options.SignIn.RequireConfirmedPhoneNumber = false;
//})
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders();




app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseAuthentication();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

app.Run();
