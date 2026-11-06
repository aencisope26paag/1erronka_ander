using System.Windows;
using TpvPartekatua;

namespace TpvBezeroa;

public partial class MainWindow : Window
{
    private readonly Erabiltzailea _erabiltzailea;

    public MainWindow(Erabiltzailea erabiltzailea)
    {
        InitializeComponent();
        _erabiltzailea = erabiltzailea;

        KaixoTextBlock.Text = $"Kaixo, {_erabiltzailea.Izena} ({_erabiltzailea.Rola})";

        // El administrador ve usuarios y stock; el usuario común, TPV y reservas.
        bool admin = _erabiltzailea.Rola == "Admin";
        ErabiltzaileakButton.Visibility = admin ? Visibility.Visible : Visibility.Collapsed;
        StockaButton.Visibility = admin ? Visibility.Visible : Visibility.Collapsed;
        TpvButton.Visibility = admin ? Visibility.Collapsed : Visibility.Visible;
        ErreserbakButton.Visibility = admin ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Tpv_Click(object sender, RoutedEventArgs e) => MessageBox.Show("TPV pantaila hemen egongo da.");
    private void Erreserbak_Click(object sender, RoutedEventArgs e)
        => new ErreserbakWindow(_erabiltzailea) { Owner = this }.ShowDialog(); private void Erabiltzaileak_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Erabiltzaileen kudeaketa hemen egongo da.");
    private void Stocka_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Stockaren kudeaketa hemen egongo da.");
}