# SafeStockAPI 🚨📦

SafeStockAPI é uma solução robusta desenvolvida em **ASP.NET Core**, com foco na **gestão de estoque de suprimentos essenciais** para resposta rápida em cenários de **desastres naturais**.

## 🔧 Tecnologias Utilizadas

- **ASP.NET Core 8**
- **Entity Framework Core**
- **SQL Server**
- **ML.NET** (para predição de prioridades)
- **Swagger** (documentação de API)

## 🚀 Funcionalidades Principais

### 📦 Gestão de Produtos
- CRUD completo de produtos
- Controle de quantidades em estoque
- Status de disponibilidade automático
- Relacionamento com categorias

### 🏷️ Gestão de Categorias
- Categorização de produtos
- Contagem automática de produtos por categoria
- Validação de exclusão (impede exclusão de categorias com produtos)

### 📊 Movimentações de Estoque
- Registro completo de entradas e saídas
- Atualização automática dos níveis de estoque
- Histórico de movimentações
- Validação de estoque negativo

### 🤖 Predição ML
- Sistema de priorização inteligente
- Recomendação de reposição baseada em histórico
- Diagnósticos do modelo de machine learning

## 📚 Endpoints Disponíveis

### Categorias (`/api/categorias`)

| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| `GET` | `/` | Lista todas as categorias com contagem de produtos | - |
| `GET` | `/{id}` | Obtém uma categoria específica | `id`: ID da categoria |
| `POST` | `/` | Cria uma nova categoria | Body: `{ "nome": string }` |
| `PUT` | `/{id}` | Atualiza uma categoria | `id`: ID da categoria, Body: `{ "id": int, "nome": string }` |
| `DELETE` | `/{id}` | Exclui uma categoria (se não tiver produtos) | `id`: ID da categoria |

### Produtos (`/api/produtos`)

| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| `GET` | `/` | Lista todos os produtos | - |
| `GET` | `/{id}` | Obtém um produto específico | `id`: ID do produto |
| `GET` | `/por-categoria/{categoriaId}` | Filtra produtos por categoria | `categoriaId`: ID da categoria |
| `POST` | `/` | Cria um novo produto | Body: `{ "nome": string, "quantidadeInicial": int, "categoriaId": int }` |
| `PUT` | `/{id}/adicionar` | Adiciona quantidade ao estoque | `id`: ID do produto, Body: `quantidade` (int) |
| `POST` | `/retirar` | Retira quantidade do estoque | Body: `{ "produtoId": int, "quantidade": int }` |
| `DELETE` | `/{id}` | Remove um produto | `id`: ID do produto |

### Movimentações (`/api/movimentacoes`)

| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| `GET` | `/` | Lista todas as movimentações | `produtoId` (query opcional): ID do produto para filtrar |
| `POST` | `/` | Registra nova movimentação | Body: `{ "produtoId": int, "quantidade": int, "tipo": "ENTRADA"\|"SAIDA" }` |
| `PUT` | `/{id}` | Atualiza uma movimentação | `id`: ID da movimentação, Body: `{ "id": int, "quantidade": int }` |
| `DELETE` | `/{id}` | Remove uma movimentação | `id`: ID da movimentação |

### Predição (`/api/products`)

| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| `GET` | `/{id}/prioridade-reposicao` | Prioridade de reposição para um produto | `id`: ID do produto |
| `GET` | `/ml-diagnostics` | Diagnósticos do modelo ML | - |

