using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GerenciadorProdutos.Data;
using GerenciadorProdutos.Models;
using GerenciadorProdutos.Services;
using Microsoft.Extensions.Configuration;

namespace GerenciadorProdutos;

public partial class MainWindow : Window
{
    private readonly CultureInfo _ptBR = new("pt-BR");
    private ProdutoRepository _repositorio = null!;
    private Logger _logger = null!;

    public MainWindow()
    {
        InitializeComponent();

        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada no appsettings.json.");
            string caminhoLog = config["Log:Arquivo"] ?? "logs/operacoes.log";

            _logger = new Logger(caminhoLog);

            string script = Path.Combine(AppContext.BaseDirectory, "Database", "criar_tabela.sql");
            DatabaseInitializer.CriarTabela(connectionString, script);

            _repositorio = new ProdutoRepository(connectionString, _logger);
            _logger.Info("Aplicação iniciada.");

            Closed += (_, _) => _logger.Info("Aplicação encerrada.");

            int total = CarregarLista();
            Status($"Pronto. {total} produto(s) cadastrado(s).");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível iniciar a aplicação:\n{ex.Message}",
                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }

    // ------------------------- Botões do menu -------------------------

    // 1. Inserir produto
    private void BtnInserir_Click(object sender, RoutedEventArgs e)
    {
        if (!TryLerProduto(out Produto produto)) return;

        try
        {
            int id = _repositorio.Inserir(produto);
            CarregarLista();
            LimparCampos();
            Status($"Produto inserido com sucesso! Id = {id}");
        }
        catch (DatabaseException ex)
        {
            MostrarErroBanco(ex);
        }
    }

    // 2. Listar produtos
    private void BtnListar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            int total = CarregarLista();
            Status(total == 0 ? "Nenhum produto cadastrado." : $"{total} produto(s) listado(s).");
        }
        catch (DatabaseException ex)
        {
            MostrarErroBanco(ex);
        }
    }

    // 3. Buscar produto por ID
    private void BtnBuscar_Click(object sender, RoutedEventArgs e)
    {
        if (!TryLerId(out int id)) return;

        try
        {
            Produto? produto = _repositorio.BuscarPorId(id);
            if (produto is null)
            {
                Status($"Produto com Id {id} não encontrado.", erro: true);
                MessageBox.Show($"Produto com Id {id} não encontrado.", "Busca",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            PreencherCampos(produto);
            if (dgProdutos.ItemsSource is List<Produto> lista)
            {
                dgProdutos.SelectedItem = lista.FirstOrDefault(p => p.Id == produto.Id);
            }
            Status($"Produto {produto.Id} encontrado: {produto.Nome}.");
        }
        catch (DatabaseException ex)
        {
            MostrarErroBanco(ex);
        }
    }

    // 4. Atualizar produto
    private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
    {
        if (!TryLerId(out int id)) return;
        if (!TryLerProduto(out Produto produto)) return;
        produto.Id = id;

        try
        {
            bool ok = _repositorio.Atualizar(produto);
            if (!ok)
            {
                Status($"Produto com Id {id} não encontrado. Nada foi atualizado.", erro: true);
                return;
            }

            CarregarLista();
            Status($"Produto {id} atualizado com sucesso!");
        }
        catch (DatabaseException ex)
        {
            MostrarErroBanco(ex);
        }
    }

    // 5. Excluir produto
    private void BtnExcluir_Click(object sender, RoutedEventArgs e)
    {
        if (!TryLerId(out int id)) return;

        try
        {
            Produto? produto = _repositorio.BuscarPorId(id);
            if (produto is null)
            {
                Status($"Produto com Id {id} não encontrado.", erro: true);
                return;
            }

            var resposta = MessageBox.Show($"Deseja realmente excluir '{produto.Nome}'?",
                "Confirmar exclusão", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resposta != MessageBoxResult.Yes) return;

            _repositorio.Excluir(id);
            CarregarLista();
            LimparCampos();
            Status($"Produto {id} excluído com sucesso!");
        }
        catch (DatabaseException ex)
        {
            MostrarErroBanco(ex);
        }
    }

    // 6. Sair
    private void BtnSair_Click(object sender, RoutedEventArgs e) => Close();

    // Clicar numa linha da tabela preenche os campos
    private void DgProdutos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgProdutos.SelectedItem is Produto produto)
        {
            PreencherCampos(produto);
        }
    }

    // ------------------------- Auxiliares -------------------------

    private int CarregarLista()
    {
        List<Produto> produtos = _repositorio.Listar();
        dgProdutos.ItemsSource = produtos;
        return produtos.Count;
    }

    private void PreencherCampos(Produto p)
    {
        txtId.Text = p.Id.ToString();
        txtNome.Text = p.Nome;
        txtPreco.Text = p.Preco.ToString("N2", _ptBR);
        txtEstoque.Text = p.Estoque.ToString();
        txtCategoria.Text = p.Categoria;
    }

    private void LimparCampos()
    {
        txtId.Clear();
        txtNome.Clear();
        txtPreco.Clear();
        txtEstoque.Clear();
        txtCategoria.Clear();
        txtNome.Focus();
    }

    private bool TryLerId(out int id)
    {
        if (int.TryParse(txtId.Text.Trim(), out id) && id > 0) return true;

        Aviso("Informe um Id válido (número inteiro maior que zero).");
        txtId.Focus();
        return false;
    }

    private bool TryLerProduto(out Produto produto)
    {
        produto = new Produto();

        string nome = txtNome.Text.Trim();
        string categoria = txtCategoria.Text.Trim();

        if (nome.Length == 0)
        {
            Aviso("Informe o nome do produto.");
            txtNome.Focus();
            return false;
        }

        string precoTexto = txtPreco.Text.Trim().Replace('.', ',');
        if (!decimal.TryParse(precoTexto, NumberStyles.Number, _ptBR, out decimal preco) || preco < 0)
        {
            Aviso("Preço inválido. Use um número maior ou igual a zero (ex: 19,90).");
            txtPreco.Focus();
            return false;
        }

        if (!int.TryParse(txtEstoque.Text.Trim(), out int estoque) || estoque < 0)
        {
            Aviso("Estoque inválido. Use um número inteiro maior ou igual a zero.");
            txtEstoque.Focus();
            return false;
        }

        if (categoria.Length == 0)
        {
            Aviso("Informe a categoria do produto.");
            txtCategoria.Focus();
            return false;
        }

        produto.Nome = nome;
        produto.Preco = preco;
        produto.Estoque = estoque;
        produto.Categoria = categoria;
        return true;
    }

    private void Aviso(string mensagem)
    {
        Status(mensagem, erro: true);
        MessageBox.Show(mensagem, "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void MostrarErroBanco(DatabaseException ex)
    {
        Status(ex.Message, erro: true);
        MessageBox.Show($"{ex.Message}\n\nOs detalhes foram registrados no arquivo de log.",
            "Erro de banco de dados", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void Status(string mensagem, bool erro = false)
    {
        txtStatus.Text = mensagem;
        txtStatus.Foreground = erro ? Brushes.Firebrick : Brushes.DarkGreen;
    }
}
