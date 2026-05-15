using Microsoft.AspNetCore.Mvc;
using BlogPessoal.Services.IA;

namespace BlogPessoal.Controllers;

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
    public async Task<ActionResult<ResultadoIA>> ResumirPostagem([FromBody] string conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
            return BadRequest("O conteúdo não pode estar vazio.");

        try
        {
            var resultado = await _iaService.GerarResumoCuriosidadeAsync(conteudo);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao processar na IA: {ex.Message}");
        }
    }
}