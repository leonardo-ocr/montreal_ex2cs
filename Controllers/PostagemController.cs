using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogPessoal.Data;
using BlogPessoal.Models;
using BlogPessoal.Services.IA;

namespace BlogPessoal.Controllers;

[Authorize] // <-- O bloqueio de segurança que exige o Token JWT
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

    [HttpPost]
    public async Task<ActionResult<Postagem>> Create(PostagemRequest request)
    {
        var temaExiste = await _context.Temas.FindAsync(request.TemaId);
        if (temaExiste == null)
            return NotFound("O Tema informado não existe.");

        var analiseIA = await _iaService.GerarResumoCuriosidadeAsync(request.Texto);

        var novaPostagem = new Postagem
        {
            Titulo = request.Titulo,
            Texto = request.Texto,
            TemaId = request.TemaId,
            ResumoIA = analiseIA.Resumo,
            TagsIA = analiseIA.Tags,
            CategoriaIA = analiseIA.Categoria
        };

        _context.Postagens.Add(novaPostagem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = novaPostagem.Id }, novaPostagem);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Postagem>> GetById(long id)
    {
        var postagem = await _context.Postagens
            .Include(p => p.Tema)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (postagem == null)
            return NotFound();

        return Ok(postagem);
    }
}