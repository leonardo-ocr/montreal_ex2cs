using BlogPessoal.Models;

namespace BlogPessoal.Services
{
    // Contrato que define as regras de negócio para os usuários.
    public interface IUsuarioService
    {
        // o uso do tipo anulável (?) indica que o método pode retornar null,
        // o que usamos como sinalização (ex: se o e-mail já estiver cadastrado) para o Controller devolver um erro 400.
        Task<Usuario?> CadastrarUsuario(Usuario usuario);
        
        // mantém a lógica de hash e comparação de senhas isolada na implementação deste serviço.
        Task<UsuarioLogin?> AutenticarUsuario(UsuarioLogin usuarioLogin);
        
        Task<Usuario?> AtualizarUsuario(Usuario usuario);
        
        // retorna um boolean simples
        Task<bool> DeletarUsuario(long id);
    }
}