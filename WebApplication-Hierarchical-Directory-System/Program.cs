using Microsoft.EntityFrameworkCore;
using WebApplication_Hierarchical_Directory_System.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DirectoryContext>(options =>
{
    options.UseSqlServer("Data Source=FENRIR-PC\\SQLEXPRESS;Initial Catalog=DirectoryStructure;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=False");
});


builder.Services.AddMvc();
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.MapControllers();

//app.MapControllerRoute(name: "root", pattern: "{controller=Home}/{action=Index}");
//app.MapControllerRoute(name: "subFolder", pattern: "{controller=Home}/{action=GetContent}/{id}");


app.Run();
