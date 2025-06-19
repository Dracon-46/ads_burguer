using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Utilities; // <-- Mantenha este using para seu expander

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// CORREÇÃO AQUI: Adiciona Controllers com Views e configura Razor Options para o ViewLocationExpander
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options => // <-- Método correto para ViewLocationExpanders
    {
        options.ViewLocationExpanders.Add(new AdminViewLocationExpander());
    });

// Resto dos serviços...
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLLiteConnection"))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.InitializeAsync(context);
}

// Configura o pipeline de requisições HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TotemPWA API v1"));

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers(); // Mantenha esta linha

// Certifique-se de que a rota 'admin' com '{area:exists}' foi removida
// app.MapControllerRoute(
//     name: "admin",
//     pattern: "{area:exists}/{controller=Home}/{action=Index}"); 

app.MapStaticAssets(); // Mantenha se for seu método de extensão

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets(); // Mantenha se for seu método de extensão

app.Run();