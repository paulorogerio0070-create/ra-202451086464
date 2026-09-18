# HANDOUT — AULA 07

## Caça às Vulnerabilidades

*Revisão de segurança de uma API .NET — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês são a dupla de revisores de segurança da empresa. Os 4 trechos abaixo são da MESMA API, prestes a ir para produção. Para CADA card:

- Descrevam a falha com as próprias palavras (não precisa do nome técnico ainda)
- Estimem o dano possível se isso chegar à produção
- Proponham a correção

*⏱️ Tempo: 30 minutos  |  👥 Formato: em duplas  |  Todo o código é fictício e roda apenas no laboratório.*

> **Nomes:** Paulo Rogério de Jesus Junior   **Turma:** Arquitetura de Aplicações WEB   **Data:** 17 / 09 / 2026

## VULNERABILIDADE 01 — A busca de clientes

> `GET /api/clientes/buscar?nome=...`

Endpoint de busca usado pela tela de atendimento. O parâmetro nome vem direto da caixa de busca do site.

```text
 1  [HttpGet("buscar")]
 2  public IActionResult Buscar(string nome)
 3  {
 4      var sql = "SELECT * FROM Clientes WHERE Nome = '"
 5                + nome + "'";
 6      var clientes = _db.Clientes.FromSqlRaw(sql).ToList();
 7      return Ok(clientes);
 8  }
```

**Sua análise:**

1. Qual é a falha?
O sistema recebe o parâmetro "nome" e insere direto no comando do SQL, isso sem utilizar nenhum parâmetro seguro

2. Qual o dano possível em produção?
Um usuário pode usar para consultar dados de outras pessoas, no caso usando o nome de outra pessoa. Causando vazamento de dados 

3. Como corrigir?

[HttpGet("buscar")]
public IActionResult Buscar(string nome)
{
    var clientes = _db.Clientes
        .FromSqlInterpolated(
            $"SELECT * FROM Clientes WHERE Nome = {nome}")
        .ToList();

    return Ok(clientes);
}

## VULNERABILIDADE 02 — A consulta de faturas

> `GET /api/faturas/{id}`

Endpoint usado pelo app para exibir a fatura do cartão. O usuário está autenticado quando chama esta rota.

```text
 1  [HttpGet("{id}")]
 2  public IActionResult GetFatura(int id)
 3  {
 4      var fatura = _db.Faturas.Find(id);
 5      if (fatura == null) return NotFound();
 6      return Ok(fatura);
 7  }
```

**Sua análise:**

1. Qual é a falha?
Apesar do usuário estar autenticado no sistema não tem uma autenticação pra saber se ele tem acesso ou não a fatura em questão

2. Qual o dano possível em produção?
O usuário pode acessar fatura de outros clientes causando vazamento de dados e valores de outras pessoas

3. Como corrigir?

[HttpGet("{id}")]
public IActionResult GetFatura(int id)
{
    var fatura = _db.Faturas.Find(id);

    if (fatura == null)
        return NotFound();

    if (fatura.ClienteId != usuarioLogadoId)
        return Forbid();

    return Ok(fatura);
}

## VULNERABILIDADE 03 — A configuração do servidor

> `Program.cs (roda igual em dev e em produção)`

Trecho de inicialização da API, idêntico em todos os ambientes. Este arquivo está versionado no Git da empresa.

```text
 1  public const string Conn =
 2      "Server=prod-db;Database=Banco;User=sa;" +
 3      "Password=Newton@2026!";
 4
 5  var app = WebApplication.CreateBuilder(args).Build();
 6  app.UseDeveloperExceptionPage();
 7  app.Run();
```

**Sua análise:**

1. Qual é a falha?
São duas falhas, a primeira é que a senha do banco consta do código fonte e outra como o arquivo está versionado no git, quem tiver acesso a pasta consegue acessar o banco de dados

2. Qual o dano possível em produção?
O banco de dados pode ser acessado e comprometido por pessoal não autorizado

3. Como corrigir?

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration
    .GetConnectionString("Default");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Run();

## VULNERABILIDADE 04 — A atualização de perfil

> `PUT /api/usuarios/{id}`

Endpoint que o app chama quando o usuário edita o próprio perfil. O corpo da requisição é o JSON enviado pelo cliente.

```text
 1  public class UsuarioUpdate
 2  {
 3      public string Nome  { get; set; }
 4      public string Email { get; set; }
 5      public string Role  { get; set; }   // "user" | "admin"
 6  }
 7
 8  [HttpPut("{id}")]
 9  public IActionResult Atualizar(int id, UsuarioUpdate dto)
10  {
11      _repo.AtualizarTudo(id, dto);
12      return NoContent();
13  }
```

**Sua análise:**

1. Qual é a falha?
O sistema aceita os dados de Role sem restringir

2. Qual o dano possível em produção?
Um user comum pode ter acesso aos privlégios de admin

3. Como corrigir?

public class UsuarioUpdateDto
{
    public string Nome { get; set; }
    public string Email { get; set; }
}

[HttpPut("{id}")]
public IActionResult Atualizar(
    int id,
    UsuarioUpdateDto dto)
{
    // Verificar se o usuário autenticado pode editar este perfil.

    _repo.AtualizarNomeEmail(
        id,
        dto.Nome,
        dto.Email
    );

    return NoContent();´´´´´´´´´´´´´´´´´´´´´´
}

## DESAFIO

1. Qual das 4 falhas um scanner automático de código teria MAIS dificuldade de encontrar? Por quê?

A vulnerabilidade 02, relacionada ao controle de acesso (IDOR), tende a ser especialmente difícil de identificar somente por meio da análise automática do código.

Isso acontece porque o método pode funcionar corretamente do ponto de vista técnico: recebe um identificador, consulta o banco de dados e retorna uma fatura existente. O problema está na regra de negócio, pois o sistema deveria verificar se aquela fatura pertence ao usuário autenticado.

Para identificar a falha, é necessário compreender o relacionamento entre usuários e faturas e testar o comportamento da aplicação com diferentes identidades. Um scanner pode ajudar a detectar esse problema, mas nem sempre consegue compreender todas as regras de autorização do sistema.
