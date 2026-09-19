
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiVazada.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ApiVazada.Controllers;

[ApiController]
[Route("api/login")]
public class LoginController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public LoginController(
        AppDbContext db,
        IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _db.Usuarios.FirstOrDefault(u =>
            u.Email == request.Email &&
            u.Senha == request.Senha);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Credenciais inválidas."
            });
        }

        // CORREÇÃO 03:
        // Obtém a chave JWT da configuração,
        // sem armazená-la no código-fonte.

        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key não configurada.");

        var key = Encoding.UTF8.GetBytes(jwtKey);

        var handler = new JwtSecurityTokenHandler();

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, user.Nome),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(ClaimTypes.Role, user.Role)
            ]),

            Expires = DateTime.UtcNow.AddHours(2),

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = handler.CreateToken(descriptor);

        return Ok(new
        {
            token = handler.WriteToken(token),
            usuarioId = user.Id,
            role = user.Role
        });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Senha { get; set; } = string.Empty;
}