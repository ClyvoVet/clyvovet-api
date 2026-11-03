# ClyvoVet - Entrega de QA (Sprint 4)

Compliance, Quality Assurance & Tests - Challenge FIAP 2026

## Integrantes

| Nome | RM |
|---|---|
| Enzo Monteiro Maciel | 563734 |
| Matheus de Almeida Sousa | 563557 |
| Paulo Henrique Alves Estalise | 563811 |
| Gabriel Bebé Silva | 562012 |
| Emanuel Ítalo Leal Trindade Soares | 561337 |

## Links da entrega

| Item | Link |
|---|---|
| Azure Boards (plano de testes manuais) | https://dev.azure.com/Challenge-FIAP-2TDSPO/Sprint%204%20%E2%80%93%20Azure%20DevOps/_boards |
| Vídeo da automação de testes | `https://www.youtube.com/watch?v=J_4eZKUO8Ow` |
| Repositório (branch develop) | https://github.com/ClyvoVet/clyvovet-api/tree/develop |

## Parte A - Testes manuais (Azure Boards)

Os testes estão em **Boards → Work items**. Para listar só eles, busque por `TC-` no título. Cada teste descreve objetivo, dados de entrada, procedimento (passos) e dados de saída esperados, com dados controlados. Estão ligados ao PBI que validam e seguem o release plan (Sprint 2, 3 e 4).

| Teste | Valida | PBI | Sprint |
|---|---|---|---|
| TC-01 | Cadastrar tutor com dados válidos | 10 | 2 |
| TC-02 | Rejeitar cadastro com e-mail duplicado | 10 | 2 |
| TC-03 | Login com credenciais válidas | 28 | 4 |
| TC-04 | Login com senha incorreta | 28 | 4 |
| TC-05 | Rota protegida sem e com token | 28 | 4 |
| TC-06 | Cadastrar pet vinculado ao tutor | 12 | 2 |
| TC-07 | Atualizar e consultar pet | 12 | 2 |
| TC-08 | Remover pet | 12 | 2 |
| TC-09 | Listagem paginada com filtro e ordenação | 29 | 4 |
| TC-10 | Registrar consulta veterinária | 6 | 3 |
| TC-11 | Registrar histórico clínico (MongoDB) | 30 | 4 |
| TC-12 | Verificar saúde da plataforma | 17 | 3 |
| TC-13 | Deploy automático a cada commit na main | 31 | 4 |
| TC-14 | Listar os pets do tutor no app | 23 | 4 |
| TC-15 | Responder dúvida de saúde do pet com a IA | 15 | 4 |
| TC-16 | Responder dúvida de saúde do pet por voz | 15 | 4 |
| TC-17 | Cadastrar um pet pelo app | 12 | 4 |

As evidências de persistência são conferidas com `SELECT` no Oracle.

## Parte B - Automação de testes (Postman)

- **Ferramenta:** Postman (a API não possui interface própria; o app mobile é um projeto separado).
- **Arquivo:** [`ClyvoVet.postman_collection.json`](ClyvoVet.postman_collection.json)
- **Conteúdo:** 14 requisições e 26 asserções, na ordem de execução: saúde, autenticação JWT, CRUD de pets com paginação e filtros e limpeza dos dados.

| Teste | Descrição |
|---|---|
| AT-01 | Health check do banco Oracle (inclui tempo de resposta) |
| AT-02 | Cadastrar tutor com dados válidos |
| AT-03 | Rejeitar cadastro com e-mail duplicado (409) |
| AT-04 | Login com credenciais válidas (guarda o JWT) |
| AT-05 | Login com senha incorreta (401) |
| AT-06 | Rota protegida sem token (401) |
| AT-07 | Rota protegida com token (200) |
| AT-08 | Cadastrar pet vinculado ao tutor |
| AT-09 | Consultar pet por identificador |
| AT-10 | Listar pets com paginação e filtros (inclui tempo de resposta) |
| AT-11 | Atualizar dados do pet |
| AT-12 | Remover pet |
| AT-13 | Remover tutor de teste |
| AT-14 | Tutor removido não é mais encontrado (404) |

### Como executar

No Postman: **Import** do arquivo, ajustar a variável `baseUrl` para o endereço da API e usar **Run collection**.

Pela linha de comando:

```bash
npm install -g newman
newman run qa/ClyvoVet.postman_collection.json --env-var "baseUrl=http://<endereco-da-api>:5139"
```

Os identificadores, o e-mail e o CPF são gerados a cada execução, então a coleção pode ser executada várias vezes.

## Ambiente

A API e os bancos executam em containers no Azure Container Instances, criados pela pipeline do Azure Pipelines a cada commit na branch `main`. O ambiente é provisionado sob demanda e removido depois dos testes para preservar os créditos da assinatura. Os detalhes da esteira estão no `README.md` principal, na seção de CI/CD.