using System.Text;
using System.Text.Json;
using BlogPessoal.DTOs;

namespace BlogPessoal.Services.IA;

public class GeminiService : IIAService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        
        // Correção do Aviso S3928: Usando InvalidOperationException para chaves de configuração ausentes
        _apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("A API Key do Gemini não foi configurada no arquivo appsettings.json.");
    }

    public async Task<ResultadoIA> GerarResumoCuriosidadeAsync(string conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
        {
            return new ResultadoIA("Conteúdo vazio fornecido para análise.", "vazio", "Nenhum");
        }

        var apiKeyLimpa = _apiKey.Trim(); 
        
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKeyLimpa}";
        
        var prompt = $"Atue como um especialista em física. Analise este texto e retorne estritamente um JSON no seguinte formato: {{\"Resumo\": \"resumo curto\", \"Tags\": \"tag1, tag2, tag3\", \"Categoria\": \"Área da Física\"}}. Texto: {conteudo}";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var erroDoGoogle = await response.Content.ReadAsStringAsync();
            
            // Correção do Aviso S112: Lançando HttpRequestException em vez de uma Exception genérica
            throw new HttpRequestException($"Google API recusou a requisição. Código: {response.StatusCode}. Detalhe: {erroDoGoogle}", null, response.StatusCode);
        }

        var responseString = await response.Content.ReadAsStringAsync();
        
        using var jsonDocument = JsonDocument.Parse(responseString);
        var textResult = jsonDocument.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        var cleanJson = textResult?.Replace("```json", "").Replace("```", "").Trim();

        return JsonSerializer.Deserialize<ResultadoIA>(cleanJson ?? "{}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
               ?? new ResultadoIA("Erro ao gerar resumo", "erro", "Erro");
    }
}