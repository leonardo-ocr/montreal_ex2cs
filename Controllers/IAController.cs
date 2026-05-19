using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlogPessoal.Services.IA;

namespace BlogPessoal.Controllers;

// garante que apenas usuários com token JWT válido acessem os recursos de IA
[Authorize]
[Route("api/ia")]
[ApiController]
public class IAController : ControllerBase
{
    private readonly IIAService _iaService;

    public IAController(IIAService iaService)
    {
        _iaService = iaService;
    }

    [HttpPost("resumir")]
    public async Task<IActionResult> Resumir([FromBody] string texto)
    {
        // barrar payloads vazios logo na entrada para não consumir cota desnecessária da API do Gemini
        if (string.IsNullOrEmpty(texto))
            return BadRequest("O texto para resumo não pode estar vazio.");

        var resultado = await _iaService.GerarResumoCuriosidadeAsync(texto);
        
        return Ok(resultado);
    }
}