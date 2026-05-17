using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogPessoal.Data;
using BlogPessoal.Models;
using BlogPessoal.DTOs;

namespace BlogPessoal.Controllers;

[Authorize] // <-- O bloqueio de segurança que exige o Token JWT
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
}