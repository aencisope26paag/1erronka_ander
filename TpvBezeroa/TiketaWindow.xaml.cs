using System.Text;
using System.Windows;
using System.Windows.Controls;
using TpvPartekatua;

namespace TpvBezeroa;

public partial class TiketaWindow : Window
{
    public TiketaWindow(List<TicketLerroa> lerroak)
    {
        InitializeComponent();

        StringBuilder sb = new StringBuilder();
        decimal guztira = 0;

        sb.AppendLine("ELKARTE GASTRONOMIKOA");
        sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        sb.AppendLine(new string('-', 34));

        foreach (TicketLerroa l in lerroak)
        {
            string izena = l.Izena;
            if (izena.Length > 16)
            {
                izena = izena.Substring(0, 16);
            }

            sb.AppendLine(izena.PadRight(16) + " x" + l.Kantitatea.ToString().PadRight(3)
                          + " " + l.Guztira.ToString("0.00").PadLeft(9) + " €");
            guztira = guztira + l.Guztira;
        }

        sb.AppendLine(new string('-', 34));
        sb.AppendLine("GUZTIRA:".PadRight(22) + " " + guztira.ToString("0.00").PadLeft(9) + " €");

        TiketaTestua.Text = sb.ToString();
    }

    private void Inprimatu_Click(object sender, RoutedEventArgs e)
    {
        PrintDialog dialogoa = new PrintDialog();
        if (dialogoa.ShowDialog() == true)
        {
            dialogoa.PrintVisual(TiketaBorder, "Tiketa");
        }
    }

    // Cierra la ventana devolviendo "true": la compra queda confirmada
    private void Baieztatu_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}