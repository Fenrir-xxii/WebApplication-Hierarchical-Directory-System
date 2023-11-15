using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApplication_Hierarchical_Directory_System.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DirectoryContext>(options =>
{
    options.UseSqlServer("Data Source=FENRIR-PC\\SQLEXPRESS;Initial Catalog=DirectoryStructure;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=False");
    //options.UseSqlServer("Data Source=WINSRVR2019;Initial Catalog=DirectoryStructure;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=False;Command Timeout=0");
});


builder.Services.AddMvc();
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.MapControllers();

app.UseStaticFiles();

app.Run();
