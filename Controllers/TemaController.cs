using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogPessoal.Data;
using BlogPessoal.Models;
using BlogPessoal.DTOs;

namespace BlogPessoal.Controllers;

[Authorize]
[Route("api/temas")]
[ApiController]
public class TemaController : ControllerBase
{
    private readonly AppDbContext _context;

    public TemaController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TemaResponse>>> GetAll()
    {
        return await _context.Temas
            .Select(t => new TemaResponse(t.Id, t.Descricao))
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<TemaResponse>> Create(TemaRequest request)
    {
        var tema = new Tema { Descricao = request.Descricao };
        _context.Temas.Add(tema);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = tema.Id }, new TemaResponse(tema.Id, tema.Descricao));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TemaResponse>> Update(long id, [FromBody] Tema request)
    {
        if (id != request.Id)
        {
            return BadRequest("O ID da URL não corresponde ao ID do corpo da requisição.");
        }

        var tema = await _context.Temas.FindAsync(id);
        if (tema == null)
            return NotFound("Tema não encontrado.");

        tema.Descricao = request.Descricao;
        _context.Temas.Update(tema);
        await _context.SaveChangesAsync();

        return Ok(new TemaResponse(tema.Id, tema.Descricao));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var tema = await _context.Temas.FindAsync(id);
        if (tema == null)
            return NotFound("Tema não encontrado.");

        _context.Temas.Remove(tema);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}