using System.Windows;

namespace GerenciadorProdutos;

public partial class App : Application
{
    public App()
    {
        // Banco (produtos.db) e logs ficam ao lado do executável
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
    }
}
