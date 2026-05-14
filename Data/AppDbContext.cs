using Microsoft.EntityFrameworkCore;
using BlogPessoal.Models;

namespace BlogPessoal.Data {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Postagem> Postagens { get; set; } = null!;
        public DbSet<Tema> Temas { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
    }
}