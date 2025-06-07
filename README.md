# SafeStockAPI 🚨📦

**SafeStockAPI** é uma solução robusta desenvolvida em **ASP.NET Core**, focada na **gestão de estoque de suprimentos essenciais** para resposta rápida em cenários de **desastres naturais**. Esta API visa otimizar o processo de rastreamento, categorização e priorização de suprimentos críticos, garantindo que os recursos certos estejam disponíveis quando e onde são mais necessários.

---

## 🔧 Tecnologias Utilizadas

Este projeto utiliza uma stack moderna e poderosa para entregar um sistema de gestão de inventário escalável e inteligente:

* **ASP.NET Core 8**: O framework fundamental para a construção de APIs web de alto desempenho e multiplataforma.
* **Entity Framework Core**: Um ORM (Object-Relational Mapper) que simplifica as interações com o banco de dados **SQL Server**.
* **SQL Server**: O sistema de gerenciamento de banco de dados relacional usado para o armazenamento persistente de dados.
* **ML.NET**: O framework de machine learning da Microsoft, integrado para a predição inteligente de prioridades de suprimentos e recomendações de reposição.
* **Swagger**: Proporciona uma documentação interativa da API, facilitando a compreensão e o teste dos endpoints disponíveis.

---

## 🚀 Como Executar o Projeto

Para colocar o SafeStockAPI em funcionamento na sua máquina local, siga estes passos:

### Pré-requisitos

Antes de começar, certifique-se de ter o seguinte instalado:

* **.NET 8 SDK**: Baixe e instale o SDK mais recente do .NET 8 no [site oficial da Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0).
* **SQL Server**: Você precisará de uma instância do SQL Server acessível. Pode ser SQL Server Express, SQL Server Developer Edition, ou um contêiner Docker.
* **SQL Server Management Studio (SSMS) ou Azure Data Studio (Opcional)**: Para gerenciar sua instância e bancos de dados SQL Server.

### Configuração do Banco de Dados

1.  **Crie um Banco de Dados**: Na sua instância do SQL Server, crie um novo banco de dados vazio. Por exemplo, `SafeStockDB`.
2.  **Atualize a Connection String**: Abra o arquivo `appsettings.json` no projeto SafeStockAPI. Localize a seção `ConnectionStrings` e atualize a `DefaultConnection` para apontar para sua instância do SQL Server e o banco de dados que você criou:

    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=SEU_NOME_DO_SERVIDOR;Database=SafeStockDB;User Id=SEU_USUARIO;Password=SUA_SENHA;TrustServerCertificate=True;"
    }
    ```

    * Substitua `SEU_NOME_DO_SERVIDOR` pelo nome da sua instância do SQL Server (ex: `localhost`, `.\SQLEXPRESS`).
    * Substitua `SafeStockDB` se você usou um nome de banco de dados diferente.
    * Ajuste `User Id` e `Password` se estiver usando Autenticação SQL Server. Se estiver usando Autenticação do Windows, você pode usar `Integrated Security=True;`.
3.  **Aplicar Migrações do Entity Framework Core**: Navegue até o diretório do projeto no seu terminal ou prompt de comando e execute os seguintes comandos para criar o esquema do banco de dados:

    ```bash
    dotnet ef database update
    ```

    Este comando aplicará todas as migrações pendentes e criará as tabelas necessárias no seu banco de dados SQL Server.

### Executando a Aplicação

Uma vez que o banco de dados esteja configurado, você pode executar a API:

1.  **Navegue até o Diretório do Projeto**:
    ```bash
    cd SafeStockAPI
    ```
    (Substitua `SafeStockAPI` pelo nome real da pasta do seu projeto, se for diferente.)
2.  **Execute a Aplicação**:
    ```bash
    dotnet run
    ```
    A API será iniciada, geralmente escutando em `http://localhost:5000` ou `https://localhost:5001`. Você verá mensagens no seu terminal indicando que a aplicação foi iniciada.

---

## 📚 Documentação dos Endpoints

O SafeStockAPI oferece um conjunto abrangente de endpoints RESTful, totalmente documentados com **Swagger**. Uma vez que a aplicação esteja em execução, você pode acessar a UI do Swagger navegando para:

* `http://localhost:5000/swagger` (para HTTP)
* `https://localhost:5001/swagger` (para HTTPS)

A UI do Swagger permite que você explore cada endpoint, entenda seus parâmetros e até mesmo faça requisições diretas para testar a API.

Aqui está um resumo dos principais grupos de endpoints:

### Categorias (`/api/categorias`)

Gerencia a categorização de produtos.

| Método | Endpoint | Descrição | Parâmetros | Exemplo de Body |
| :----- | :------- | :-------- | :--------- | :-------------- |
| `GET` | `/` | Lista todas as categorias com contagem de produtos. | - | - |
| `GET` | `/{id}` | Obtém uma categoria específica pelo seu ID. | `id`: ID da categoria (int) | - |
| `POST` | `/` | Cria uma nova categoria. | Body: `{ "nome": "string" }` | `{ "nome": "Alimentos" }` |
| `PUT` | `/{id}` | Atualiza o nome de uma categoria existente. | `id`: ID da categoria (int), Body: `{ "id": int, "nome": "string" }` | `{ "id": 1, "nome": "Alimentos Não Perecíveis" }` |
| `DELETE` | `/{id}` | Exclui uma categoria (somente se não houver produtos associados). | `id`: ID da categoria (int) | - |

### Produtos (`/api/produtos`)

Lida com o gerenciamento de itens de produtos individuais dentro do inventário.

| Método | Endpoint | Descrição | Parâmetros | Exemplo de Body |
| :----- | :------- | :-------- | :--------- | :-------------- |
| `GET` | `/` | Lista todos os produtos em estoque. | - | - |
| `GET` | `/{id}` | Obtém um produto específico pelo seu ID. | `id`: ID do produto (int) | - |
| `GET` | `/por-categoria/{categoriaId}` | Filtra produtos por uma categoria específica. | `categoriaId`: ID da categoria (int) | - |
| `POST` | `/` | Cria um novo produto. | Body: `{ "nome": "string", "quantidadeInicial": int, "categoriaId": int }` | `{ "nome": "Água Mineral", "quantidadeInicial": 1000, "categoriaId": 1 }` |
| `PUT` | `/{id}/adicionar` | Adiciona uma quantidade específica ao estoque de um produto. | `id`: ID do produto (int), Body: `quantidade` (int) | `500` (para adicionar 500 unidades) |
| `POST` | `/retirar` | Retira uma quantidade específica do estoque de um produto. | Body: `{ "produtoId": int, "quantidade": int }` | `{ "produtoId": 2, "quantidade": 200 }` |
| `DELETE` | `/{id}` | Remove um produto do sistema. | `id`: ID do produto (int) | - |

### Movimentações (`/api/movimentacoes`)

Registra todas as movimentações de entrada (`ENTRADA`) e saída (`SAIDA`) de produtos.

| Método | Endpoint | Descrição | Parâmetros | Exemplo de Body |
| :----- | :------- | :-------- | :--------- | :-------------- |
| `GET` | `/` | Lista todas as movimentações de estoque. | `produtoId` (query opcional): ID do produto para filtrar movimentações. | - |
| `POST` | `/` | Registra uma nova movimentação de estoque (entrada ou saída). | Body: `{ "produtoId": int, "quantidade": int, "tipo": "ENTRADA"\|"SAIDA" }` | `{ "produtoId": 1, "quantidade": 100, "tipo": "ENTRADA" }` |
| `PUT` | `/{id}` | Atualiza uma movimentação de estoque existente. | `id`: ID da movimentação (int), Body: `{ "id": int, "quantidade": int }` | `{ "id": 5, "quantidade": 150 }` |
| `DELETE` | `/{id}` | Remove uma movimentação de estoque. | `id`: ID da movimentação (int) | - |

### Predição (`/api/products`)

Utiliza ML.NET para priorização inteligente e diagnósticos do modelo.

| Método | Endpoint | Descrição | Parâmetros |
| :----- | :------- | :-------- | :--------- |
| `GET` | `/{id}/prioridade-reposicao` | Retorna a prioridade de reposição para um produto específico com base no modelo ML. | `id`: ID do produto (int) |
| `GET` | `/ml-diagnostics` | Fornece diagnósticos e métricas sobre o modelo de machine learning. | - |

---

## 🧪 Instruções de Testes

Você pode testar o SafeStockAPI de diversas maneiras:

### 1. Usando a UI do Swagger

A maneira mais fácil de testar a API é diretamente através da **UI do Swagger**.

1.  Certifique-se de que a API está em execução (siga "Como Executar o Projeto").
2.  Abra seu navegador e navegue para `http://localhost:5000/swagger` (ou `https://localhost:5001/swagger`).
3.  Expanda o endpoint desejado clicando nele.
4.  Clique no botão "Try it out".
5.  Forneça quaisquer parâmetros ou corpo de requisição necessários.
6.  Clique em "Execute" para enviar a requisição e visualizar a resposta.

### 2. Usando Ferramentas como Postman ou Insomnia

Para testes mais avançados e construção de cenários, você pode usar clientes de API como Postman ou Insomnia.

1.  **Instale Postman/Insomnia**: Baixe e instale sua ferramenta preferida.
2.  **Crie uma Nova Requisição**:
    * Defina o **Método HTTP** (GET, POST, PUT, DELETE).
    * Insira a **URL da Requisição** (ex: `http://localhost:5000/api/produtos`).
    * Se a requisição exigir um **Body** (para POST/PUT), defina o cabeçalho `Content-Type` como `application/json` e forneça o payload JSON.
    * Envie a requisição e observe a resposta.

### 3. Executando Testes Automatizados (se disponíveis)

Se o projeto incluir testes unitários ou de integração automatizados, você pode executá-los usando a CLI do .NET:

1.  **Navegue até o Diretório da Solução**: Vá para o diretório raiz onde o seu arquivo `.sln` está localizado.
2.  **Execute os Testes**:
    ```bash
    dotnet test
    ```
    Este comando descobrirá e executará todos os testes dentro da solução, fornecendo um resumo dos testes aprovados e reprovados.

---

Sinta-se à vontade para contribuir, abrir issues ou sugerir melhorias!
