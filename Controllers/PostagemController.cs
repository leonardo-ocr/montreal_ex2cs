using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogPessoal.Data;
using BlogPessoal.Models;
using BlogPessoal.Services.IA;
using BlogPessoal.DTOs;

namespace BlogPessoal.Controllers;

// protege todos os endpoints deste controller, exigindo Token JWT
[Authorize]
[Route("api/postagens")]
[ApiController]
public class PostagemController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IIAService _iaService;

    // injeção de dependência do contexto do banco e do serviço de IA
    public PostagemController(AppDbContext context, IIAService iaService)
    {
        _context = context;
        _iaService = iaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Postagem>>> GetAll()
    {
        return await _context.Postagens
            .Include(p => p.Tema)
            .Include(p => p.Usuario)
            .ToListAsync();
    }

    [HttpGet("filtro")]
    public async Task<ActionResult<IEnumerable<Postagem>>> GetByFiltro([FromQuery] long? autor, [FromQuery] long? tema)
    {
        var query = _context.Postagens
            .Include(p => p.Tema)
            .Include(p => p.Usuario)
            .AsQueryable();

        if (autor.HasValue)
        {
            query = query.Where(p => p.UsuarioId == autor.Value);
        }

        if (tema.HasValue)
        {
            query = query.Where(p => p.TemaId == tema.Value);
        }

        return await query.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Postagem>> GetById(long id)
    {
        var postagem = await _context.Postagens
            .Include(p => p.Tema)
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (postagem == null)
            return NotFound("Postagem não encontrada.");

        return Ok(postagem);
    }

    [HttpPost]
    public async Task<ActionResult<Postagem>> Create([FromBody] Postagem postagem)
    {
        // intercepta a requisição e valida as diretrizes da comunidade antes de qualquer processamento
        var moderacao = await _iaService.VerificarConteudoOfensivoAsync(postagem.Texto);
        if (moderacao.EhOfensivo)
        {
            return BadRequest(new 
            { 
                erro = "A postagem contém termos inadequados ou ofensivos que violam as diretrizes do blog.", 
                motivo = moderacao.Motivo 
            });
        }
        
        var resumoCuriosidade = await _iaService.GerarResumoCuriosidadeAsync(postagem.Texto);
        
        postagem.ResumoIA = resumoCuriosidade.Resumo;
        postagem.TagsIA = resumoCuriosidade.Tags;
        postagem.CategoriaIA = resumoCuriosidade.Categoria;

        _context.Postagens.Add(postagem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = postagem.Id }, postagem);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Postagem>> Update(long id, [FromBody] Postagem postagem)
    {
        // previne a vulnerabilidade de alteração de ID via corpo da requisição
        if (id != postagem.Id)
        {
            return BadRequest("O ID da URL não corresponde ao ID do corpo da requisição.");
        }

        var postagemExiste = await _context.Postagens.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (postagemExiste == null)
        {
            return NotFound("Postagem não encontrada.");
        }

        postagem.ResumoIA ??= postagemExiste.ResumoIA;
        postagem.TagsIA ??= postagemExiste.TagsIA;
        postagem.CategoriaIA ??= postagemExiste.CategoriaIA;

        _context.Postagens.Update(postagem);
        await _context.SaveChangesAsync();

        return Ok(postagem);
    }

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

        // retorna 204
        return NoContent();
    }
}