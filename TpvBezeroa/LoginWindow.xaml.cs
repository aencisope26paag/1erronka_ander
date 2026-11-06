using System.Windows;

namespace TpvBezeroa;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        IzenaTextBox.Focus();
    }

    private void Sartu_Click(object sender, RoutedEventArgs e)
    {
        var erabiltzailea = Datuak.Login(IzenaTextBox.Text.Trim(), PasahitzaBox.Password);
        if (erabiltzailea == null)
        {
            ErroreaTextBlock.Text = "Erabiltzaile edo pasahitz okerra.";
            return;
        }

        new MainWindow(erabiltzailea).Show();
        Close();
    }
}