using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BlogPessoal.Models
{
    public class Tema
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonRequired]
        public long Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Descricao { get; set; } = string.Empty;

        public virtual ICollection<Postagem>? Postagem { get; set; }
    }
}