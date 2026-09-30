namespace GerenciadorProdutos.Data;

/// <summary>Exceção amigável lançada pela camada de dados.</summary>
public class DatabaseException : Exception
{
    public DatabaseException(string mensagem, Exception inner) : base(mensagem, inner) { }
}
