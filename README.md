# Blog Pessoal - Backend API (.NET & IA)

Esta é uma API desenvolvida em ASP.NET Core para o gerenciamento de um blog pessoal. O projeto cobre desde a infraestrutura básica de um CRUD relacional e autenticação até o processamento inteligente de conteúdo utilizando IA generativa para categorização, resumos automáticos e moderação de conteúdo.

O design da aplicação foi pensado para ser limpo, extensível e totalmente aderente às boas práticas do ecossistema .NET, dividindo responsabilidades através de uma arquitetura em camadas clara.

---

## Tecnologias e Decisões de Projeto

- **Runtime & Framework:** .NET 8 (ASP.NET Core Web API).
- **Persistência:** Entity Framework Core integrado ao MySQL (via Pomelo). Relacionamentos estruturados de forma estrita (Usuários, Temas e Postagens) usando propriedades de navegação virtuais.
- **Segurança:** Autenticação e autorização via JWT (JSON Web Tokens). O Swagger foi customizado para permitir testes locais injetando o cabeçalho `Authorization: Bearer <token>` de forma nativa.
- **Camada de IA:** Integração com a API do Google Gemini (`gemini-2.5-flash`). A escolha do Gemini trouxe excelente tempo de resposta e permitiu parametrizar um prompt focado em análise técnica/científica (garantindo resumos precisos e extração de tags contextualizadas), além de atuar como um moderador automatizado de comunidade, avaliando e bloqueando publicações com conteúdo ofensivo antes da persistência.
- **Qualidade de Código:** Estruturação preparada para análise do SonarQube e tratamento de exceções específico na camada de integração de serviços HTTP.

---

## Arquitetura do Sistema

A base de código segue o padrão de divisão por responsabilidade única:

* **Controllers/**: Exposição dos endpoints REST e validação preliminar de rotas.
* **Services/**: Regras de negócio e lógica de autenticação.
* **IA/**: Integração externa com provedores de Inteligência Artificial.
* **Models/**: Entidades mapeadas para o banco de dados (Code-First).
* **DTOs/**: Objetos de transferência de dados para requests e responses limpos.
* **Data/**: Contexto do Entity Framework (`AppDbContext`) e Migrations.

---

## Rotas e Endpoints da API

A API segue estritamente os padrões RESTful.

### Autenticação e Usuários (`api/usuarios`)
- `POST /cadastrar` -> Criação de novas contas.
- `POST /login` -> Validação de credenciais e geração do Token JWT.
- `PUT /{id}` -> Atualização de dados cadastrais.
- `DELETE /{id}` -> Remoção de conta.

### Gerenciamento de Temas (`api/temas`)
- `GET /` -> Listagem completa de categorias.
- `POST /` -> Cadastro de novos temas.
- `PUT /{id}` -> Atualização de descrições.
- `DELETE /{id}` -> Exclusão de temas.

### Postagens (`api/postagens`)
- `GET /` -> Retorna o feed completo do blog.
- `GET /{id}` -> Busca uma postagem específica pelo ID.
- `GET /filtro` -> Filtro flexível via query string (`?autor={id}&tema={id}`).
- `POST /` -> Publicação de artigos (com validação de moderação via IA; bloqueia conteúdos ofensivos com status 400). 
- `PUT /{id}` -> Edição de conteúdo (preservando metadados gerados por IA).
- `DELETE /{id}` -> Remoção física do post.

### Inteligência Artificial (`api/ia`)
- `POST /resumir` -> Recebe uma string de texto, processa via prompt no Gemini e retorna um JSON estruturado contendo: `Resumo`, `Tags` e `Categoria`.

---

## Como Executar o Projeto Localmente

### Pré-requisitos
- .NET SDK 8 instalado
- Instância do MySQL ativa (local ou Docker)

### Configurando credenciais
As chaves privadas não estão no controle de versão. Antes de rodar, crie o arquivo `appsettings.json` na raiz da aplicação com a estrutura correta:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=blog_pessoal;Uid=seu_usuario;Pwd=sua_senha;"
  },
  "JwtSettings": {
    "SecretKey": "SUA_CHAVE_JWT"
  },
  "Gemini": {
    "ApiKey": "SUA_API_KEY_DO_GEMINI"
  }
}