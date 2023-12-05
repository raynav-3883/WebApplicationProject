using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplicationProject.Areas.Identity.Data;



var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("WebApplicationProjectContextConnection") ?? throw new InvalidOperationException("Connection string 'WebApplicationProjectContextConnection' not found.");

builder.Services.AddDbContext<WebApplicationProjectContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ProjectUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<WebApplicationProjectContext>();


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
   
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
