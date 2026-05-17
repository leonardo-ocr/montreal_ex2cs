using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BlogPessoal.Models;

public class Postagem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonRequired]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Texto { get; set; } = string.Empty;

    public string? ResumoIA { get; set; }
    public string? TagsIA { get; set; }
    public string? CategoriaIA { get; set; }

    [JsonRequired]
    public long TemaId { get; set; }
    public virtual Tema? Tema { get; set; }
    
    [JsonRequired]
    public long UsuarioId { get; set; }
    public virtual Usuario? Usuario { get; set; }
}