using BlogPessoal.DTOs;

namespace BlogPessoal.Services.IA;

public interface IIAService
{
    Task<ResultadoIA> GerarResumoCuriosidadeAsync(string conteudo);
    Task<ResultadoModeracaoIA> VerificarConteudoOfensivoAsync(string conteudo);
}