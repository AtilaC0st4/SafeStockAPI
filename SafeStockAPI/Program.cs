using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using SafeStockAPI.Services; // Adicione este using

var builder = WebApplication.CreateBuilder(args);

// Configuração do Oracle
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("OracleConnection");
    options.UseOracle(connectionString, oracleOptions =>
    {
        oracleOptions.CommandTimeout(180);
    });
});

// Configuração do CORS (Política ampla para desenvolvimento)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("*");
    });
});

// Configuração de logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configuração dos serviços de Machine Learning
builder.Services.AddSingleton<MLPriorityService>(); // Adicione esta linha
// OU, se preferir usar interface:
// builder.Services.AddScoped<IMLPriorityService, MLPriorityService>();

// Configuração dos controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SafeStockAPI",
        Version = "v1",
        Description = "API para gerenciamento de estoque com ML",
        Contact = new OpenApiContact
        {
            Name = "Equipe de Suporte",
            Email = "suporte@safestock.com"
        }
    });
});

var app = builder.Build();

// Middleware pipeline (ORDEM É CRÍTICA)
app.UseCors("AllowAll"); // Deve vir primeiro

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SafeStockAPI v1");
        c.RoutePrefix = string.Empty;
    });
    app.UseDeveloperExceptionPage();
}

app.UseAuthorization();
app.MapControllers();

// Configuração para aceitar conexões externas
app.Urls.Add("http://0.0.0.0:5194");
app.Urls.Add("https://0.0.0.0:7093");

// Aplicar migrations automaticamente (apenas para desenvolvimento)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("Migrations aplicadas com sucesso.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrations: {ex.Message}");
    }
}

// Middleware para log de requisições
app.Use(async (context, next) =>
{
    Console.WriteLine($"Requisição recebida: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"Resposta enviada: {context.Response.StatusCode}");
});

app.Run();