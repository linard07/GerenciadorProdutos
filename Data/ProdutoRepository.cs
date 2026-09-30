using GerenciadorProdutos.Models;
using GerenciadorProdutos.Services;
using Microsoft.Data.Sqlite;

namespace GerenciadorProdutos.Data;

/// <summary>Responsável por todo o acesso ao banco (ADO.NET puro).</summary>
public class ProdutoRepository
{
    private readonly string _connectionString;
    private readonly Logger _logger;

    public ProdutoRepository(string connectionString, Logger logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public int Inserir(Produto produto)
    {
        const string sql = @"INSERT INTO Produtos (Nome, Preco, Estoque, Categoria)
                             VALUES (@Nome, @Preco, @Estoque, @Categoria);";
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            using var comando = new SqliteCommand(sql, conexao);
            comando.Parameters.AddWithValue("@Nome", produto.Nome);
            comando.Parameters.AddWithValue("@Preco", (double)produto.Preco);
            comando.Parameters.AddWithValue("@Estoque", produto.Estoque);
            comando.Parameters.AddWithValue("@Categoria", produto.Categoria);
            comando.ExecuteNonQuery();

            using var comandoId = new SqliteCommand("SELECT last_insert_rowid();", conexao);
            int id = Convert.ToInt32(comandoId.ExecuteScalar());

            _logger.Info($"INSERIR: produto '{produto.Nome}' criado com Id={id}.");
            return id;
        }
        catch (SqliteException ex)
        {
            _logger.Erro("INSERIR falhou", ex);
            throw new DatabaseException("Erro ao inserir o produto no banco de dados.", ex);
        }
    }

    public List<Produto> Listar()
    {
        const string sql = "SELECT Id, Nome, Preco, Estoque, Categoria FROM Produtos ORDER BY Id;";
        var produtos = new List<Produto>();
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            using var comando = new SqliteCommand(sql, conexao);
            using var leitor = comando.ExecuteReader();
            while (leitor.Read())
            {
                produtos.Add(Mapear(leitor));
            }

            _logger.Info($"LISTAR: {produtos.Count} produto(s) retornado(s).");
            return produtos;
        }
        catch (SqliteException ex)
        {
            _logger.Erro("LISTAR falhou", ex);
            throw new DatabaseException("Erro ao listar os produtos.", ex);
        }
    }

    public Produto? BuscarPorId(int id)
    {
        const string sql = "SELECT Id, Nome, Preco, Estoque, Categoria FROM Produtos WHERE Id = @Id;";
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            using var comando = new SqliteCommand(sql, conexao);
            comando.Parameters.AddWithValue("@Id", id);

            using var leitor = comando.ExecuteReader();
            Produto? produto = leitor.Read() ? Mapear(leitor) : null;

            _logger.Info($"BUSCAR: Id={id} {(produto is null ? "não encontrado" : "encontrado")}.");
            return produto;
        }
        catch (SqliteException ex)
        {
            _logger.Erro($"BUSCAR Id={id} falhou", ex);
            throw new DatabaseException("Erro ao buscar o produto.", ex);
        }
    }

    public bool Atualizar(Produto produto)
    {
        const string sql = @"UPDATE Produtos
                             SET Nome = @Nome, Preco = @Preco, Estoque = @Estoque, Categoria = @Categoria
                             WHERE Id = @Id;";
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            using var comando = new SqliteCommand(sql, conexao);
            comando.Parameters.AddWithValue("@Nome", produto.Nome);
            comando.Parameters.AddWithValue("@Preco", (double)produto.Preco);
            comando.Parameters.AddWithValue("@Estoque", produto.Estoque);
            comando.Parameters.AddWithValue("@Categoria", produto.Categoria);
            comando.Parameters.AddWithValue("@Id", produto.Id);

            int linhas = comando.ExecuteNonQuery();
            _logger.Info($"ATUALIZAR: Id={produto.Id}, linhas afetadas={linhas}.");
            return linhas > 0;
        }
        catch (SqliteException ex)
        {
            _logger.Erro($"ATUALIZAR Id={produto.Id} falhou", ex);
            throw new DatabaseException("Erro ao atualizar o produto.", ex);
        }
    }

    public bool Excluir(int id)
    {
        const string sql = "DELETE FROM Produtos WHERE Id = @Id;";
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            using var comando = new SqliteCommand(sql, conexao);
            comando.Parameters.AddWithValue("@Id", id);

            int linhas = comando.ExecuteNonQuery();
            _logger.Info($"EXCLUIR: Id={id}, linhas afetadas={linhas}.");
            return linhas > 0;
        }
        catch (SqliteException ex)
        {
            _logger.Erro($"EXCLUIR Id={id} falhou", ex);
            throw new DatabaseException("Erro ao excluir o produto.", ex);
        }
    }

    // Mapeamento manual DataReader -> Produto
    private static Produto Mapear(SqliteDataReader leitor)
    {
        return new Produto
        {
            Id = leitor.GetInt32(leitor.GetOrdinal("Id")),
            Nome = leitor.GetString(leitor.GetOrdinal("Nome")),
            Preco = (decimal)leitor.GetDouble(leitor.GetOrdinal("Preco")),
            Estoque = leitor.GetInt32(leitor.GetOrdinal("Estoque")),
            Categoria = leitor.GetString(leitor.GetOrdinal("Categoria"))
        };
    }
}
