# ClyvoVet - Backend API & Infraestrutura DevOps

## 📌 Sobre o Projeto

O ecossistema **ClyvoVet** foi projetado para suportar o gerenciamento e o monitoramento de saúde de pets, permitindo o cadastro de usuários e animais e fornecendo uma API RESTful para consulta e manutenção dessas informações.

O backend foi desenvolvido em **C# com .NET 8**, utilizando **Entity Framework Core** e integração com banco de dados **Oracle**.

Nesta etapa do projeto foram adicionados recursos de **monitoramento, observabilidade e testes automatizados**, incluindo:

* Health Checks para acompanhamento da saúde da API e do banco Oracle;
* Logging estruturado utilizando Serilog;
* Correlação de requisições através de `CorrelationId`;
* Distributed Tracing utilizando OpenTelemetry;
* Métricas de desempenho e taxa de erros;
* Documentação dos endpoints utilizando Swagger;
* Testes unitários com xUnit, Moq e Entity Framework Core InMemory;
* Testes de integração utilizando `WebApplicationFactory`;
* Separação dos testes em projetos **Unit** e **Integration**;
* Organização da aplicação seguindo separação entre Presentation, Application, Domain e Infrastructure.

---

## 🛠️ Tecnologias e Funcionalidades

O backend foi estruturado seguindo boas práticas de desenvolvimento, separação de responsabilidades e observabilidade.

### Backend

* **C#**
* **.NET 8**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **Oracle.EntityFrameworkCore**

### Banco de Dados

* **Oracle Database**
* Code-First Migrations;
* Aplicação automática das migrations durante a inicialização;
* Relacionamento entre usuários/tutores e pets.

### Arquitetura

A aplicação está organizada nas seguintes camadas:

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
│   │   ├── AppData/
│   │   └── Repositories/
│   ├── HealthChecks/
│   ├── Observability/
│   └── IoC/
│
├── Presentation/
│   └── Controllers/
│
└── Program.cs
```

Cada camada possui uma responsabilidade específica:

* **Presentation:** recebe as requisições HTTP e retorna as respostas da API;
* **Application:** coordena os casos de uso e regras de aplicação;
* **Domain:** contém as entidades e contratos centrais do sistema;
* **Infrastructure:** concentra persistência, repositórios, observabilidade e configurações técnicas.

---

# 📊 Monitoramento e Observabilidade

A aplicação possui mecanismos de monitoramento para permitir o acompanhamento de sua disponibilidade, dependências e comportamento durante a execução.

## ❤️ Health Checks

Os Health Checks permitem verificar se a aplicação está em execução e se possui conectividade com o banco de dados Oracle.

### Health geral

```http
GET /health
```

Executa os Health Checks registrados na aplicação.

### Liveness

```http
GET /health/live
```

Verifica se o processo da API está ativo.

Esse endpoint é autocontido e não depende do banco de dados.

Resultado esperado quando a aplicação está funcionando:

```text
HTTP 200 OK
```

### Banco de dados Oracle

```http
GET /health/db
```

Verifica a disponibilidade e a conectividade da aplicação com o Oracle.

Quando a conexão com o banco está disponível:

```text
HTTP 200 OK
```

Quando o Oracle está indisponível ou a conexão não pode ser estabelecida:

```text
HTTP 503 Service Unavailable
```

Também estão disponíveis endpoints documentados pelo controller de Health Check:

```http
GET /api/health2/live
GET /api/health2/db
```

Esses endpoints retornam informações estruturadas sobre o resultado da verificação.

---

## 🔎 Como monitorar a aplicação

Durante a execução local é possível verificar a aplicação diretamente pelo navegador, Swagger ou por ferramentas como Postman.

Exemplos:

```text
http://localhost:5139/health/live
http://localhost:5139/health/db
```

ou utilizando HTTPS:

```text
https://localhost:7011/health/live
https://localhost:7011/health/db
```

As portas podem variar de acordo com o perfil configurado em `launchSettings.json`.

Uma forma simples de testar o comportamento do monitoramento é:

1. Iniciar a aplicação;
2. Consultar `/health/live`;
3. Consultar `/health/db`;
4. Interromper temporariamente o Oracle;
5. Consultar novamente os endpoints.

Nesse cenário, o comportamento esperado é:

```text
/health/live → continua retornando 200
/health/db   → retorna 503 enquanto o Oracle estiver indisponível
```

Isso permite diferenciar a disponibilidade da própria API da disponibilidade de uma de suas dependências.

---

# 📝 Logging Estruturado

A aplicação utiliza **Serilog** para geração de logs estruturados.

Os principais níveis utilizados são:

* `Information` — eventos normais da aplicação;
* `Warning` — situações inesperadas que não impedem a execução;
* `Error` — erros e exceções ocorridos durante o processamento.

Os logs são enviados para:

```text
Console
```

e para arquivos locais:

```text
logs/
```

---

## 🔗 Correlação de requisições

Cada requisição recebe um identificador de correlação.

O valor pode ser observado no header:

```http
X-Correlation-ID
```

Esse identificador também é incluído nos logs, permitindo acompanhar todos os eventos relacionados a uma mesma requisição.

Exemplo:

```text
Requisição HTTP
      ↓
CorrelationId
      ↓
Controller
      ↓
UseCase
      ↓
Repository
      ↓
Logs relacionados
```

Essa abordagem facilita rastreamento e diagnóstico de erros.

---

# 🔭 Distributed Tracing

O projeto utiliza **OpenTelemetry** para rastrear requisições entre as camadas da aplicação.

O tracing acompanha o fluxo das operações entre componentes como:

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

São utilizadas fontes de tracing específicas para as camadas da aplicação e infraestrutura, permitindo identificar o caminho percorrido pelas requisições.

---

# 📈 Métricas

A aplicação coleta métricas relacionadas ao comportamento das requisições.

Entre os indicadores monitorados estão:

* quantidade total de requisições;
* quantidade de erros;
* taxa de erros;
* tempo médio de resposta.

As métricas podem ser consultadas através de:

```http
GET /metrics
```

Essas informações ajudam a identificar problemas de desempenho e aumento da taxa de erros.

---

# 📚 Swagger

A API disponibiliza documentação interativa através do Swagger.

Ao executar o projeto em ambiente de desenvolvimento, acesse:

```text
https://localhost:7011/swagger
```

ou:

```text
http://localhost:5139/swagger
```

A documentação dos endpoints utiliza Swagger Annotations, incluindo:

* `Summary`;
* `Description`;
* códigos de resposta HTTP;
* tipos de retorno.

O Swagger pode ser utilizado para executar diretamente operações de cadastro, consulta, alteração e exclusão disponíveis na API.

---

# 🧪 Testes Automatizados

Os testes automatizados utilizam:

* **xUnit**
* **Moq**
* **Entity Framework Core InMemory**
* **Microsoft.AspNetCore.Mvc.Testing**
* **WebApplicationFactory**
* **Coverlet**

Os testes seguem o padrão **AAA**:

```text
Arrange
Act
Assert
```

A nomenclatura segue o padrão:

```text
MetodoTestado_Cenario_ResultadoEsperado
```

Exemplo:

```text
ObterUmPetAsync_PetExistente_DeveRetornarPet
```

---

## 🗂️ Organização dos testes

Os testes estão separados em dois projetos.

```text
ClyvoVet.Tests.Unit/
├── Domain/
│   └── EntityTest.cs
│
├── Application/
│   ├── PetUseCaseTest.cs
│   └── UserUseCaseTest.cs
│
└── Infrastructure/
    ├── PetRepositoryTest.cs
    └── UserRepositoryTest.cs
```

Os testes unitários verificam individualmente as regras e comportamentos das camadas da aplicação.

Os testes da camada Application utilizam **Moq** para simular as dependências.

Os testes de Repository utilizam **Entity Framework Core InMemory**, evitando a necessidade de acessar o banco Oracle real durante sua execução.

Os testes de integração ficam separados em:

```text
ClyvoVet.Tests.Integration/
├── Controllers/
│   ├── PetControllerTest.cs
│   └── UserControllerTest.cs
│
├── Fixtures/
│   └── CustomWebApplicationFactory.cs
│
└── HealthChecks/
    └── HealthCheckTest.cs
```

Os testes de integração utilizam:

```text
WebApplicationFactory<Program>
```

para executar a aplicação em memória e realizar requisições HTTP contra os endpoints reais da API.

Também são utilizadas **Fixtures** e **Collection Fixtures** para compartilhar o contexto da aplicação entre os testes.

---

# ▶️ Executando os testes

Os testes podem ser executados pelo Visual Studio através do **Test Explorer** ou pelo terminal utilizando `dotnet test`.

## Executar todos os testes da solução

Na pasta raiz do projeto:

```bash
dotnet test
```

Esse comando executa os projetos de testes presentes na solução.

---

## Executar somente os testes unitários

```bash
dotnet test ./ClyvoVet.Tests.Unit/ClyvoVet.Tests.Unit.csproj
```

---

## Executar somente os testes de integração

```bash
dotnet test ./ClyvoVet.Tests.Integration/ClyvoVet.Tests.Integration.csproj
```



---

# 💻 Executando a aplicação localmente

## Pré-requisitos

Para executar o projeto localmente são necessários:

* .NET 8 SDK;
* Visual Studio 2022 ou outra IDE compatível;
* Docker Desktop, caso o Oracle seja executado em container;
* Oracle Database configurado e disponível para conexão.

Após configurar a connection string da aplicação, execute o projeto:

```bash
dotnet run --project ./ClyvoVet.API/ClyvoVet.API.csproj
```

Ou execute diretamente pelo Visual Studio utilizando:

```text
F5
```

Após a inicialização, o Swagger poderá ser utilizado para testar os endpoints disponíveis.

---

# 🏗️ Arquitetura de Nuvem

A infraestrutura do projeto utiliza recursos em Microsoft Azure e conteinerização.

* **Provedor Cloud:** Microsoft Azure
* **Backend:** C# .NET 8 - API RESTful
* **Banco de Dados:** Oracle Database Free
* **Registro de Imagens:** Azure Container Registry - ACR
* **Servidor:** Azure Virtual Machine Linux/Ubuntu
* **Automação:** GitHub Actions
* **Containerização:** Docker

---

# 🚀 Fluxo de Deploy Contínuo - CI/CD

O processo de deploy foi automatizado utilizando GitHub Actions.

O fluxo inclui:

1. Validação do build da aplicação;
2. Execução dos testes automatizados;
3. Criação da imagem Docker;
4. Envio da imagem para o Azure Container Registry;
5. Acesso à VM através de SSH;
6. Atualização do container da aplicação;
7. Aplicação das migrations do Entity Framework Core.

A separação dos testes permite executar individualmente:

```text
ClyvoVet.Tests.Unit
```

e:

```text
ClyvoVet.Tests.Integration
```

durante o processo de CI.

---

# 🛠️ Como Provisionar a Infraestrutura do Zero

Para replicar o ambiente:

1. Configure as variáveis de ambiente no arquivo `.env`;
2. Configure as credenciais do Oracle e Azure;
3. Dê permissão de execução ao script:

```bash
chmod +x deploy-interativo.sh
```

4. Execute:

```bash
./deploy-interativo.sh
```

O script realiza o provisionamento dos recursos necessários, instala o Docker e inicia a stack da aplicação.

O Entity Framework Core realiza a aplicação das migrations necessárias durante a inicialização da API.

---


## 👥 Integrantes do Grupo

* **Enzo Monteiro Maciel** - RM: 563734
* **Matheus de Almeida Sousa** - RM: 563557
* **Paulo Estalise** - RM: 563811
* **Gabriel Bebé Silva** - RM: 562012
* **Emanuel Italo** - RM:561337

---