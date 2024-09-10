var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();