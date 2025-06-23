using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add SQL Server Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    // options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServerConnection"))
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLLiteConnection"))
);

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.InitializeAsync(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TotemPWA API v1"));

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "products_slug",
    pattern: "TelaProduto/{categorySlug?}/{subcategorySlug?}",
    defaults: new { controller = "Home", action = "TelaProduto" });


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();