using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Utilities; // Certifique-se que esta pasta 'Utilities' e classes como AdminViewLocationExpander existem

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
// Importante: EnsureDeleted() APAGA o banco de dados. Use com cautela, principalmente em produção!
// Migrate() aplica as migrações pendentes.
// DbInitializer.InitializeAsync(context) é para popular dados iniciais.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    // context.Database.EnsureDeleted(); // CUIDADO: Descomentar esta linha APAGA o banco de dados a cada inicialização!
    context.Database.Migrate();      // Garante que o banco de dados está atualizado com as últimas migrações
    // await DbInitializer.InitializeAsync(context); // Se você estiver usando o DbInitializer para seed inicial
                                                    // Certifique-se de que este arquivo existe e está correto
                                                    // e se não tiver, comente esta linha.
}

// Configuração do pipeline de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    // Em produção, usa uma página de erro genérica e HSTS para segurança
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
// Em desenvolvimento (o padrão), você verá a página de erro detalhada do desenvolvedor

app.UseSwagger(); // Habilita o middleware do Swagger
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TotemPWA API v1")); // Habilita a UI do Swagger

app.UseHttpsRedirection(); // Redireciona requisições HTTP para HTTPS
app.UseStaticFiles();      // Habilita o serviço de arquivos estáticos (como CSS, JS, imagens em wwwroot)

app.UseRouting(); // Define os pontos de roteamento para endpoints da aplicação

// Habilitar sessão (DEVE VIR APÓS UseRouting e ANTES de UseAuthorization)
app.UseSession();

app.UseAuthorization(); // Habilita o middleware de autorização

// --- CONFIGURAÇÃO DE ROTAS MVC ---
// app.MapControllers() permite o uso de roteamento por atributos (e.g., [Route("Admin/[controller]/[action]")] )
app.MapControllers();

// app.MapControllerRoute é para roteamento baseado em convenção.
// A rota "default" é a mais comum e deve ser a última.
// Removemos as chamadas a 'MapStaticAssets' e '.WithStaticAssets()' aqui para um roteamento MVC padrão.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Se você tiver um método 'MapStaticAssets' no seu projeto, ele não será mais usado no pipeline de roteamento MVC
// padrão. Ele pode ser removido se não for usado para outros propósitos.
// app.MapStaticAssets(); // Se não for mais usado, pode ser removido daqui.

app.Run(); // Inicia a aplicação