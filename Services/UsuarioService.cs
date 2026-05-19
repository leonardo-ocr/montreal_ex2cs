using BlogPessoal.Data;
using BlogPessoal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogPessoal.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // injeção do contexto do EF e das configurações
        public UsuarioService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Usuario?> CadastrarUsuario(Usuario usuario)
        {
            // impede a criação de contas duplicadas com o mesmo e-mail
            if (await _context.Usuarios.AnyAsync(u => u.UsuarioLogin == usuario.UsuarioLogin))
                return null;

            //aplica hash na senha com BCrypt antes de salvar
            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha, workFactor: 10);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<UsuarioLogin?> AutenticarUsuario(UsuarioLogin usuarioLogin)
        {
            var buscaUsuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioLogin == usuarioLogin.Usuario);

            if (buscaUsuario == null)
                return null;

            //valida a senha fornecida em texto limpo contra o hash seguro armazenado no banco
            if (!BCrypt.Net.BCrypt.Verify(usuarioLogin.Senha, buscaUsuario.Senha))
                return null;

            usuarioLogin.Id = buscaUsuario.Id;
            usuarioLogin.Nome = buscaUsuario.Nome;
            usuarioLogin.Foto = buscaUsuario.Foto ?? string.Empty;
            usuarioLogin.Token = GerarToken(buscaUsuario);
            
            usuarioLogin.Senha = string.Empty;

            return usuarioLogin;
        }

        public async Task<Usuario?> AtualizarUsuario(Usuario usuario)
        {
            var usuarioExiste = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == usuario.Id);
            if (usuarioExiste == null)
                return null;

            // só processa e gasta processamento com um novo hash se o usuário realmente enviou uma senha nova
            if (!string.IsNullOrEmpty(usuario.Senha) && usuario.Senha != usuarioExiste.Senha)
            {
                usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha, workFactor: 10);
            }
            else
            {
                usuario.Senha = usuarioExiste.Senha;
            }

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<bool> DeletarUsuario(long id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return true;
        }
        
        private string GerarToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var chave = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // inclui o email do usuário no payload do token como claim de identificação (Subject)
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, usuario.UsuarioLogin) 
                }),
                // define tempo de vida curto (2h) para mitigar danos caso o token seja interceptado
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(chave), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}