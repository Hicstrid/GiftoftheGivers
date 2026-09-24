using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Part1.Data;
using System.IO;
var builder = WebApplication.CreateBuilder(args);

var expectedWebRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(expectedWebRoot))
{
    Directory.CreateDirectory(expectedWebRoot);
    builder.Environment.WebRootPath = expectedWebRoot;
}
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); //enable Razor pages in addition to MVC controllers/views
builder.Services.AddDbContext<Part1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Part1Context")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
//Map both controllers and Razor Pages so the app runs regardless of page type.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

