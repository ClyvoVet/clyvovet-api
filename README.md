# ClyvoVet - API RESTful .NET 8 | Sprint 4

## Sobre o projeto

O **ClyvoVet** é uma API RESTful desenvolvida em **ASP.NET Core 8** para gerenciamento de tutores, pets, consultas veterinárias, medicações e histórico clínico.

A Sprint 4 consolida as funcionalidades das sprints anteriores e adiciona os requisitos finais da disciplina **Advanced Business Development with .NET**:

- Clean Architecture e Injeção de Dependência;
- tratamento global de exceções;
- API RESTful documentada com Swagger/OpenAPI;
- paginação, ordenação e filtros;
- HATEOAS nos endpoints de consulta;
- autenticação e autorização via JWT;
- persistência relacional com Oracle e Entity Framework Core;
- persistência NoSQL com MongoDB;
- padrão Repository;
- Health Checks, Serilog, OpenTelemetry e métricas;
- testes unitários e de integração com xUnit, Moq e padrão AAA;
- coleta de cobertura de testes com Coverlet.

---

## Índice

1. [Arquitetura](#arquitetura)
2. [Modelo de dados](#modelo-de-dados)
3. [Tecnologias](#tecnologias)
4. [Autenticação e autorização JWT](#autenticação-e-autorização-jwt)
5. [Paginação, ordenação, filtros e HATEOAS](#paginação-ordenação-filtros-e-hateoas)
6. [Persistência Oracle](#persistência-oracle)
7. [Persistência MongoDB](#persistência-mongodb)
8. [Tratamento global de exceções](#tratamento-global-de-exceções)
9. [Endpoints](#endpoints)
10. [Swagger / OpenAPI](#swagger--openapi)
11. [Monitoramento e observabilidade](#monitoramento-e-observabilidade)
12. [Testes automatizados](#testes-automatizados)
13. [Executando a aplicação localmente](#executando-a-aplicação-localmente)
14. [Docker](#docker)
15. [Migrations](#migrations)
16. [Integrantes](#integrantes)

---

## Arquitetura

O projeto está organizado em camadas, mantendo as responsabilidades separadas.

```text
ClyvoVet.Api/
├── Presentation/
│   └── Controllers/
├── Application/
│   ├── Dtos/
│   ├── Helpers/
│   ├── Interfaces/
│   ├── Mappers/
│   └── UseCases/
├── Domain/
│   ├── Entities/
│   ├── Errors/
│   ├── Interfaces/
│   └── Models/
├── Infrastructure/
│   ├── Data/
│   │   ├── Migrations/
│   │   └── Repositories/
│   ├── Errors/
│   ├── IoC/
│   ├── Observability/
│   └── Security/
└── Program.cs
```

### Diagrama da solução

```mermaid
flowchart TD
    Client[Cliente / Swagger] --> Presentation[Presentation - Controllers]
    Presentation --> Application[Application - UseCases / DTOs / Mappers]
    Application --> Domain[Domain - Entities / Interfaces / Errors]

    Infrastructure[Infrastructure] -. implementa contratos .-> Domain
    Infrastructure --> Oracle[(Oracle Database)]
    Infrastructure --> Mongo[(MongoDB)]
    Infrastructure --> JWT[JWT Security]
    Infrastructure --> Obs[Serilog / OpenTelemetry / Metrics]

    Application --> Infrastructure
```

### Responsabilidade de cada camada

- **Presentation:** expõe os endpoints HTTP e traduz o resultado da aplicação em respostas HTTP.
- **Application:** contém os casos de uso, DTOs, mapeamentos, paginação e construção de HATEOAS.
- **Domain:** contém entidades, contratos de repositories, `OperationResult<T>` e erros do domínio.
- **Infrastructure:** implementa persistência Oracle/MongoDB, segurança JWT, observabilidade e DI.

A configuração das dependências fica centralizada em:

```text
ClyvoVet.Api/Infrastructure/IoC/Bootstrap.cs
```

---

## Modelo de dados

### Oracle - modelo relacional

```text
TUTOR
  └── 1:N PET
          ├── 1:N CONSULTA
          └── 1:N MEDICACAO
```

Tabelas relacionais:

- `TUTOR`
- `PET`
- `CONSULTA`
- `MEDICACAO`

A entidade `Tutor` continua sendo a identidade utilizada para cadastro e login. Não existe uma tabela separada `User` no modelo atual.

### MongoDB - modelo NoSQL

O MongoDB é utilizado para documentos de **histórico clínico**:

```text
historico_clinico
├── Id (ObjectId)
├── IdPet
├── DataRegistro
├── Tipo
├── Descricao
└── Tags[]
```

O histórico clínico foi mantido em NoSQL para demonstrar armazenamento de documentos flexíveis sem alterar o modelo relacional principal.

---

## Tecnologias

### Backend

- C#
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8

### Persistência

- Oracle Database
- Oracle.EntityFrameworkCore
- MongoDB
- MongoDB.EntityFrameworkCore
- MongoDB.Bson

### Segurança

- Microsoft.AspNetCore.Authentication.JwtBearer
- JWT Bearer Token
- `[Authorize]` e `[AllowAnonymous]`

### Observabilidade

- Microsoft Health Checks
- AspNetCore.HealthChecks.Oracle
- Serilog
- OpenTelemetry
- System.Diagnostics.Metrics

### Documentação

- Swagger / OpenAPI
- Swashbuckle.AspNetCore
- Swagger Annotations

### Testes

- xUnit
- Moq
- WebApplicationFactory
- EF Core InMemory
- Coverlet Collector

---

## Autenticação e autorização JWT

A Sprint 4 substitui o retorno simples do login por um **JWT**.

### Cadastro público

```http
POST /api/tutors
```

Exemplo:

```json
{
  "idTutor": 1,
  "nome": "Tutor Teste",
  "email": "tutor@teste.com",
  "telefone": "11999999999",
  "cpf": "123.456.789-00",
  "senha": "123456"
}
```

### Login público

```http
POST /api/tutors/login
```

```json
{
  "email": "tutor@teste.com",
  "senha": "123456"
}
```

Resposta:

```json
{
  "email": "tutor@teste.com",
  "jwt": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAtUtc": "2026-10-23T04:00:00Z"
}
```

O token possui claims de:

- identificador do tutor;
- e-mail;
- nome de autenticação;
- role `tutor`;
- expiração.

Os endpoints de negócio utilizam `[Authorize(Roles = "tutor")]`, demonstrando autorização baseada em role. Cadastro e login utilizam `[AllowAnonymous]`.

### Configuração JWT

Em `appsettings.Development.json`:

```json
"Secretkey": "ClyvoVet_Sprint4_FIAP_2026_Chave_JWT_Segura_123456"
```

A chave é lida diretamente por `builder.Configuration["Secretkey"]`, seguindo o padrão utilizado em aula. A validade do token é de 8 horas e fica definida no código. Para ambientes compartilhados ou produção, substitua a chave por uma chave segura e preferencialmente configure-a por variável de ambiente:

```text
Secretkey
```

---

## Paginação, ordenação, filtros e HATEOAS

Os principais endpoints `GET` de coleção utilizam paginação por deslocamento.

Parâmetros comuns:

| Parâmetro | Descrição | Padrão |
|---|---|---:|
| `deslocamento` | quantidade de registros ignorados | `0` |
| `registroRetornado` | quantidade máxima de registros | `10` |
| `ordenarPor` | propriedade utilizada na ordenação | ID do recurso |
| `direcao` | `asc` ou `desc` | `asc` |

Exemplo:

```http
GET /api/pets?deslocamento=0&registroRetornado=10&especie=Cachorro&ordenarPor=nome&direcao=asc
Authorization: Bearer {token}
```

Resposta paginada:

```json
{
  "items": [
    {
      "idPet": 1,
      "idTutor": 1,
      "nome": "Thor",
      "especie": "Cachorro",
      "links": [
        { "href": "/api/pets/1", "rel": "self", "method": "GET" },
        { "href": "/api/pets/1", "rel": "update", "method": "PUT" },
        { "href": "/api/pets/1", "rel": "delete", "method": "DELETE" }
      ]
    }
  ],
  "deslocamento": 0,
  "registroRetornado": 10,
  "totalRegistros": 1,
  "links": [
    { "href": "/api/pets?...", "rel": "self", "method": "GET" },
    { "href": "/api/pets", "rel": "create", "method": "POST" }
  ]
}
```

### Filtros disponíveis

**Tutores**

- `nome`
- `email`

**Pets**

- `nome`
- `especie`
- `idTutor`

**Consultas**

- `idPet`
- `veterinario`
- `dataInicio`
- `dataFim`

**Medicações**

- `idPet`
- `nome`

**Histórico clínico (MongoDB)**

- `idPet`
- `tipo`

---

## Persistência Oracle

O `ApplicationContext` utiliza Entity Framework Core com Oracle.

Repositories concretos:

```text
TutorRepository
PetRepository
ConsultaRepository
MedicacaoRepository
```

O padrão Repository mantém o acesso a dados fora dos Controllers e UseCases.

A migration atual é uma `InitialCreate` limpa contendo somente as tabelas atuais do projeto.

---

## Persistência MongoDB

O contexto NoSQL está em:

```text
Infrastructure/Data/MongoApplicationContext.cs
```

Repository:

```text
HistoricoClinicoRepository
```

Caso de uso:

```text
HistoricoClinicoUseCase
```

Controller:

```text
HistoricosClinicosController
```

Configuração padrão:

```json
"ConnectionStrings": {
  "Mongodb": "mongodb://localhost:27017"
}
```

O nome do banco MongoDB é definido no `Bootstrap.cs` como `ClyvoVet`, seguindo o padrão utilizado no projeto de exemplo da disciplina.

A coleção é criada/utilizada pelo MongoDB quando os documentos são persistidos.

---

## Tratamento global de exceções

A API utiliza um `IExceptionHandler` global:

```text
Infrastructure/Errors/GlobalExceptionHandler.cs
```

O tratamento centralizado padroniza erros inesperados e devolve respostas no formato `ProblemDetails`.

Mapeamentos principais:

| Exceção | HTTP |
|---|---:|
| recurso não encontrado | `404` |
| argumento/formato inválido | `400` |
| conflito de persistência EF Core | `409` |
| erro não tratado | `500` |

As Controllers não precisam repetir blocos `try/catch` genéricos em todos os métodos.

Além disso, a camada Application utiliza `OperationResult<T>` para padronizar fluxos esperados de sucesso e falha.

---

## Endpoints

> Endpoints protegidos exigem `Authorization: Bearer {token}`.

### Tutores

| Método | Endpoint | Autorização | Função |
|---|---|---|---|
| POST | `/api/tutors` | público | cadastrar tutor |
| POST | `/api/tutors/login` | público | autenticar e gerar JWT |
| GET | `/api/tutors` | JWT | paginação, filtros e HATEOAS |
| GET | `/api/tutors/{id}` | JWT | consultar tutor |
| PUT | `/api/tutors/{id}` | JWT | atualizar tutor |
| DELETE | `/api/tutors/{id}` | JWT | excluir tutor |

### Pets

| Método | Endpoint | Função |
|---|---|---|
| GET | `/api/pets` | paginação, ordenação, filtros e HATEOAS |
| GET | `/api/pets/{id}` | obter pet |
| GET | `/api/pets/tutor/{idTutor}` | filtrar por tutor |
| GET | `/api/pets/especie/{especie}` | filtrar por espécie |
| POST | `/api/pets` | cadastrar |
| PUT | `/api/pets/{id}` | atualizar |
| DELETE | `/api/pets/{id}` | excluir |

### Consultas

| Método | Endpoint | Função |
|---|---|---|
| GET | `/api/consultas` | paginação, ordenação, filtros e HATEOAS |
| GET | `/api/consultas/{id}` | obter consulta |
| GET | `/api/consultas/pet/{idPet}` | consultar por pet |
| POST | `/api/consultas` | cadastrar |
| PUT | `/api/consultas/{id}` | atualizar |
| DELETE | `/api/consultas/{id}` | excluir |

### Medicações

| Método | Endpoint | Função |
|---|---|---|
| GET | `/api/medicacoes` | paginação, ordenação, filtros e HATEOAS |
| GET | `/api/medicacoes/{id}` | obter medicação |
| GET | `/api/medicacoes/pet/{idPet}` | consultar por pet |
| POST | `/api/medicacoes` | cadastrar |
| PUT | `/api/medicacoes/{id}` | atualizar |
| DELETE | `/api/medicacoes/{id}` | excluir |

### Histórico clínico - MongoDB

| Método | Endpoint | Função |
|---|---|---|
| GET | `/api/historicos-clinicos` | paginação, filtros e HATEOAS |
| GET | `/api/historicos-clinicos/{id}` | obter documento MongoDB |
| POST | `/api/historicos-clinicos` | cadastrar documento |
| PUT | `/api/historicos-clinicos/{id}` | atualizar documento |
| DELETE | `/api/historicos-clinicos/{id}` | excluir documento |

### Monitoramento

| Método | Endpoint | Função |
|---|---|---|
| GET | `/api/health/live` | liveness da API |
| GET | `/api/health/db` | conectividade Oracle |
| GET | `/metrics` | métricas consolidadas |

---

## Swagger / OpenAPI

Com a aplicação em execução:

```text
http://localhost:5139/swagger
```

O Swagger possui esquema **Bearer JWT**.

Fluxo recomendado:

1. execute `POST /api/tutors` para cadastrar um tutor;
2. execute `POST /api/tutors/login`;
3. copie o valor `jwt`;
4. clique em **Authorize** no Swagger;
5. informe `Bearer {token}`;
6. teste os endpoints protegidos.

---

## Monitoramento e observabilidade

### Health Checks

```http
GET /api/health/live
GET /api/health/db
```

`live` verifica o processo da API e `db` verifica a conexão Oracle.

### Logging estruturado

Serilog registra em:

- console;
- `logs/api-AAAA-MM-DD.log`;
- níveis Information, Warning e Error;
- `CorrelationId`;
- `TraceId`.

### OpenTelemetry

Tracing configurado para:

- ASP.NET Core;
- HttpClient;
- `ClyvoVet.Application`;
- `ClyvoVet.Infrastructure`.

### Métricas

O middleware mede:

- quantidade total de requisições;
- quantidade total de erros;
- taxa de erros;
- tempo médio de resposta.

Endpoint:

```http
GET /metrics
```

---

## Testes automatizados

Os testes são separados em:

```text
ClyvoVet.Tests.Unit/
ClyvoVet.Tests.Integration/
```

São utilizados:

- xUnit;
- Moq;
- padrão AAA (`Arrange`, `Act`, `Assert`);
- WebApplicationFactory;
- Fixtures e Collection Fixtures;
- EF Core InMemory;
- autenticação de teste para endpoints protegidos;
- testes específicos da Sprint 4 para JWT, HATEOAS e MongoDB/Application.

### Executar todos os testes

Na raiz:

```powershell
dotnet test .\ClyvoVet.Tests.Unit\ClyvoVet.Tests.Unit.csproj
dotnet test .\ClyvoVet.Tests.Integration\ClyvoVet.Tests.Integration.csproj
```

### Cobertura

O projeto mantém `coverlet.collector` para coleta de cobertura:

```powershell
dotnet test .\ClyvoVet.Tests.Unit\ClyvoVet.Tests.Unit.csproj --collect:"XPlat Code Coverage"
dotnet test .\ClyvoVet.Tests.Integration\ClyvoVet.Tests.Integration.csproj --collect:"XPlat Code Coverage"
```

Os resultados são gerados dentro de `TestResults`.

---

## Executando a aplicação localmente

### 1. Pré-requisitos

Instale:

- .NET SDK 8;
- Oracle Database acessível;
- MongoDB acessível (local, Docker ou Atlas);
- `dotnet-ef` caso precise manipular migrations.

### 2. Configurar credenciais Oracle

Edite:

```text
ClyvoVet.Api/appsettings.Development.json
```

A entrega deixa a estrutura da connection string pronta, porém **sem credenciais Oracle**:

```json
"OracleDbConnection": "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521))) (CONNECT_DATA=(SERVER=DEDICATED)(SID=ORCL)));User Id=;Password=;"
```

Preencha `User Id` e `Password` com as credenciais do banco que será utilizado:

```text
User Id=SEU_USUARIO;Password=SUA_SENHA;
```

Recomenda-se utilizar um usuário/schema Oracle exclusivo ou vazio para o ClyvoVet, evitando conflito com tabelas preexistentes que não estejam registradas no `__EFMigrationsHistory`.

### 3. Configurar MongoDB

O padrão local é:

```json
"Mongodb": "mongodb://localhost:27017"
```

Se utilizar MongoDB Atlas ou outro servidor, substitua pela connection string correspondente.

### 4. Configurar a chave JWT

Para uso acadêmico local há uma chave de desenvolvimento configurada. Para outro ambiente, substitua:

```json
"Secretkey": "SUA_CHAVE_SEGURA"
```

ou utilize a variável de ambiente:

```text
Secretkey
```

### 5. Restaurar dependências

Na raiz do repositório:

```powershell
dotnet restore .\ClyvoVet.Api\ClyvoVet.Api.csproj
```

### 6. Executar

```powershell
dotnet run --project .\ClyvoVet.Api\ClyvoVet.Api.csproj
```

A aplicação aplica as migrations Oracle pendentes durante a inicialização, exceto no ambiente `Testing`.

### 7. Validar

```text
Swagger:       http://localhost:5139/swagger
Health API:    http://localhost:5139/api/health/live
Health Oracle: http://localhost:5139/api/health/db
Metrics:       http://localhost:5139/metrics
```

---

## Docker

O `docker-compose.yml` contém:

- Oracle;
- MongoDB;
- API ClyvoVet.

Variáveis esperadas:

```text
ORACLE_ROOT_PASSWORD
ORACLE_APP_USER
ORACLE_APP_PASSWORD
JWT_SECRET_KEY
```

Depois:

```bash
docker compose up -d
```

---

## Migrations

A pasta:

```text
ClyvoVet.Api/Infrastructure/Data/Migrations
```

contém a migration relacional do Oracle.

Para gerar uma nova migration:

```powershell
dotnet ef migrations add NomeDaMigration --project .\ClyvoVet.Api\ClyvoVet.Api.csproj --startup-project .\ClyvoVet.Api\ClyvoVet.Api.csproj --context ApplicationContext --output-dir Infrastructure\Data\Migrations
```

Para gerar o script SQL:

```powershell
dotnet ef migrations script --project .\ClyvoVet.Api\ClyvoVet.Api.csproj --startup-project .\ClyvoVet.Api\ClyvoVet.Api.csproj --context ApplicationContext --output script_bd.sql
```

As migrations EF Core se aplicam ao banco **relacional Oracle**. O MongoDB não utiliza as migrations relacionais do EF Core.

---

## Integrantes

- **Enzo Monteiro Maciel** - RM: 563734
- **Matheus de Almeida Sousa** - RM: 563557
- **Paulo Estalise** - RM: 563811
- **Gabriel Bebé Silva** - RM: 562012
- **Emanuel Italo** - RM: 561337
