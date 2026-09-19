
using System.Security.Claims;
using ApiVazada.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiVazada.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _db;

    public PedidosController(AppDbContext db) => _db = db;

    // CORREÇÃO 02 — IDOR
    // Identifica o usuário autenticado por meio do token JWT.

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var usuarioIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out int usuarioLogadoId))
            return Unauthorized();

        var pedido = _db.Pedidos.Find(id);

        if (pedido is null)
            return NotFound();

        // Verifica se o pedido pertence ao usuário autenticado.
        if (pedido.UsuarioId != usuarioLogadoId)
            return Forbid();

        return Ok(pedido);
    }

    // CORREÇÃO 02 — Proteção da listagem de pedidos.

    [Authorize]
    [HttpGet("usuario/{usuarioId}")]
    public IActionResult GetByUsuario(int usuarioId)
    {
        var usuarioIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out int usuarioLogadoId))
            return Unauthorized();

        // Impede consultar a lista de pedidos de outro usuário.
        if (usuarioId != usuarioLogadoId)
            return Forbid();

        var pedidos = _db.Pedidos
            .Where(p => p.UsuarioId == usuarioLogadoId)
            .ToList();

        return Ok(pedidos);
    }
}