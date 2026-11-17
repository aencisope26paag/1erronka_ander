using System.Windows;
using TpvPartekatua;

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
        Erabiltzailea? erabiltzailea;

        try
        {
            erabiltzailea = Datuak.Login(IzenaTextBox.Text.Trim(), PasahitzaBox.Password);
        }
        catch (Exception ex)
        {
            ErroreaTextBlock.Text = ex.Message;
            return;
        }

        if (erabiltzailea == null)
        {
            ErroreaTextBlock.Text = "Erabiltzaile edo pasahitz okerra.";
            return;
        }

        new MainWindow(erabiltzailea).Show();
        Close();
    }
}