using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogPessoal.Models
{
    public class Postagem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Texto { get; set; } = string.Empty;

        public DateTime? Data { get; set; } = DateTime.Now;

        // Campos para a Integração com IA
        public string? ResumoIA { get; set; }
        public string? TagsIA { get; set; }
        public string? CategoriaIA { get; set; }

        // Chaves Estrangeiras
        public virtual Tema? Tema { get; set; }
        public virtual Usuario? Usuario { get; set; }
    }
}