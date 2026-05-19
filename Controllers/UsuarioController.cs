using BlogPessoal.Models;
using BlogPessoal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPessoal.Controllers
{
    [Route("api/usuarios")]
    [ApiController]
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
        [HttpPut("{id}")]
        public async Task<ActionResult<Usuario>> Atualizar(long id, [FromBody] Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest("O ID da URL não corresponde ao ID do corpo da requisição.");
            }

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
            {
                return NotFound("Utilizador não encontrado.");
            }

            return NoContent();
        }
    }
}