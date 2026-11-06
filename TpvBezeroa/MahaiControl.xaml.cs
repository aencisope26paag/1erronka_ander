using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TpvBezeroa;

public partial class MahaiControl : UserControl
{
    // ---------- Propiedades ----------

    public static readonly DependencyProperty ZenbakiaProperty = DependencyProperty.Register(
        nameof(Zenbakia), typeof(int), typeof(MahaiControl), new PropertyMetadata(1, Aldatuta));

    public static readonly DependencyProperty EserlekuakProperty = DependencyProperty.Register(
        nameof(Eserlekuak), typeof(int), typeof(MahaiControl), new PropertyMetadata(2, Aldatuta));

    public static readonly DependencyProperty FormaProperty = DependencyProperty.Register(
        nameof(Forma), typeof(string), typeof(MahaiControl), new PropertyMetadata("Karratua", Aldatuta));

    public static readonly DependencyProperty ErreserbatutaProperty = DependencyProperty.Register(
        nameof(Erreserbatuta), typeof(bool), typeof(MahaiControl), new PropertyMetadata(false, Aldatuta));

    public static readonly DependencyProperty HautatutaProperty = DependencyProperty.Register(
        nameof(Hautatuta), typeof(bool), typeof(MahaiControl), new PropertyMetadata(false, Aldatuta));

    public int Zenbakia { get => (int)GetValue(ZenbakiaProperty); set => SetValue(ZenbakiaProperty, value); }
    public int Eserlekuak { get => (int)GetValue(EserlekuakProperty); set => SetValue(EserlekuakProperty, value); }
    public string Forma { get => (string)GetValue(FormaProperty); set => SetValue(FormaProperty, value); }
    public bool Erreserbatuta { get => (bool)GetValue(ErreserbatutaProperty); set => SetValue(ErreserbatutaProperty, value); }
    public bool Hautatuta { get => (bool)GetValue(HautatutaProperty); set => SetValue(HautatutaProperty, value); }

    // ---------- Evento ----------

    /// <summary>Se lanza cuando el usuario hace clic en la mesa.</summary>
    public event EventHandler? Aukeratuta;

    public MahaiControl()
    {
        InitializeComponent();

        MouseLeftButtonDown += (s, e) => Aukeratuta?.Invoke(this, EventArgs.Empty);
        MouseEnter += (s, e) => Txartela.Opacity = 0.8;   // efecto hover
        MouseLeave += (s, e) => Txartela.Opacity = 1;

        Eguneratu();
    }

    private static void Aldatuta(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((MahaiControl)d).Eguneratu();

    // ---------- Aspecto ----------

    private void Eguneratu()
    {
        ZenbakiaTestua.Text = Zenbakia.ToString();
        Testua.Text = $"{Eserlekuak} eserleku";

        switch (Forma)
        {
            case "Biribila":
                MahaiForma.Width = 60; MahaiForma.Height = 60;
                MahaiForma.CornerRadius = new CornerRadius(30);
                break;
            case "Luzea":
                MahaiForma.Width = 110; MahaiForma.Height = 35;
                MahaiForma.CornerRadius = new CornerRadius(4);
                break;
            default: // Karratua
                MahaiForma.Width = 60; MahaiForma.Height = 60;
                MahaiForma.CornerRadius = new CornerRadius(4);
                break;
        }

        Txartela.Background = Erreserbatuta ? Brushes.LightPink : Brushes.LightGreen;
        Txartela.BorderBrush = Hautatuta ? Brushes.DodgerBlue
                             : Erreserbatuta ? Brushes.Firebrick
                             : Brushes.Green;
    }
}