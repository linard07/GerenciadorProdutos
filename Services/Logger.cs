namespace GerenciadorProdutos.Services;

/// <summary>Registra as operações da aplicação em arquivo texto.</summary>
public class Logger
{
    private static readonly object _trava = new();
    private readonly string _caminho;

    public Logger(string caminho)
    {
        _caminho = Path.GetFullPath(caminho);
        Directory.CreateDirectory(Path.GetDirectoryName(_caminho)!);
    }

    public void Info(string mensagem) => Escrever("INFO ", mensagem);

    public void Erro(string mensagem, Exception ex) =>
        Escrever("ERRO ", $"{mensagem} | {ex.GetType().Name}: {ex.Message}");

    private void Escrever(string nivel, string mensagem)
    {
        string linha = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{nivel}] {mensagem}{Environment.NewLine}";
        lock (_trava)
        {
            File.AppendAllText(_caminho, linha);
        }
    }
}
