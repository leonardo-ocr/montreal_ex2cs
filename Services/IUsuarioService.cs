using BlogPessoal.Models;

namespace BlogPessoal.Services
{
    public interface IUsuarioService
    {
        Task<Usuario?> CadastrarUsuario(Usuario usuario);
        Task<UsuarioLogin?> AutenticarUsuario(UsuarioLogin usuarioLogin);
        Task<Usuario?> AtualizarUsuario(Usuario usuario); // <-- Adicionar esta linha
        Task<bool> DeletarUsuario(long id);               // <-- Adicionar esta linha
    }
}