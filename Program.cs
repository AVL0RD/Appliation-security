using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Application_Security_Practical.Models;
using Application_Security_Practical;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ApplicationSecurityPractical.Models;
using ApplicationSecurityPractical.Classes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<MyDbContext>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<MyDbContext>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddDistributedMemoryCache(); //save session in memory (session 1)

builder.Services.Configure<GoogleCaptchaConfig>(builder.Configuration.GetSection("GoogleReCaptcha"));

builder.Services.AddTransient(typeof(GoogleCaptchaService));

builder.Services.AddSession(options => // (session 2)
{
    options.IdleTimeout = TimeSpan.FromSeconds(2);
    options.Cookie.IsEssential  = true;

});

builder.Services.ConfigureApplicationCookie(Config =>
{
    //Config.LoginPath = "/Login";
});

builder.Services.AddDataProtection();

builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.Cookie.Name = "MyCookieAuth";
    options.AccessDeniedPath = "/AccessDenied";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBelongToHRDepartment", policy => policy.RequireClaim("Department", "HR"));
});


builder.Services.Configure<IdentityOptions>(opts =>
{
    opts.Password.RequireDigit = true;
    opts.Password.RequireLowercase = true;
    opts.Password.RequireUppercase = true;
    opts.Password.RequiredLength = 12;
    opts.Password.RequiredUniqueChars = 1;

    //lockout settings 
    opts.Lockout.AllowedForNewUsers = true;
    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    opts.Lockout.MaxFailedAccessAttempts = 3;


});



//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<MyDbContext>()
//    .AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseStatusCodePagesWithRedirects("/errors/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePages(context =>
{
    context.HttpContext.Response.ContentType = "text/plain";

    switch (context.HttpContext.Response.StatusCode)
    {
        case 404:
            context.HttpContext.Response.Redirect("/errors/404");
            break;
        case 403:
            context.HttpContext.Response.Redirect("/errors/403");
            break;
        default:
            context.HttpContext.Response.Redirect("/errors/error");
            break;
    }

    return Task.CompletedTask;

});

app.UseRouting();

app.UseAuthorization();

app.UseAuthentication();

app.UseSession(); // Add session (session 3)

app.MapRazorPages();

app.Run();
