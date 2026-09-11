# ClyvoVet - Backend API & Infraestrutura DevOps

## 📌 Sobre o Projeto

O **ClyvoVet** é uma API RESTful desenvolvida em **C# com .NET 8** para gerenciamento de informações relacionadas a tutores, pets, consultas veterinárias e medicações.

A aplicação utiliza **ASP.NET Core Web API**, **Entity Framework Core** e banco de dados **Oracle**, seguindo uma organização em camadas para separar regras de domínio, casos de uso, persistência e apresentação.

Nesta Sprint 3 o projeto foi evoluído principalmente nos seguintes pontos:

- monitoramento da aplicação através de **Health Checks**;
- logging estruturado com **Serilog**;
- correlação de requisições com `CorrelationId` e `TraceId`;
- distributed tracing com **OpenTelemetry**;
- métricas de desempenho e taxa de erros;
- documentação dos endpoints com **Swagger Annotations**;
- testes unitários com **xUnit**, **Moq** e **Entity Framework Core InMemory**;
- testes de integração com **WebApplicationFactory**;
- separação dos testes em projetos **Unit** e **Integration**;
- autenticação de tutores por e-mail e senha;
- persistência das entidades `TUTOR`, `PET`, `CONSULTA` e `MEDICACAO` no Oracle.

---

## 🐾 Modelo de Dados

O modelo atual da aplicação é composto pelas entidades:

```text
TUTOR
  ├── autenticação por EMAIL + SENHA
  └── 1:N PET
          ├── 1:N CONSULTA
          └── 1:N MEDICACAO
```

### Tutor

Representa o responsável pelos pets cadastrados na aplicação.

Principais campos:

- `IdTutor`
- `Nome`
- `Email`
- `Telefone`
- `Cpf`
- `Senha`

A autenticação é realizada diretamente pela entidade `Tutor`. A entidade `User` não faz mais parte do modelo atual.

A propriedade de senha possui `JsonIgnore`, portanto a senha não é devolvida nas respostas JSON da API.

### Pet

Cada pet pertence a um tutor.

Principais campos:

- `IdPet`
- `IdTutor`
- `Nome`
- `Especie`
- `Raca`
- `DataNascimento`
- `PesoKg`

### Consulta

Representa uma consulta veterinária associada a um pet.

Principais campos:

- `IdConsulta`
- `IdPet`
- `DataConsulta`
- `Veterinario`
- `Observacoes`

### Medicação

Representa uma medicação associada a um pet.

Principais campos:

- `IdMedicacao`
- `IdPet`
- `Nome`
- `Dose`
- `Frequencia`
- `DataInicio`
- `DataFim`

---

## 🛠️ Tecnologias Utilizadas

### Backend

- C#
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- Oracle.EntityFrameworkCore

### Monitoramento e Observabilidade

- Microsoft Health Checks
- AspNetCore.HealthChecks.Oracle
- Serilog
- OpenTelemetry
- System.Diagnostics.Metrics

### Documentação

- Swagger / OpenAPI
- Swashbuckle.AspNetCore.Annotations

### Testes

- xUnit
- Moq
- Entity Framework Core InMemory
- Microsoft.AspNetCore.Mvc.Testing
- WebApplicationFactory


### Infraestrutura

- Docker
- Oracle Database
- Microsoft Azure
- Azure Container Registry
- GitHub Actions

---

## 🏗️ Arquitetura da Aplicação

A API está organizada nas seguintes camadas:

```text
ClyvoVet.API/
├── Application/
│   ├── Dtos/
│   ├── Interfaces/
│   ├── Mappers/
│   └── UseCases/
│
├── Domain/
│   ├── Entities/
│   └── Interfaces/
│
├── Infrastructure/
│   ├── Data/
│   │   ├── Migrations/
│   │   └── Repositories/
│   ├── HealthChecks/
│   ├── IoC/
│   │   └── Bootstrap.cs
│   └── Observability/
│
├── Presentation/
│   └── Controllers/
│
└── Program.cs
```

### Responsabilidade das camadas

- **Presentation:** recebe requisições HTTP, chama os casos de uso e produz as respostas da API.
- **Application:** contém DTOs, mapeamentos, interfaces e UseCases responsáveis pelo fluxo das operações.
- **Domain:** contém as entidades e contratos dos repositories.
- **Infrastructure:** contém acesso ao Oracle, repositories, migrations, Health Checks, métricas e registro de dependências.

### IoC / Bootstrap

A pasta `Infrastructure/IoC` contém o `Bootstrap.cs`, utilizado para centralizar o registro das dependências da aplicação.

O `Program.cs` chama:

```csharp
builder.Services.AddClyvoVetInfrastructure(builder.Configuration);
```

O `Bootstrap.cs` registra:

- `ApplicationContext`;
- repositories de Tutor, Pet, Consulta e Medicação;
- UseCases correspondentes;
- serviço `ApiMetrics`.

Essa organização evita concentrar todos os registros de dependência diretamente no `Program.cs`.

---

# 🔐 Autenticação de Tutores

A aplicação utiliza o próprio cadastro de `Tutor` para validação de credenciais.

## Cadastro

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

## Login

```http
POST /api/tutors/login
```

Exemplo:

```json
{
  "email": "tutor@teste.com",
  "senha": "123456"
}
```

O fluxo de login é:

```text
TutorsController
      ↓
TutorUseCase
      ↓
TutorRepository
      ↓
Oracle - TUTOR
```

Respostas principais:

- `200 OK`: credenciais válidas;
- `401 Unauthorized`: e-mail ou senha incorretos;
- `400 Bad Request`: dados inválidos ou erro durante a operação;
- `429 Too Many Requests`: limite de tentativas excedido.

---

# 📚 Endpoints da API

## Tutores

| Método | Endpoint | Descrição |
| --- | --- | --- |
| POST | `/api/tutors` | Cadastra um tutor |
| POST | `/api/tutors/login` | Valida as credenciais do tutor |
| GET | `/api/tutors` | Lista os tutores cadastrados |
| GET | `/api/tutors/{id}` | Busca um tutor por ID |
| PUT | `/api/tutors/{id}` | Atualiza um tutor |
| DELETE | `/api/tutors/{id}` | Exclui um tutor |

## Pets

| Método | Endpoint | Descrição |
| --- | --- | --- |
| POST | `/api/pets` | Cadastra um pet |
| GET | `/api/pets` | Lista todos os pets |
| GET | `/api/pets/{id}` | Busca um pet por ID |
| GET | `/api/pets/tutor/{idTutor}` | Lista os pets filtrados por tutor |
| GET | `/api/pets/especie/{especie}` | Lista os pets filtrados por espécie |
| PUT | `/api/pets/{id}` | Atualiza um pet |
| DELETE | `/api/pets/{id}` | Exclui um pet |

## Consultas

| Método | Endpoint | Descrição |
| --- | --- | --- |
| POST | `/api/consultas` | Cadastra uma consulta |
| GET | `/api/consultas` | Lista todas as consultas |
| GET | `/api/consultas/{id}` | Busca uma consulta por ID |
| GET | `/api/consultas/pet/{idPet}` | Lista as consultas de um pet |
| PUT | `/api/consultas/{id}` | Atualiza uma consulta |
| DELETE | `/api/consultas/{id}` | Exclui uma consulta |


## Medicações

| Método | Endpoint | Descrição |
| --- | --- | --- |
| POST | `/api/medicacoes` | Cadastra uma medicação |
| GET | `/api/medicacoes` | Lista todas as medicações |
| GET | `/api/medicacoes/{id}` | Busca uma medicação por ID |
| GET | `/api/medicacoes/pet/{idPet}` | Lista as medicações de um pet |
| PUT | `/api/medicacoes/{id}` | Atualiza uma medicação |
| DELETE | `/api/medicacoes/{id}` | Exclui uma medicação |

---

# 📊 Monitoramento e Observabilidade

## ❤️ Health Checks

A aplicação utiliza `Microsoft.Extensions.Diagnostics.HealthChecks` para verificar a disponibilidade da própria API e a conectividade com o banco Oracle. Os Health Checks são expostos exclusivamente pelo `HealthController`, evitando endpoints duplicados no `Program.cs`.

### Liveness da API

```http
GET /api/health/live
```

Executa somente o Health Check registrado com a tag `live` e verifica se a API está em execução sem depender da conexão com o banco.

Quando saudável:

```text
HTTP 200 OK
```

### Banco Oracle

```http
GET /api/health/db
```

Executa somente o Health Check registrado com a tag `db` e verifica se a aplicação consegue se conectar ao Oracle.

Resultados esperados:

```text
Oracle disponível   → HTTP 200 OK
Oracle indisponível → HTTP 503 Service Unavailable
```

Os endpoints utilizam `HealthCheckService`, retornam informações estruturadas sobre as verificações e são documentados no Swagger.

> A versão atual do ClyvoVet não integra uma dependência HTTP externa; por isso não existe Health Check de serviço externo nesta implementação.

---

## 🔎 Como monitorar a aplicação

Com a API em execução localmente:

### HTTP

```text
http://localhost:5139/api/health/live
http://localhost:5139/api/health/db
http://localhost:5139/metrics
```

### HTTPS

```text
https://localhost:7011/api/health/live
https://localhost:7011/api/health/db
https://localhost:7011/metrics
```

Uma forma de validar o monitoramento do banco é:

1. iniciar a API com o Oracle disponível;
2. consultar `/api/health/live` e `/api/health/db`;
3. interromper temporariamente o Oracle;
4. consultar novamente os endpoints.

O comportamento esperado é:

```text
/api/health/live → continua respondendo 200
/api/health/db   → responde 503 enquanto o Oracle estiver indisponível
```

---

# 📝 Logging Estruturado

A aplicação utiliza **Serilog**.

Os níveis utilizados incluem:

- `Information` para operações normais;
- `Warning` para situações que exigem atenção;
- `Error` para falhas e exceções.

Os logs são enviados para:

- console;
- arquivos com rotação diária no diretório `logs` da aplicação.

O padrão de arquivo é semelhante a:

```text
logs/api-AAAAMMdd.log
```


---

## 🔗 Correlação de Requisições

Cada requisição recebe um identificador através do header:

```http
X-Correlation-ID
```

Caso o cliente não envie esse header, a aplicação gera um novo identificador.

O valor é incluído na resposta e também é registrado pelo Serilog juntamente com o `TraceId`.

Fluxo:

```text
Requisição HTTP
      ↓
CorrelationId / TraceId
      ↓
Controller
      ↓
UseCase
      ↓
Repository
      ↓
Logs e traces relacionados
```

O request logging do Serilog utiliza os níveis:

```text
2xx / 3xx → Information
4xx       → Warning
5xx       → Error
```

---

# 🔭 Distributed Tracing com OpenTelemetry

O projeto utiliza **OpenTelemetry** para rastrear o fluxo das requisições.

Instrumentações configuradas:

- ASP.NET Core;
- HttpClient;
- `ClyvoVet.Application`;
- `ClyvoVet.Infrastructure`.

Fluxo conceitual:

```text
ASP.NET Core
      ↓
Controller
      ↓
Application / UseCase
      ↓
Infrastructure / Repository
      ↓
Oracle
```

Os UseCases e repositories utilizam `ActivitySource`, permitindo acompanhar a execução entre as camadas.

O exportador de console pode ser controlado pela configuração:

```json
{
  "Observability": {
    "EnableConsoleExporter": true
  }
}
```

---

# 📈 Métricas

A classe `ApiMetrics` registra métricas da API através de `System.Diagnostics.Metrics` e OpenTelemetry.

São coletados:

- total de requisições;
- total de erros HTTP;
- taxa de erros;
- tempo médio de resposta;
- histograma de tempo de resposta.

As métricas resumidas podem ser consultadas por:

```http
GET /metrics
```

Exemplo de estrutura retornada:

```json
{
  "totalRequests": 10,
  "totalErrors": 1,
  "errorRatePercent": 10.0,
  "averageResponseTimeMs": 25.3
}
```

---

# 📚 Swagger

A documentação interativa está disponível em ambiente de desenvolvimento.

### HTTPS

```text
https://localhost:7011/swagger
```

### HTTP

```text
http://localhost:5139/swagger
```

Os controllers utilizam `SwaggerOperation` e `SwaggerResponse`.

As descrições incluem, conforme aplicável:

- breve descrição do endpoint;
- dados utilizados;
- fluxo de processamento;
- observações importantes;
- códigos de resposta HTTP.

---

# ⏱️ Rate Limiting

A aplicação possui uma política de limite de requisições chamada:

```text
politica_5_tentativas
```

Configuração atual:

- 5 requisições por janela;
- janela de 20 segundos;
- fila de até 2 requisições;
- retorno `429 Too Many Requests` quando o limite é excedido.

Essa política é aplicada, entre outros pontos, ao login de Tutor e a endpoints de listagem configurados no projeto.

---

# 🧪 Testes Automatizados

Os testes foram separados em dois projetos independentes:

```text
ClyvoVet.Tests.Unit
ClyvoVet.Tests.Integration
```

A versão atual possui:

```text
28 testes Unit
17 testes Integration
45 testes no total
```

Todos os testes seguem explicitamente o padrão **AAA**:

```text
Arrange
Act
Assert
```

A nomenclatura segue o padrão:

```text
MetodoTestado_Cenario_ResultadoEsperado
```

---

## ✅ Testes Unitários

Estrutura:

```text
ClyvoVet.Tests.Unit/
├── Domain/
│   └── EntityTest.cs
│
├── Application/
│   ├── TutorUseCaseTest.cs
│   ├── PetUseCaseTest.cs
│   ├── ConsultaUseCaseTest.cs
│   └── MedicacaoUseCaseTest.cs
│
└── Infrastructure/
    ├── TutorRepositoryTest.cs
    ├── PetRepositoryTest.cs
    ├── ConsultaRepositoryTest.cs
    └── MedicacaoRepositoryTest.cs
```

Nos testes de Application, as dependências são isoladas com **Moq**.

Nos testes de Repository é utilizado **Entity Framework Core InMemory**, permitindo testar a persistência sem acessar o Oracle real.

---

## 🔄 Testes de Integração

Estrutura:

```text
ClyvoVet.Tests.Integration/
├── Controllers/
│   ├── TutorControllerTest.cs
│   ├── PetControllerTest.cs
│   ├── ConsultaControllerTest.cs
│   └── MedicacaoControllerTest.cs
│
├── Fixtures/
│   └── CustomWebApplicationFactory.cs
│
└── HealthChecks/
    └── HealthCheckTest.cs
```

Os testes utilizam:

```text
WebApplicationFactory<Program>
```

para subir a API em memória e realizar requisições HTTP.

São cobertos cenários como:

- cadastro com sucesso;
- consulta de registros;
- recursos inexistentes;
- respostas `200`, `201`, `204`, `400`, `401` e `404`;
- login com credenciais válidas;
- login com credenciais inválidas;
- Health Checks.

A suíte utiliza tanto `IClassFixture` quanto `ICollectionFixture` para compartilhar o contexto da aplicação entre os testes.

---

# ▶️ Como Executar os Testes

A partir da pasta raiz do repositório, os testes podem ser executados com `dotnet test`.

## Testes Unitários

```bash
dotnet test ./ClyvoVet.Tests.Unit/ClyvoVet.Tests.Unit.csproj
```

## Testes de Integração

```bash
dotnet test ./ClyvoVet.Tests.Integration/ClyvoVet.Tests.Integration.csproj
```


Os testes também podem ser executados através do **Test Explorer** do Visual Studio.

---

# 💻 Executando a Aplicação Localmente

## Pré-requisitos

- .NET 8 SDK;
- Visual Studio 2022 ou outra IDE compatível;
- Oracle Database acessível;
- Docker Desktop, caso o Oracle seja executado em container.

Configure a connection string Oracle e execute:

```bash
dotnet run --project ./ClyvoVet.API/ClyvoVet.API.csproj
```

Ou execute pelo Visual Studio utilizando `F5`.

Os perfis atuais utilizam:

```text
HTTP  → http://localhost:5139
HTTPS → https://localhost:7011
```

O Swagger é aberto automaticamente pelo `launchSettings.json`.

---

# 🐳 Docker

O projeto possui `docker-compose.yml` com:

- container Oracle;
- container da API;
- rede dedicada;
- volume persistente do Oracle;


As credenciais do banco devem ser fornecidas por variáveis de ambiente, como:

```text
ORACLE_ROOT_PASSWORD
ORACLE_APP_USER
ORACLE_APP_PASSWORD
```

---

# 🏗️ Infraestrutura de Nuvem

A estrutura de DevOps do projeto contempla:

- Microsoft Azure;
- Azure Container Registry;
- Azure Virtual Machine;
- Docker;
- GitHub Actions;
- deploy da imagem da API em container.

O repositório possui workflows separados para CI e deploy. A execução local dos testes continua disponível pelos comandos `dotnet test` documentados acima.

---

## 👥 Integrantes do Grupo

- **Enzo Monteiro Maciel** - RM: 563734
- **Matheus de Almeida Sousa** - RM: 563557
- **Paulo Estalise** - RM: 563811
- **Gabriel Bebé Silva** - RM: 562012
- **Emanuel Italo** - RM: 561337
