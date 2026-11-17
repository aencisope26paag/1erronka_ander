using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TpvBezeroa;

public partial class ProduktuControl : UserControl
{
    // ---------- Propiedades ----------

    public static readonly DependencyProperty IzenaProperty = DependencyProperty.Register(
        "Izena", typeof(string), typeof(ProduktuControl), new PropertyMetadata("", Aldatuta));

    public static readonly DependencyProperty PrezioaProperty = DependencyProperty.Register(
        "Prezioa", typeof(decimal), typeof(ProduktuControl), new PropertyMetadata(0m, Aldatuta));

    public static readonly DependencyProperty StockaProperty = DependencyProperty.Register(
        "Stocka", typeof(int), typeof(ProduktuControl), new PropertyMetadata(0, Aldatuta));

    public string Izena
    {
        get { return (string)GetValue(IzenaProperty); }
        set { SetValue(IzenaProperty, value); }
    }

    public decimal Prezioa
    {
        get { return (decimal)GetValue(PrezioaProperty); }
        set { SetValue(PrezioaProperty, value); }
    }

    // Unidades disponibles. Con 0, la ficha queda desactivada.
    public int Stocka
    {
        get { return (int)GetValue(StockaProperty); }
        set { SetValue(StockaProperty, value); }
    }

    // ---------- Evento ----------

    // Se lanza cuando el usuario hace clic en un producto con stock.
    public event EventHandler? Aukeratuta;

    public ProduktuControl()
    {
        InitializeComponent();

        MouseLeftButtonDown += Klikatuta;
        MouseEnter += SaguaSartu;
        MouseLeave += SaguaIrten;

        Eguneratu();
    }

    private static void Aldatuta(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ProduktuControl kontrola = (ProduktuControl)d;
        kontrola.Eguneratu();
    }

    // ---------- Ratón ----------

    private void Klikatuta(object sender, MouseButtonEventArgs e)
    {
        if (Stocka > 0)
        {
            Aukeratuta?.Invoke(this, EventArgs.Empty);
        }
    }

    // Efecto hover
    private void SaguaSartu(object sender, MouseEventArgs e)
    {
        if (Stocka > 0)
        {
            Txartela.BorderBrush = Brushes.DodgerBlue;
        }
    }

    private void SaguaIrten(object sender, MouseEventArgs e)
    {
        Txartela.BorderBrush = Brushes.SaddleBrown;
    }

    // ---------- Aspecto ----------

    private void Eguneratu()
    {
        IzenaTestua.Text = Izena;
        PrezioaTestua.Text = Prezioa.ToString("0.00") + " €";

        if (Stocka > 0)
        {
            StockaTestua.Text = "Stock: " + Stocka;
            Txartela.Background = Brushes.Cornsilk;
            Cursor = Cursors.Hand;
        }
        else
        {
            StockaTestua.Text = "Agortuta";
            Txartela.Background = Brushes.LightGray;
            Cursor = Cursors.No;
        }
    }
}