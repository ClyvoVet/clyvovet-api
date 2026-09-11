#!/bin/bash
# ==============================================================================
#  ██████╗██╗  ██╗   ██╗██╗   ██╗ ██████╗     ██╗   ██╗███████╗████████╗
# ██╔════╝██║  ╚██╗ ██╔╝██║   ██║██╔═══██╗    ██║   ██║██╔════╝╚══██╔══╝
# ██║     ██║   ╚████╔╝ ██║   ██║██║   ██║    ██║   ██║█████╗     ██║
# ██║     ██║    ╚██╔╝  ╚██╗ ██╔╝██║   ██║    ╚██╗ ██╔╝██╔══╝     ██║
# ╚██████╗███████╗██║    ╚████╔╝ ╚██████╔╝     ╚████╔╝ ███████╗   ██║
#  ╚═════╝╚══════╝╚═╝     ╚═══╝   ╚═════╝       ╚═══╝  ╚══════╝   ╚═╝
#
# PROJETO   : ClyvoVet -- Infraestrutura em Nuvem para Clinicas Veterinarias
# DESCRICAO : Provisioner para Azure Container Instances (ACR + ACI)
# STACK     : .NET 8 API | Oracle DB (gvenzl/oracle-free) | ACR | ACI
# VERSAO    : 3.0.0
# ==============================================================================

# -- Modo estrito: aborta em erros, variaveis nao definidas e falhas em pipes --
set -euo pipefail

# ==============================================================================
# SECAO 1 -- CONSTANTES GLOBAIS
# ==============================================================================

readonly SCRIPT_VERSION="3.0.0"
readonly TEMP_DIR="$(mktemp -d /tmp/clyvovet-deploy.XXXXXX)"
readonly ACI_YAML_FILE="${TEMP_DIR}/aci-container-group.yaml"
readonly ACI_OUTPUT_FILE="${TEMP_DIR}/aci-output.json"
readonly LOG_FILE="${TEMP_DIR}/deploy.log"

# Porta publica exposta pelo container group ACI
readonly API_PORT=5139

# Dependencias obrigatorias para execucao do script
readonly -a REQUIRED_CMDS=("az" "jq" "curl")

# -- Paleta de Cores ANSI -----------------------------------------------------
readonly C_RESET='\033[0m'
readonly C_BOLD='\033[1m'
readonly C_GREEN='\033[1;32m'
readonly C_BLUE='\033[1;34m'
readonly C_CYAN='\033[1;36m'
readonly C_YELLOW='\033[1;33m'
readonly C_RED='\033[1;31m'
readonly C_MAGENTA='\033[1;35m'
readonly C_WHITE='\033[1;37m'
readonly C_DIM='\033[2m'

# -- Simbolos do tema ClyvoVet ------------------------------------------------
readonly SYM_PAW="🐾"
readonly SYM_VET="🏥"
readonly SYM_SYRINGE="💉"
readonly SYM_CLOUD="☁️"
readonly SYM_ROCKET="🚀"
readonly SYM_LOCK="🔒"
readonly SYM_KEY="🔑"
readonly SYM_CHECK="✅"
readonly SYM_WARN="⚠️"
readonly SYM_GLOBE="🌐"

# ==============================================================================
# SECAO 1.5 -- FORCADO ENCODING UTF-8 PARA O AZURE CLI (Python)
# ==============================================================================
export PYTHONIOENCODING="utf-8"
export PYTHONUTF8=1
export LANG="en_US.UTF-8"
export LC_ALL="en_US.UTF-8"

# ==============================================================================
# SECAO 2 -- FUNCOES UTILITARIAS (LOGGING E UI)
# ==============================================================================

_log_raw() { echo -e "$1" | tee -a "${LOG_FILE}"; }

log_info()    { _log_raw "  ${C_CYAN}[INFO]${C_RESET}    $1"; }
log_success() { _log_raw "  ${C_GREEN}[OK]${C_RESET}      $1"; }
log_warn()    { _log_raw "  ${C_YELLOW}[AVISO]${C_RESET}   $1"; }
log_error()   { _log_raw "  ${C_RED}[ERRO]${C_RESET}    $1" >&2; }

separator() {
    echo -e "${C_BLUE}$(printf '%0.s-' {1..64})${C_RESET}"
}

step_header() {
    local -r num="$1" title="$2"
    echo ""
    separator
    echo -e "  ${C_MAGENTA}${C_BOLD}ETAPA ${num}${C_RESET} ${C_DIM}|${C_RESET} ${C_WHITE}${title}${C_RESET}"
    separator
    echo ""
}

show_banner() {
    clear
    echo -e "${C_BLUE}"
    cat << 'BANNER'
  +==============================================================+
  |                                                              |
  |    ██████╗██╗  ██╗   ██╗██╗   ██╗ ██████╗                    |
  |   ██╔════╝██║  ╚██╗ ██╔╝██║   ██║██╔═══██╗                   |
  |   ██║     ██║   ╚████╔╝ ██║   ██║██║   ██║                   |
  |   ██║     ██║    ╚██╔╝  ╚██╗ ██╔╝██║   ██║                   |
  |   ╚██████╗███████╗██║    ╚████╔╝ ╚██████╔╝                   |
  |    ╚═════╝╚══════╝╚═╝     ╚═══╝   ╚═════╝                    |
  |                                                              |
  |  🐾  V E T  --  A C I  P R O V I S I O N E R  🐾             |
  |                                                              |
  +==============================================================+
BANNER
    echo -e "${C_RESET}"
    echo -e "  ${C_CYAN}Sistema de Gerenciamento para Clinicas Veterinarias${C_RESET}"
    echo -e "  ${C_WHITE}Stack: .NET 8 API | Oracle DB | ACR | Azure Container Instances${C_RESET}"
    echo -e "  ${C_DIM}Versao ${SCRIPT_VERSION}  |  Log: ${LOG_FILE}${C_RESET}"
    separator
    echo ""
}

# ==============================================================================
# SECAO 3 -- GERENCIAMENTO DE ERROS E LIMPEZA
# ==============================================================================

cleanup() {
    rm -f "${ACI_YAML_FILE}" "${ACI_OUTPUT_FILE}" 2>/dev/null || true
    log_info "${C_DIM}Arquivos temporarios removidos. Log preservado em: ${LOG_FILE}${C_RESET}"
}

abort() {
    local -r msg="${1:-"Erro desconhecido."}"
    echo ""
    separator
    log_error "${C_BOLD}Provisionamento abortado!${C_RESET}"
    log_error "${msg}"
    log_error "Consulte o log completo: ${C_BOLD}${LOG_FILE}${C_RESET}"
    separator
    echo ""
    exit 1
}

# ==============================================================================
# SECAO 4 -- VERIFICACAO DE DEPENDENCIAS E AUTENTICACAO AZURE
# ==============================================================================

check_dependencies() {
    step_header "01" "${SYM_CHECK}  Verificando Dependencias do Sistema"

    for cmd in "${REQUIRED_CMDS[@]}"; do
        if command -v "${cmd}" &>/dev/null; then
            log_success "Dependencia encontrada: ${C_BOLD}${cmd}${C_RESET} $(command -v "${cmd}")"
        else
            abort "Dependencia ausente: '${C_BOLD}${cmd}${C_RESET}'.\n  ${C_DIM}-> az:    https://docs.microsoft.com/cli/azure/install-azure-cli\n  -> jq:    apt install jq / brew install jq\n  -> curl:  apt install curl${C_RESET}"
        fi
    done

    if command -v python3 &>/dev/null; then
        log_success "Dependencia encontrada: ${C_BOLD}python3${C_RESET} $(command -v python3)"
    else
        log_warn "python3 nao encontrado. O YAML do ACI sera gerado via heredoc."
        log_warn "Recomendado: instale python3 + PyYAML para serializacao segura de senhas."
    fi
}

check_azure_auth() {
    step_header "02" "${SYM_LOCK}  Verificando Autenticacao na Azure"

    if ! az account show &>/dev/null 2>&1; then
        log_warn "Sessao Azure CLI nao encontrada. Iniciando login..."
        echo ""
        if ! az login --output none; then
            abort "Falha na autenticacao. Execute 'az login' manualmente e tente novamente."
        fi
    fi

    local account_name subscription_id tenant_id
    account_name=$(az account show    --query "user.name"   -o tsv)
    subscription_id=$(az account show --query "id"          -o tsv)
    tenant_id=$(az account show       --query "tenantId"    -o tsv)

    log_success "Autenticado como:   ${C_BOLD}${account_name}${C_RESET}"
    log_info    "Subscription ID:    ${C_BOLD}${subscription_id}${C_RESET}"
    log_info    "Tenant ID:          ${C_BOLD}${tenant_id}${C_RESET}"

    export AZURE_SUBSCRIPTION_ID="${subscription_id}"
}

# ==============================================================================
# SECAO 5 -- COLETA E VALIDACAO DE PARAMETROS
# ==============================================================================

_validate_password() {
    local -r password="$1"
    [[ ${#password} -ge 12 ]]         || return 1
    [[ "$password" =~ [A-Z] ]]        || return 1
    [[ "$password" =~ [a-z] ]]        || return 1
    [[ "$password" =~ [0-9] ]]        || return 1
    [[ "$password" =~ [^a-zA-Z0-9] ]] || return 1
    return 0
}

_prompt_password() {
    local -r label="$1"
    local -n _pw_out="$2"   # nameref -- requer bash 4.3+

    while true; do
        echo -e "\n  ${C_YELLOW}${SYM_LOCK} ${label}${C_RESET} ${C_DIM}(>=12 chars, maiuscula, minuscula, numero, especial):${C_RESET}"
        read -rsp "  > " _tmp_pw
        echo ""

        if ! _validate_password "${_tmp_pw}"; then
            log_warn "Senha nao atende aos requisitos de complexidade. Tente novamente."
            continue
        fi

        echo -e "  ${C_YELLOW}${SYM_LOCK} Confirme ${label}:${C_RESET}"
        read -rsp "  > " _tmp_pw2
        echo ""

        if [[ "${_tmp_pw}" != "${_tmp_pw2}" ]]; then
            log_warn "As senhas nao coincidem. Tente novamente."
            continue
        fi

        _pw_out="${_tmp_pw}"
        log_success "Senha '${label}' validada."
        break
    done
}

collect_parameters() {
    step_header "03" "${SYM_PAW}  Configurando Parametros do Ambiente ClyvoVet"
    echo -e "  ${C_CYAN}Preencha os dados para o provisionamento ACI:${C_RESET}\n"

    read -rp "  $(echo -e "${C_YELLOW}${SYM_CLOUD}  Resource Group${C_RESET}        ${C_DIM}(ex: rg-clyvovet-prod):${C_RESET}   ")" RG_NAME
    [[ -n "${RG_NAME}" ]] || abort "Resource Group nao pode ser vazio."

    read -rp "  $(echo -e "${C_YELLOW}${SYM_GLOBE}  Regiao Azure${C_RESET}          ${C_DIM}(ex: brazilsouth):${C_RESET}        ")" LOCATION
    [[ -n "${LOCATION}" ]] || abort "Regiao nao pode ser vazia."

    read -rp "  $(echo -e "${C_YELLOW}${SYM_VET}   Nome do Container Group${C_RESET} ${C_DIM}(ex: cg-clyvovet):${C_RESET}      ")" CONTAINER_GROUP_NAME
    [[ -n "${CONTAINER_GROUP_NAME}" ]] || abort "Nome do Container Group nao pode ser vazio."

    read -rp "  $(echo -e "${C_YELLOW}${SYM_KEY}   Nome do ACR${C_RESET}           ${C_DIM}(ex: clyvovetregistry):${C_RESET}   ")" ACR_NAME
    [[ -n "${ACR_NAME}" ]] || abort "Nome do ACR nao pode ser vazio."

    read -rp "  $(echo -e "${C_YELLOW}${SYM_GLOBE}  DNS Label publico${C_RESET}     ${C_DIM}(ex: clyvovet-api):${C_RESET}       ")" DNS_LABEL
    [[ -n "${DNS_LABEL}" ]] || abort "DNS Label nao pode ser vazio."

    read -rp "  $(echo -e "${C_YELLOW}${SYM_KEY}   Oracle App User${C_RESET}       ${C_DIM}(ex: clyvovet_user):${C_RESET}      ")" ORACLE_APP_USER
    [[ -n "${ORACLE_APP_USER}" ]] || abort "Oracle App User nao pode ser vazio."

    # Variaveis inicializadas antes do nameref para satisfazer set -u
    ORACLE_ROOT_PASSWORD=""
    ORACLE_APP_PASSWORD=""

    _prompt_password "Oracle Root Password" ORACLE_ROOT_PASSWORD
    _prompt_password "Oracle App Password"  ORACLE_APP_PASSWORD

    export RG_NAME LOCATION CONTAINER_GROUP_NAME ACR_NAME DNS_LABEL
    export ORACLE_APP_USER ORACLE_ROOT_PASSWORD ORACLE_APP_PASSWORD

    echo ""
    log_info "Parametros coletados e validados."
}

# ==============================================================================
# SECAO 6 -- BUSCA AUTOMATICA DE CREDENCIAIS DO ACR
# ==============================================================================

fetch_acr_credentials() {
    step_header "04" "${SYM_KEY}  Buscando Credenciais do Azure Container Registry"

    log_info "Verificando existencia do ACR '${C_BOLD}${ACR_NAME}${C_RESET}'..."

    if ! az acr show --name "${ACR_NAME}" --output none 2>>"${LOG_FILE}"; then
        abort "ACR '${ACR_NAME}' nao encontrado ou sem permissao de acesso.\n  ${C_DIM}Verifique o nome do registro e suas permissoes na subscription.${C_RESET}"
    fi

    log_info "Obtendo credenciais via 'az acr credential show'..."

    local cred_json
    if ! cred_json=$(az acr credential show \
            --name "${ACR_NAME}" \
            --output json 2>>"${LOG_FILE}"); then
        abort "Falha ao buscar credenciais do ACR '${ACR_NAME}'.\n  ${C_DIM}O admin user precisa estar habilitado:\n  az acr update --name ${ACR_NAME} --admin-enabled true${C_RESET}"
    fi

    ACR_LOGIN_SERVER=$(az acr show \
        --name "${ACR_NAME}" \
        --query "loginServer" -o tsv 2>>"${LOG_FILE}")

    ACR_USERNAME=$(echo "${cred_json}" | jq -r '.username')
    ACR_PASSWORD=$(echo "${cred_json}" | jq -r '.passwords[0].value')

    if [[ -z "${ACR_LOGIN_SERVER}" || -z "${ACR_USERNAME}" || -z "${ACR_PASSWORD}" ]]; then
        abort "Credenciais do ACR incompletas. Verifique o log: ${LOG_FILE}"
    fi

    log_success "ACR Login Server : ${C_BOLD}${ACR_LOGIN_SERVER}${C_RESET}"
    log_success "ACR Username     : ${C_BOLD}${ACR_USERNAME}${C_RESET}"
    log_success "ACR Password     : ${C_BOLD}[OCULTA]${C_RESET}"

    export ACR_LOGIN_SERVER ACR_USERNAME ACR_PASSWORD
}

# ==============================================================================
# SECAO 7 -- GERACAO DO MANIFESTO YAML DO CONTAINER GROUP ACI
# ==============================================================================

generate_aci_yaml() {
    step_header "05" "${SYM_SYRINGE}  Gerando Manifesto YAML do Container Group ACI"

    if command -v python3 &>/dev/null; then
        _generate_aci_yaml_python
    else
        log_warn "python3 ausente -- gerando YAML via heredoc."
        log_warn "Senhas com aspas duplas ou barras invertidas podem causar YAML invalido."
        _generate_aci_yaml_heredoc
    fi

    # Validacao YAML via python3 (se disponivel)
    if command -v python3 &>/dev/null; then
        if python3 - "${ACI_YAML_FILE}" << 'PYEOF' 2>>"${LOG_FILE}"
import sys, yaml
with open(sys.argv[1], encoding='utf-8') as f:
    yaml.safe_load(f)
PYEOF
        then
            log_success "Validacao YAML: OK"
        else
            abort "Manifesto ACI invalido. Verifique: ${LOG_FILE}"
        fi
    fi

    log_success "Manifesto ACI gerado em: ${C_BOLD}${ACI_YAML_FILE}${C_RESET}"
    log_info    "Tamanho do arquivo: $(wc -c < "${ACI_YAML_FILE}") bytes"
}

_generate_aci_yaml_python() {
    log_info "Gerando YAML via Python (serializacao segura de senhas e caracteres especiais)..."

    # O path do arquivo e passado como argv[1]; demais valores vem do ambiente exportado
    python3 - "${ACI_YAML_FILE}" << 'PYEOF'
import yaml, os, sys

output_path    = sys.argv[1]
location       = os.environ['LOCATION']
cg_name        = os.environ['CONTAINER_GROUP_NAME']
acr_server     = os.environ['ACR_LOGIN_SERVER']
acr_user       = os.environ['ACR_USERNAME']
acr_pass       = os.environ['ACR_PASSWORD']
dns_label      = os.environ['DNS_LABEL']
oracle_root_pw = os.environ['ORACLE_ROOT_PASSWORD']
oracle_app_usr = os.environ['ORACLE_APP_USER']
oracle_app_pw  = os.environ['ORACLE_APP_PASSWORD']

api_image  = f"{acr_server}/clyvovet-api:latest"
# No ACI todos os containers do grupo compartilham interface de rede (localhost)
conn_str   = (
    f"Data Source=localhost:1521/FREEPDB1;"
    f"User Id={oracle_app_usr};"
    f"Password={oracle_app_pw};"
)

manifest = {
    'apiVersion': '2021-10-01',
    'location': location,
    'name': cg_name,
    'properties': {
        'containers': [
            {
                'name': 'db-oracle',
                'properties': {
                    'image': 'gvenzl/oracle-free:latest',
                    'resources': {
                        'requests': {'cpu': 2.0, 'memoryInGB': 4.0}
                    },
                    'ports': [{'port': 1521, 'protocol': 'TCP'}],
                    'environmentVariables': [
                        {'name': 'ORACLE_PASSWORD',   'secureValue': oracle_root_pw},
                        {'name': 'APP_USER',          'value':       oracle_app_usr},
                        {'name': 'APP_USER_PASSWORD', 'secureValue': oracle_app_pw},
                    ]
                }
            },
            {
                'name': 'clyvo-api',
                'properties': {
                    'image': api_image,
                    'resources': {
                        'requests': {'cpu': 1.0, 'memoryInGB': 1.5}
                    },
                    'command': [
                        'bash', '-c',
                        'until (echo > /dev/tcp/localhost/1521) 2>/dev/null; '
                        'do echo "Aguardando Oracle listener..."; sleep 5; done; '
                        'echo "Listener Oracle respondeu -- aguardando abertura do banco (+60s)..."; '
                        'sleep 60; '
                        'echo "Iniciando API"; '
                        'exec dotnet ClyvoVet.Api.dll'
                    ],
                    'ports': [{'port': 5139, 'protocol': 'TCP'}],
                    'environmentVariables': [
                        {'name': 'ASPNETCORE_ENVIRONMENT',               'value':       'Production'},
                        {'name': 'ASPNETCORE_URLS',                      'value':       'http://+:5139'},
                        {'name': 'ConnectionStrings__OracleDbConnection', 'secureValue': conn_str},
                    ]
                }
            }
        ],
        'imageRegistryCredentials': [
            {
                'server':   acr_server,
                'username': acr_user,
                'password': acr_pass,
            }
        ],
        'ipAddress': {
            'type':         'Public',
            'dnsNameLabel': dns_label,
            'ports': [
                {'protocol': 'TCP', 'port': 5139}
            ]
        },
        'osType':         'Linux',
        'restartPolicy':  'Always',
    },
    'type': 'Microsoft.ContainerInstance/containerGroups'
}

with open(output_path, 'w', encoding='utf-8') as f:
    yaml.dump(manifest, f, default_flow_style=False, allow_unicode=True, sort_keys=False)
PYEOF
}

_generate_aci_yaml_heredoc() {
    # Fallback sem python3. Nao e seguro para senhas com " ou \
    cat > "${ACI_YAML_FILE}" << ACIEOF
apiVersion: '2021-10-01'
location: ${LOCATION}
name: ${CONTAINER_GROUP_NAME}
properties:
  containers:
  - name: db-oracle
    properties:
      image: gvenzl/oracle-free:latest
      resources:
        requests:
          cpu: 2.0
          memoryInGB: 4.0
      ports:
      - port: 1521
        protocol: TCP
      environmentVariables:
      - name: ORACLE_PASSWORD
        secureValue: "${ORACLE_ROOT_PASSWORD}"
      - name: APP_USER
        value: ${ORACLE_APP_USER}
      - name: APP_USER_PASSWORD
        secureValue: "${ORACLE_APP_PASSWORD}"
  - name: clyvo-api
    properties:
      image: ${ACR_LOGIN_SERVER}/clyvovet-api:latest
      resources:
        requests:
          cpu: 1.0
          memoryInGB: 1.5
      command:
      - bash
      - -c
      - 'until (echo > /dev/tcp/localhost/1521) 2>/dev/null; do echo "Aguardando Oracle listener..."; sleep 5; done; echo "Listener Oracle respondeu -- aguardando abertura do banco (+60s)..."; sleep 60; echo "Iniciando API"; exec dotnet ClyvoVet.Api.dll'
      ports:
      - port: 5139
        protocol: TCP
      environmentVariables:
      - name: ASPNETCORE_ENVIRONMENT
        value: Production
      - name: ASPNETCORE_URLS
        value: 'http://+:5139'
      - name: ConnectionStrings__OracleDbConnection
        secureValue: "Data Source=localhost:1521/FREEPDB1;User Id=${ORACLE_APP_USER};Password=${ORACLE_APP_PASSWORD};"
  imageRegistryCredentials:
  - server: ${ACR_LOGIN_SERVER}
    username: ${ACR_USERNAME}
    password: "${ACR_PASSWORD}"
  ipAddress:
    type: Public
    dnsNameLabel: ${DNS_LABEL}
    ports:
    - protocol: TCP
      port: 5139
  osType: Linux
  restartPolicy: Always
type: Microsoft.ContainerInstance/containerGroups
ACIEOF
}

# ==============================================================================
# SECAO 8 -- PROVISIONAMENTO DO CONTAINER GROUP ACI
# ==============================================================================

provision_resource_group() {
    log_info "Criando/verificando Resource Group '${C_BOLD}${RG_NAME}${C_RESET}' em '${C_BOLD}${LOCATION}${C_RESET}'..."

    if ! az group create \
            --name     "${RG_NAME}" \
            --location "${LOCATION}" \
            --output none 2>>"${LOG_FILE}"; then
        abort "Falha ao criar o Resource Group '${RG_NAME}'."
    fi

    log_success "Resource Group '${C_BOLD}${RG_NAME}${C_RESET}' pronto."
}

provision_aci() {
    step_header "06" "${SYM_ROCKET}  Provisionando Container Group no ACI"
    provision_resource_group

    log_info "Criando container group '${C_BOLD}${CONTAINER_GROUP_NAME}${C_RESET}'..."
    log_info "${C_DIM}Isso pode levar alguns minutos (pull das imagens + inicializacao do Oracle)...${C_RESET}"

    if ! PYTHONIOENCODING=utf-8 az container create \
            --resource-group "${RG_NAME}" \
            --file           "${ACI_YAML_FILE}" \
            --output json > "${ACI_OUTPUT_FILE}" 2>>"${LOG_FILE}"; then
        abort "Falha ao criar o Container Group ACI.\n  ${C_DIM}Causas comuns:\n  -> Imagem '${ACR_LOGIN_SERVER}/clyvovet-api:latest' nao existe no ACR\n  -> Credenciais ACR invalidas ou admin user desabilitado\n  -> Cota de CPU/memoria insuficiente na regiao '${LOCATION}'\n  -> DNS label '${DNS_LABEL}' ja em uso\n  Consulte: ${LOG_FILE}${C_RESET}"
    fi

    log_success "Container Group '${C_BOLD}${CONTAINER_GROUP_NAME}${C_RESET}' criado com sucesso."
}

# ==============================================================================
# SECAO 9 -- TESTE DE SAUDE DA API (/health/live)
# ==============================================================================

wait_and_test_health() {
    step_header "07" "${SYM_VET}  Verificando Saude da API (/health/live)"

    # Tenta FQDN primeiro; cai para IP publico se DNS label nao estiver pronto
    local endpoint
    endpoint=$(jq -r '.properties.ipAddress.fqdn // empty' "${ACI_OUTPUT_FILE}" 2>/dev/null || true)

    if [[ -z "${endpoint}" ]]; then
        endpoint=$(jq -r '.properties.ipAddress.ip // empty' "${ACI_OUTPUT_FILE}" 2>/dev/null || true)
    fi

    if [[ -z "${endpoint}" ]]; then
        log_warn "Nao foi possivel extrair FQDN/IP do output do ACI."
        log_warn "Verifique manualmente: az container show -g ${RG_NAME} -n ${CONTAINER_GROUP_NAME}"
        export API_FQDN=""
        return 0
    fi

    export API_FQDN="${endpoint}"

    local health_url="http://${endpoint}:${API_PORT}/health/live"
    log_info "Endpoint de saude: ${C_BOLD}${health_url}${C_RESET}"

    local -r max_attempts=20
    local -r sleep_sec=15
    local attempt=0

    log_info "${C_DIM}Aguardando Oracle inicializar (~2-3 min) e API subir. Tempo maximo: $((max_attempts * sleep_sec / 60)) min...${C_RESET}"
    echo ""

    while (( attempt < max_attempts )); do
        (( attempt++ ))
        printf "  ${C_DIM}[%02d/%02d]${C_RESET} %s -- " "${attempt}" "${max_attempts}" "$(date +%H:%M:%S)"

        local http_status
        http_status=$(curl -s -o /dev/null -w "%{http_code}" \
            --max-time 8 \
            "${health_url}" 2>/dev/null || echo "000")

        if [[ "${http_status}" == "200" ]]; then
            echo "HTTP ${http_status}"
            echo ""
            log_success "API respondeu com ${C_BOLD}HTTP 200${C_RESET} -- servico saudavel! ${SYM_CHECK}"
            return 0
        fi

        echo "HTTP ${http_status} (aguardando...)"
        sleep "${sleep_sec}"
    done

    echo ""
    log_warn "API nao respondeu apos $((max_attempts * sleep_sec))s. O Oracle pode precisar de mais tempo."
    log_warn "Teste manual:"
    log_warn "  curl ${health_url}"
    log_warn "Logs da API:"
    log_warn "  az container logs -g ${RG_NAME} -n ${CONTAINER_GROUP_NAME} --container-name clyvo-api"
}

# ==============================================================================
# SECAO 10 -- RESUMO FINAL DO PROVISIONAMENTO
# ==============================================================================

display_summary() {
    local fqdn="${API_FQDN:-}"
    local endpoint_base

    if [[ -n "${fqdn}" ]]; then
        endpoint_base="http://${fqdn}:${API_PORT}"
    else
        endpoint_base="http://<IP-do-ACI>:${API_PORT}"
    fi

    echo ""
    echo -e "${C_GREEN}"
    cat << 'SUMMARY'
  +==============================================================+
  |                                                              |
  |  [OK] ClyvoVet -- Container Group ACI Provisionado!         |
  |  [OK] API .NET 8 + Oracle rodando no Azure Container Inst.  |
  |                                                              |
  +==============================================================+
SUMMARY
    echo -e "${C_RESET}"

    separator
    echo -e "  ${SYM_CLOUD} ${C_WHITE}${C_BOLD}Detalhes do Container Group${C_RESET}"
    separator
    echo -e "  ${C_CYAN}Resource Group    :${C_RESET}  ${RG_NAME}"
    echo -e "  ${C_CYAN}Container Group   :${C_RESET}  ${CONTAINER_GROUP_NAME}"
    echo -e "  ${C_CYAN}Regiao            :${C_RESET}  ${LOCATION}"
    echo -e "  ${C_CYAN}FQDN / IP         :${C_RESET}  ${C_BOLD}${fqdn:-"(veja: az container show abaixo)"}${C_RESET}"
    echo -e "  ${C_CYAN}Containers        :${C_RESET}  db-oracle  (Oracle Free, porta interna 1521)"
    echo -e "                      clyvo-api  (.NET 8 API, porta publica ${API_PORT})"
    echo -e "  ${C_CYAN}Rede interna      :${C_RESET}  localhost (containers compartilham interface no ACI)"
    echo ""
    separator
    echo -e "  ${SYM_GLOBE} ${C_WHITE}${C_BOLD}Endpoints da Aplicacao${C_RESET}"
    separator
    echo -e "  ${C_YELLOW}API REST          :${C_RESET}  ${endpoint_base}"
    echo -e "  ${C_YELLOW}Health Live       :${C_RESET}  ${endpoint_base}/health/live"
    echo -e "  ${C_YELLOW}Health Ready      :${C_RESET}  ${endpoint_base}/health/ready"
    echo ""
    separator
    echo -e "  ${SYM_PAW} ${C_WHITE}${C_BOLD}Comandos Uteis (Azure CLI)${C_RESET}"
    separator
    echo ""
    echo -e "  ${C_CYAN}# Status dos containers:${C_RESET}"
    echo -e "  ${C_GREEN}az container show -g ${RG_NAME} -n ${CONTAINER_GROUP_NAME} \\${C_RESET}"
    echo -e "  ${C_GREEN}  --query 'containers[].{nome:name,estado:instanceView.currentState.state}' -o table${C_RESET}"
    echo ""
    echo -e "  ${C_CYAN}# Logs em tempo real (API):${C_RESET}"
    echo -e "  ${C_GREEN}az container logs -g ${RG_NAME} -n ${CONTAINER_GROUP_NAME} --container-name clyvo-api --follow${C_RESET}"
    echo ""
    echo -e "  ${C_CYAN}# Logs em tempo real (Oracle):${C_RESET}"
    echo -e "  ${C_GREEN}az container logs -g ${RG_NAME} -n ${CONTAINER_GROUP_NAME} --container-name db-oracle --follow${C_RESET}"
    echo ""
    echo -e "  ${C_CYAN}# Reiniciar todos os containers do grupo:${C_RESET}"
    echo -e "  ${C_GREEN}az container restart -g ${RG_NAME} -n ${CONTAINER_GROUP_NAME}${C_RESET}"
    echo ""
    echo -e "  ${C_CYAN}# Destruir o ambiente (CUIDADO -- apaga dados Oracle):${C_RESET}"
    echo -e "  ${C_GREEN}az container delete -g ${RG_NAME} -n ${CONTAINER_GROUP_NAME} --yes${C_RESET}"
    echo ""
    separator
    echo -e "  ${SYM_WARN}  ${C_YELLOW}Oracle DB pode levar ate 3-5 min para inicializar na primeira vez.${C_RESET}"
    echo -e "  ${SYM_WARN}  ${C_YELLOW}No ACI a rede e local -- API conecta em localhost:1521, nao db-oracle:1521.${C_RESET}"
    echo -e "  ${SYM_WARN}  ${C_YELLOW}Dados Oracle NAO sao persistidos apos 'az container delete'.${C_RESET}"
    separator
    echo ""
    log_info "Log completo: ${C_BOLD}${LOG_FILE}${C_RESET}"
    echo ""
}

# ==============================================================================
# SECAO 11 -- PONTO DE ENTRADA PRINCIPAL
# ==============================================================================

main() {
    trap cleanup EXIT
    trap 'echo ""; abort "Script interrompido pelo usuario (CTRL+C)."' INT TERM

    show_banner
    check_dependencies
    check_azure_auth
    collect_parameters
    fetch_acr_credentials
    generate_aci_yaml
    provision_aci
    wait_and_test_health
    display_summary
}

main "$@"
