
using ApiVazada.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiVazada.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProdutosController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult GetAll() => Ok(_db.Produtos.ToList());

    // FALHA 1 CORRIGIDA — SQL Injection
    // O Entity Framework utiliza LINQ para tratar
    // a entrada do usuário como dado, e não como comando SQL.

    [HttpGet("buscar")]
    public IActionResult Buscar(string nome)
    {
        var resultado = _db.Produtos
            .Where(p => p.Nome.Contains(nome))
            .ToList();

        return Ok(resultado);
    }

    /// <summary>
    /// Endpoint utilizado para demonstrar a FALHA 3.
    /// Será tratado posteriormente.
    /// </summary>
    [HttpGet("quebrar")]
    public IActionResult Quebrar()
    {
        throw new InvalidOperationException(
            "Erro proposital para demonstrar a exposição de stack trace.");
    }
}