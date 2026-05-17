using BlogPessoal.Models;
using BlogPessoal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPessoal.Controllers
{
    [Route("api/usuarios")]
    [ApiController] // <-- Esta anotação é obrigatória para o Swagger mapear as rotas
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [AllowAnonymous]
        [HttpPost("cadastrar")]
        public async Task<ActionResult<Usuario>> Cadastrar([FromBody] Usuario usuario)
        {
            var resposta = await _usuarioService.CadastrarUsuario(usuario);

            if (resposta == null)
                return BadRequest("Usuário já existe!");

            return StatusCode(StatusCodes.Status201Created, resposta);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UsuarioLogin>> Autenticar([FromBody] UsuarioLogin usuarioLogin)
        {
            var resposta = await _usuarioService.AutenticarUsuario(usuarioLogin);

            if (resposta == null)
                return Unauthorized("Usuário e/ou senha inválidos!");

            return Ok(resposta);
        }

        [Authorize]
        [HttpPut("atualizar")]
        public async Task<ActionResult<Usuario>> Atualizar([FromBody] Usuario usuario)
        {
            var resposta = await _usuarioService.AtualizarUsuario(usuario);

            if (resposta == null)
                return NotFound("Utilizador não encontrado.");

            return Ok(resposta);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(long id)
        {
            var apagado = await _usuarioService.DeletarUsuario(id);

            if (!apagado)
                {return NotFound("Utilizador não encontrado.");
                }

            return NoContent();
        }
    }
}