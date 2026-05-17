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

        public UsuarioService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Usuario?> CadastrarUsuario(Usuario usuario)
        {
            // Corrigido para u.UsuarioLogin e usuario.UsuarioLogin
            if (await _context.Usuarios.AnyAsync(u => u.UsuarioLogin == usuario.UsuarioLogin))
                return null;

            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha, workFactor: 10);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<UsuarioLogin?> AutenticarUsuario(UsuarioLogin usuarioLogin)
        {
            // Corrigido para comparar o UsuarioLogin do Model com o Usuario do DTO
            var buscaUsuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioLogin == usuarioLogin.Usuario);

            if (buscaUsuario == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(usuarioLogin.Senha, buscaUsuario.Senha))
                return null;

            usuarioLogin.Id = buscaUsuario.Id;
            usuarioLogin.Nome = buscaUsuario.Nome;
            usuarioLogin.Foto = buscaUsuario.Foto;
            usuarioLogin.Token = GerarToken(buscaUsuario);
            usuarioLogin.Senha = string.Empty;

            return usuarioLogin;
        }

        private string GerarToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var chave = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    // Corrigido para usuario.UsuarioLogin
                    new Claim(ClaimTypes.Name, usuario.UsuarioLogin) 
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(chave), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}