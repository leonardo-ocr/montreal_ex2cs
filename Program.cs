using BlogPessoal.Data;
using BlogPessoal.Services.IA;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do Banco de Dados (MySQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. Registrar os Controllers e o Swagger
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Registrar o Serviço de IA (Gemini)
builder.Services.AddHttpClient<IIAService, GeminiService>();

var app = builder.Build();

// 4. Habilitar o Swagger (Independente de estar em Development ou não, para facilitar seu teste)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog Pessoal v1");
    c.RoutePrefix = "swagger"; // Isso faz o Swagger abrir em http://localhost:5072/swagger
});

app.UseAuthorization();

// 5. Mapear as rotas dos Controllers
app.MapControllers();

app.Run();