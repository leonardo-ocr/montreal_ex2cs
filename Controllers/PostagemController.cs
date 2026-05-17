using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogPessoal.Data;
using BlogPessoal.Models;
using BlogPessoal.Services.IA;
using BlogPessoal.DTOs;

namespace BlogPessoal.Controllers;

[Authorize]
[Route("api/postagens")]
[ApiController]
public class PostagemController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IIAService _iaService;

    public PostagemController(AppDbContext context, IIAService iaService)
    {
        _context = context;
        _iaService = iaService;
    }

    // 1. GET /api/postagens -> Listar todas as postagens.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Postagem>>> GetAll()
    {
        return await _context.Postagens
            .Include(p => p.Tema)
            .Include(p => p.Usuario)
            .ToListAsync();
    }

    // 2. GET /api/postagens/filtro?autor={id}&tema={id} -> Filtrar postagens por autor e/ou tema.
    [HttpGet("filtro")]
    public async Task<ActionResult<IEnumerable<Postagem>>> GetByFiltro([FromQuery] long? autor, [FromQuery] long? tema)
    {
        var query = _context.Postagens
            .Include(p => p.Tema)
            .Include(p => p.Usuario)
            .AsQueryable();

        // Filtro por ID do Autor (Usuário)
        if (autor.HasValue)
        {
            query = query.Where(p => p.UsuarioId == autor.Value);
        }

        // Filtro por ID do Tema
        if (tema.HasValue)
        {
            query = query.Where(p => p.TemaId == tema.Value);
        }

        return await query.ToListAsync();
    }

    // GET /api/postagens/{id} -> Buscar por ID (Auxiliar necessário para o CreatedAtAction)
    [HttpGet("{id}")]
    public async Task<ActionResult<Postagem>> GetById(long id)
    {
        var postagem = await _context.Postagens
            .Include(p => p.Tema)
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (postagem == null)
        {
            return NotFound("Postagem não encontrada.");
        }

        return Ok(postagem);
    }

    // 3. POST /api/postagens -> Criar uma nova postagem.
    [HttpPost]
    public async Task<ActionResult<Postagem>> Create([FromBody] PostagemRequest request)
    {
        var temaExiste = await _context.Temas.FindAsync(request.TemaId);
        if (temaExiste == null)
        {
            return NotFound("O Tema informado não existe.");
        }

        // Recupera o email do usuário logado diretamente do Token JWT
        var usuarioEmail = User.Identity?.Name;
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioLogin == usuarioEmail);

        if (usuario == null)
        {
            return Unauthorized("Usuário inválido.");
        }

        // Chama a IA para gerar os metadados de Física
        var analiseIA = await _iaService.GerarResumoCuriosidadeAsync(request.Texto);

        var novaPostagem = new Postagem
        {
            Titulo = request.Titulo,
            Texto = request.Texto,
            TemaId = request.TemaId,
            UsuarioId = usuario.Id,
            ResumoIA = analiseIA.Resumo,
            TagsIA = analiseIA.Tags,
            CategoriaIA = analiseIA.Categoria
        };

        _context.Postagens.Add(novaPostagem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = novaPostagem.Id }, novaPostagem);
    }

    // 4. PUT /api/postagens/{id} -> Atualizar uma postagem existente.
    [HttpPut("{id}")]
    public async Task<ActionResult<Postagem>> Update(long id, [FromBody] Postagem postagem)
    {
        if (id != postagem.Id)
        {
            return BadRequest("O ID da URL não corresponde ao ID do corpo da requisição.");
        }

        var postagemExiste = await _context.Postagens.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (postagemExiste == null)
        {
            return NotFound("Postagem não encontrada.");
        }

        // Garante que os dados gerados pela IA não se percam na atualização
        postagem.ResumoIA ??= postagemExiste.ResumoIA;
        postagem.TagsIA ??= postagemExiste.TagsIA;
        postagem.CategoriaIA ??= postagemExiste.CategoriaIA;

        _context.Postagens.Update(postagem);
        await _context.SaveChangesAsync();

        return Ok(postagem);
    }

    // 5. DELETE /api/postagens/{id} -> Excluir uma postagem.
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var postagem = await _context.Postagens.FindAsync(id);
        if (postagem == null)
        {
            return NotFound("Postagem não encontrada.");
        }

        _context.Postagens.Remove(postagem);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}