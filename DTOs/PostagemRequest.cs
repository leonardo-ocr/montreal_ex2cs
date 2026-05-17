using System.Text.Json.Serialization;

namespace BlogPessoal.DTOs;

public class PostagemRequest
{
    [JsonRequired]
    public string Titulo { get; set; } = string.Empty;

    [JsonRequired]
    public string Texto { get; set; } = string.Empty;

    [JsonRequired]
    public long TemaId { get; set; }
}