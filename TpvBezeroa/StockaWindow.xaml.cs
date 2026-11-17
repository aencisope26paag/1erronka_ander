using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TpvPartekatua;

namespace TpvBezeroa;

public partial class StockaWindow : Window
{
    private Produktua? _hautatua;   // null = se está creando uno nuevo

    public StockaWindow()
    {
        InitializeComponent();
        Taula.ItemsSource = Datuak.Produktuak();
        FormularioaGarbitu();
    }

    private void Mezua(string testua, bool errorea)
    {
        MezuaTextBlock.Text = testua;
        MezuaTextBlock.Foreground = errorea ? Brushes.Crimson : Brushes.ForestGreen;
    }

    private void FormularioaGarbitu()
    {
        Taula.SelectedItem = null;
        _hautatua = null;
        IzenaBox.Text = "";
        KategoriaBox.Text = "";
        PrezioaBox.Text = "";
        StockaBox.Text = "";
        EzabatuButton.IsEnabled = false;
        IzenaBox.Focus();
    }

    private void Taula_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Taula.SelectedItem is not Produktua p) return;

        _hautatua = p;
        IzenaBox.Text = p.Izena;
        KategoriaBox.Text = p.Kategoria;
        PrezioaBox.Text = p.Prezioa.ToString("0.00");
        StockaBox.Text = p.Stocka.ToString();
        EzabatuButton.IsEnabled = true;
        MezuaTextBlock.Text = "";
    }

    private void Berria_Click(object sender, RoutedEventArgs e)
    {
        FormularioaGarbitu();
        MezuaTextBlock.Text = "";
    }

    private void Gorde_Click(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(PrezioaBox.Text, out decimal prezioa))
        {
            Mezua("Prezioa ez da zuzena (adib. 2,50).", true);
            return;
        }
        if (!int.TryParse(StockaBox.Text, out int stocka))
        {
            Mezua("Stocka zenbaki oso bat izan behar da.", true);
            return;
        }

        var p = new Produktua
        {
            Id = _hautatua?.Id ?? 0,
            Izena = IzenaBox.Text.Trim(),
            Kategoria = KategoriaBox.Text.Trim(),
            Prezioa = prezioa,
            Stocka = stocka
        };

        var errorea = Datuak.ProduktuaGorde(p);
        if (errorea != null)
        {
            Mezua(errorea, true);
            return;
        }

        Taula.ItemsSource = Datuak.Produktuak();
        FormularioaGarbitu();
        Mezua("Gordeta.", false);
    }

    private void Ezabatu_Click(object sender, RoutedEventArgs e)
    {
        if (_hautatua == null) return;

        var erantzuna = MessageBox.Show($"'{_hautatua.Izena}' ezabatu nahi duzu?", "Berretsi",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (erantzuna != MessageBoxResult.Yes) return;

        Datuak.ProduktuaEzabatu(_hautatua.Id);
        Taula.ItemsSource = Datuak.Produktuak();
        FormularioaGarbitu();
        Mezua("Ezabatuta.", false);
    }
}