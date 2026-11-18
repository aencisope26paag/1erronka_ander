using System.Windows;
using TpvPartekatua;

namespace TpvBezeroa;

public partial class TpvWindow : Window
{
    private List<TicketLerroa> _tiketa = new List<TicketLerroa>();
    private List<Produktua> _produktuak = new List<Produktua>();

    public TpvWindow()
    {
        InitializeComponent();
        TiketaGrid.ItemsSource = _tiketa;
        _produktuak = Datuak.Produktuak();
        Eguneratu();
    }

    // Busca la línea del ticket de un producto (null si todavía no está)
    private TicketLerroa? LerroaBilatu(long produktuId)
    {
        foreach (TicketLerroa l in _tiketa)
        {
            if (l.ProduktuId == produktuId)
            {
                return l;
            }
        }
        return null;
    }

    private decimal GuztiraKalkulatu()
    {
        decimal guztira = 0;
        foreach (TicketLerroa l in _tiketa)
        {
            guztira = guztira + l.Guztira;
        }
        return guztira;
    }

    // Redibuja las fichas de productos y el ticket
    private void Eguneratu()
    {
        ProduktuPanela.Children.Clear();

        foreach (Produktua p in _produktuak)
        {
            // Stock disponible = stock real - lo que ya está en el ticket
            int tiketan = 0;
            TicketLerroa? lerroa = LerroaBilatu(p.Id);
            if (lerroa != null)
            {
                tiketan = lerroa.Kantitatea;
            }

            ProduktuControl kontrola = new ProduktuControl();
            kontrola.Izena = p.Izena;
            kontrola.Prezioa = p.Prezioa;
            kontrola.Stocka = p.Stocka - tiketan;
            kontrola.Irudia = p.Irudia;
            kontrola.Tag = p;
            kontrola.Aukeratuta += Produktua_Aukeratuta;
            ProduktuPanela.Children.Add(kontrola);
        }

        TiketaGrid.Items.Refresh();
        GuztiraTextBlock.Text = "Guztira: " + GuztiraKalkulatu().ToString("0.00") + " €";
    }

    private void Produktua_Aukeratuta(object? sender, EventArgs e)
    {
        ProduktuControl? kontrola = sender as ProduktuControl;
        if (kontrola == null) return;

        Produktua p = (Produktua)kontrola.Tag;

        TicketLerroa? lerroa = LerroaBilatu(p.Id);
        if (lerroa == null)
        {
            TicketLerroa berria = new TicketLerroa();
            berria.ProduktuId = p.Id;
            berria.Izena = p.Izena;
            berria.Prezioa = p.Prezioa;
            berria.Kantitatea = 1;
            _tiketa.Add(berria);
        }
        else
        {
            lerroa.Kantitatea = lerroa.Kantitatea + 1;
        }

        Eguneratu();
    }

    private void Gutxitu_Click(object sender, RoutedEventArgs e)
    {
        TicketLerroa? lerroa = TiketaGrid.SelectedItem as TicketLerroa;
        if (lerroa == null) return;

        lerroa.Kantitatea = lerroa.Kantitatea - 1;
        if (lerroa.Kantitatea <= 0)
        {
            _tiketa.Remove(lerroa);
        }
        Eguneratu();
    }

    private void Garbitu_Click(object sender, RoutedEventArgs e)
    {
        _tiketa.Clear();
        Eguneratu();
    }

    private void Osatu_Click(object sender, RoutedEventArgs e)
    {
        if (_tiketa.Count == 0)
        {
            MessageBox.Show("Tiketa hutsik dago.");
            return;
        }

        // 1. Se muestra el ticket con productos, cantidades y total
        TiketaWindow tiketaLeihoa = new TiketaWindow(_tiketa);
        tiketaLeihoa.Owner = this;
        bool? emaitza = tiketaLeihoa.ShowDialog();
        if (emaitza != true) return;

        // 2. Compra confirmada: se descuenta el stock
        string mezua;
        if (!Datuak.Erosi(_tiketa, out mezua))
        {
            MessageBox.Show(mezua);
            return;
        }

        _tiketa.Clear();
        _produktuak = Datuak.Produktuak();
        Eguneratu();
        MessageBox.Show("Erosketa baieztatuta. Stocka eguneratu da.");
    }
}