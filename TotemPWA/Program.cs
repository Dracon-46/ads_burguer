using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Utilities; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Adicionar Autenticação por Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Define o caminho para a página de login.  
        options.LoginPath = "/Admin/Employee/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
    });

// Adicionar Serviço de Autorização e Políticas de Cargos
builder.Services.AddAuthorization(options =>
{
    // Define a política "AdminOnly" para restringir acesso apenas a funcionários com Type "Administrador"
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrador"));
});

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        // Esta linha permite que o ASP.NET Core MVC encontre views em pastas como /Views/Admin/Product/
        // O AdminViewLocationExpander deve estar definido na sua pasta TotemPWA\Utilities
        options.ViewLocationExpanders.Add(new AdminViewLocationExpander());
    });

// Configuração do banco de dados (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLLiteConnection")));

// Configuração da sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de inatividade da sessão
    options.Cookie.HttpOnly = true; // Impede acesso via JavaScript ao cookie da sessão
    options.Cookie.IsEssential = true; // Torna o cookie da sessão essencial para a funcionalidade da aplicação
});

// Configuração para Swagger/OpenAPI (geralmente para APIs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Bloco para inicialização do banco de dados e seed de dados
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    //context.Database.EnsureDeleted(); 
    //context.Database.Migrate();      
    //await DbInitializer.InitializeAsync(context); 
}

// Configuração do pipeline de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    // Em produção, usa uma página de erro genérica e HSTS para segurança
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger(); // Habilita o middleware do Swagger
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TotemPWA API v1")); // Habilita a UI do Swagger

app.UseHttpsRedirection(); // Redireciona requisições HTTP para HTTPS
app.UseStaticFiles();      // Habilita o serviço de arquivos estáticos (como CSS, JS, imagens em wwwroot)

app.UseRouting(); // Define os pontos de roteamento para endpoints da aplicação

// Habilitar sessão (DEVE VIR APÓS UseRouting e ANTES de UseAuthentication/UseAuthorization)
app.UseSession();

app.UseAuthentication(); // Adicionado: Habilita o middleware de autenticação
app.UseAuthorization(); // Habilita o middleware de autorização

// --- CONFIGURAÇÃO DE ROTAS MVC ---
// app.MapControllers() permite o uso de roteamento por atributos (e.g., [Route("Admin/[controller]/[action]")] )
app.MapControllers();

// app.MapControllerRoute é para roteamento baseado em convenção.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();