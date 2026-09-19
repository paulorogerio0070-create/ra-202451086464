
using System.Text;
using ApiVazada.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Serviços da API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =====================================================
// CORREÇÃO 03 - PARTE A
// Retirar as configurações sensíveis do código-fonte.
// =====================================================

// Lê a conexão com o SQLite do appsettings.json
var connectionString = builder.Configuration
    .GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Default não configurada.");

// Lê a chave JWT do User Secrets (desenvolvimento)
// ou de outra fonte de configuração segura.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key não configurada.");

// Banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Autenticação JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configuração mantida apenas para o laboratório local.
    options.RequireHttpsMetadata = false;

    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)),

            ValidateIssuer = false,
            ValidateAudience = false
        };
});

var app = builder.Build();

// =====================================================
// CORREÇÃO 03 - PARTE B
// Tratamento de erros conforme o ambiente.
// =====================================================

if (app.Environment.IsDevelopment())
{
    // Detalhes de erros permitidos apenas em desenvolvimento
    app.UseDeveloperExceptionPage();

    // Swagger disponível apenas em desenvolvimento
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Em produção, utiliza a página de erro genérica
    app.UseExceptionHandler("/erro");
}

// Endpoint de erro genérico
app.Map("/erro", () =>
    Results.Problem(
        title: "Erro interno do servidor.",
        statusCode: 500
    ));

// Inicialização do banco fictício
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    db.Database.EnsureCreated();
}

// Autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();