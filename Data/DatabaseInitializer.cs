using Microsoft.Data.Sqlite;

namespace GerenciadorProdutos.Data;

/// <summary>Executa o script .sql para garantir que a tabela Produtos exista.</summary>
public static class DatabaseInitializer
{
    public static void CriarTabela(string connectionString, string caminhoScript)
    {
        try
        {
            string sql = File.ReadAllText(caminhoScript);

            using var conexao = new SqliteConnection(connectionString);
            conexao.Open();

            using var comando = new SqliteCommand(sql, conexao);
            comando.ExecuteNonQuery();
        }
        catch (SqliteException ex)
        {
            throw new DatabaseException("Não foi possível criar/verificar a tabela Produtos.", ex);
        }
        catch (IOException ex)
        {
            throw new DatabaseException("Não foi possível ler o script SQL de criação da tabela.", ex);
        }
    }
}
