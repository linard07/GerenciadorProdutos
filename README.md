# Cadastro de Produtos — C# + ADO.NET

Aplicação desktop **WPF** em C# (.NET 8, Windows) para cadastrar e gerenciar produtos, com CRUD completo usando **ADO.NET** (`Microsoft.Data.Sqlite`) e banco **SQLite**.

## Funcionalidades
1. Inserir produto
2. Listar produtos
3. Buscar produto por ID
4. Atualizar produto
5. Excluir produto
6. Sair

## Requisitos técnicos atendidos
- ADO.NET (`SqliteConnection`, `SqliteCommand`, `SqliteDataReader`)
- `ExecuteNonQuery` em INSERT/UPDATE/DELETE e `ExecuteReader` nos SELECT
- Mapeamento manual do DataReader para objetos `Produto`
- SQL parametrizado em todas as operações (proteção contra SQL Injection)
- Tratamento de exceções de banco (`SqliteException` → `DatabaseException`)
- Log das operações em arquivo (`logs/operacoes.log`)
- Connection string no `appsettings.json`

## Estrutura
```
GerenciadorProdutos/
├── Database/criar_tabela.sql     # script de criação da tabela
├── Data/
│   ├── ProdutoRepository.cs      # acesso ao banco
│   ├── DatabaseInitializer.cs    # executa o script .sql
│   └── DatabaseException.cs
├── Models/Produto.cs
├── Services/Logger.cs            # log em arquivo
├── App.xaml / App.xaml.cs        # inicialização e estilos
├── MainWindow.xaml(.cs)          # tela (interface)
└── appsettings.json              # connection string
```

## Como executar
1. Windows com [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (ou Visual Studio 2022 com a carga de trabalho ".NET desktop development").
2. Clone o repositório:
   ```
   git clone <URL-DO-REPOSITORIO>
   cd GerenciadorProdutos
   ```
3. Execute:
   ```
   dotnet run
   ```
O banco `produtos.db` é criado automaticamente na primeira execução (a tabela é criada pelo script `Database/criar_tabela.sql`).

## Banco de dados
- SQLite, arquivo `produtos.db` criado ao lado do executável (`bin/Debug/net8.0-windows/`).
- Para mudar o local, edite `ConnectionStrings:DefaultConnection` em `appsettings.json`.

## Log
As operações ficam em `logs/operacoes.log`, ao lado do executável.

## Prints
Veja a pasta `prints/`.

## Autor
Guilherme Linard — RM 555768 — FIAP 3ESPZ
