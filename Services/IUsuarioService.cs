using BlogPessoal.Models;

namespace BlogPessoal.Services
{
    public interface IUsuarioService
    {
        Task<Usuario?> CadastrarUsuario(Usuario usuario);
        Task<UsuarioLogin?> AutenticarUsuario(UsuarioLogin usuarioLogin);
    }
}