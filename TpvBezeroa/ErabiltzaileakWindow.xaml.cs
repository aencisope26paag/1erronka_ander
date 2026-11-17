using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TpvPartekatua;

namespace TpvBezeroa;

public partial class ErabiltzaileakWindow : Window
{
    private readonly Erabiltzailea _unekoa;       // el administrador que está usando la ventana
    private Erabiltzailea? _hautatua;              // null = se está creando uno nuevo

    public ErabiltzaileakWindow(Erabiltzailea unekoa)
    {
        InitializeComponent();
        _unekoa = unekoa;

        Taula.ItemsSource = Datuak.Erabiltzaileak();
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
        PasahitzaBox.Text = "";
        RolaCombo.SelectedIndex = 1;
        EzabatuButton.IsEnabled = false;
        IzenaBox.Focus();
    }

    private void Taula_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Taula.SelectedItem is not Erabiltzailea u) return;

        _hautatua = u;
        IzenaBox.Text = u.Izena;
        PasahitzaBox.Text = u.Pasahitza;
        RolaCombo.SelectedIndex = u.Rola == "Admin" ? 0 : 1;
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
        var u = new Erabiltzailea
        {
            Id = _hautatua?.Id ?? 0,
            Izena = IzenaBox.Text.Trim(),
            Pasahitza = PasahitzaBox.Text.Trim(),
            Rola = (string)((ComboBoxItem)RolaCombo.SelectedItem).Content
        };

        var errorea = Datuak.ErabiltzaileaGorde(u);
        if (errorea != null)
        {
            Mezua(errorea, true);
            return;
        }

        Taula.ItemsSource = Datuak.Erabiltzaileak();
        FormularioaGarbitu();
        Mezua("Gordeta.", false);
    }

    private void Ezabatu_Click(object sender, RoutedEventArgs e)
    {
        if (_hautatua == null) return;

        var erantzuna = MessageBox.Show($"'{_hautatua.Izena}' ezabatu nahi duzu?", "Berretsi",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (erantzuna != MessageBoxResult.Yes) return;

        var errorea = Datuak.ErabiltzaileaEzabatu(_hautatua.Id, _unekoa.Id);
        if (errorea != null)
        {
            Mezua(errorea, true);
            return;
        }

        Taula.ItemsSource = Datuak.Erabiltzaileak();
        FormularioaGarbitu();
        Mezua("Ezabatuta.", false);
    }
}