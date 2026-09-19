
using System.Security.Claims;
using ApiVazada.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiVazada.Controllers;

// DTO: contém somente os campos que o usuário pode alterar.
public class AtualizarUsuarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsuariosController(AppDbContext db) => _db = db;

    // Consulta do perfil
    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var claim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out int usuarioLogadoId))
            return Unauthorized();

        // Impede consultar o perfil de outro usuário.
        if (id != usuarioLogadoId)
            return Forbid();

        var usuario = _db.Usuarios.Find(id);

        if (usuario is null)
            return NotFound();

        return Ok(new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role
        });
    }

    // CORREÇÃO 04 — Mass Assignment
    [Authorize]
    [HttpPut("{id}")]
    public IActionResult Atualizar(
        int id,
        [FromBody] AtualizarUsuarioRequest dados)
    {
        var claim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out int usuarioLogadoId))
            return Unauthorized();

        // Verifica se o usuário está editando o próprio perfil.
        if (id != usuarioLogadoId)
            return Forbid();

        var usuario = _db.Usuarios.Find(id);

        if (usuario is null)
            return NotFound();

        // Somente os campos permitidos são atualizados.
        usuario.Nome = dados.Nome;
        usuario.Email = dados.Email;

        // Role e Senha NÃO são alteradas.

        _db.SaveChanges();

        return Ok(new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role
        });
    }
}