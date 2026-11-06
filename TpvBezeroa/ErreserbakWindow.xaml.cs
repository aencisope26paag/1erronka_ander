using System.Windows;
using System.Windows.Controls;
using TpvPartekatua;

namespace TpvBezeroa;

public partial class ErreserbakWindow : Window
{
    private readonly Erabiltzailea _erabiltzailea;
    private Mahaia? _hautatutakoa;

    public ErreserbakWindow(Erabiltzailea erabiltzailea)
    {
        InitializeComponent();
        _erabiltzailea = erabiltzailea;
        ErabiltzaileaTextBlock.Text = "Erabiltzailea: " + _erabiltzailea.Izena;

        DataPicker.SelectedDate = DateTime.Today;
        ZerbitzuaCombo.SelectedIndex = 0;

        // Se conectan después de dar los valores iniciales, para que no se disparen antes de tiempo.
        DataPicker.SelectedDateChanged += (s, e) => Eguneratu();
        ZerbitzuaCombo.SelectionChanged += (s, e) => Eguneratu();

        Eguneratu();
    }

    // Si se borra la fecha, se usa la de hoy.
    private DateTime Data => DataPicker.SelectedDate ?? DateTime.Today;
    private string Zerbitzua => (string)((ComboBoxItem)ZerbitzuaCombo.SelectedItem).Content;

    // Redibuja todas las mesas según la fecha y el servicio elegidos
    private void Eguneratu()
    {
        MahaiPanela.Children.Clear();

        foreach (var mahaia in Datuak.Mahaiak())
        {
            var kontrola = new MahaiControl
            {
                Zenbakia = mahaia.Zenbakia,
                Eserlekuak = mahaia.Eserlekuak,
                Forma = mahaia.Forma,
                Erreserbatuta = Datuak.ErreserbaLortu(mahaia.Id, Data, Zerbitzua) != null,
                Hautatuta = _hautatutakoa?.Id == mahaia.Id,
                Tag = mahaia
            };
            kontrola.Aukeratuta += Mahaia_Aukeratuta;
            MahaiPanela.Children.Add(kontrola);
        }

        ErreserbenZerrenda.ItemsSource = Datuak.ErreserbenZerrenda(Data, Zerbitzua);
        HautatutakoaErakutsi();
    }

    private void HautatutakoaErakutsi()
    {
        if (_hautatutakoa == null)
        {
            HautatutakoaTextBlock.Text = "Hautatutako mahaia: -";
            EdukieraTextBlock.Text = "";
            ErreserbaButton.IsEnabled = false;
            EzabatuButton.IsEnabled = false;
            return;
        }

        bool erreserbatuta = Datuak.ErreserbaLortu(_hautatutakoa.Id, Data, Zerbitzua) != null;
        HautatutakoaTextBlock.Text = $"Hautatutako mahaia: {_hautatutakoa.Zenbakia:00}";
        EdukieraTextBlock.Text = $"Edukiera: {_hautatutakoa.Eserlekuak} eserleku";
        ErreserbaButton.IsEnabled = !erreserbatuta;
        EzabatuButton.IsEnabled = erreserbatuta;
    }

    private void Mahaia_Aukeratuta(object? sender, EventArgs e)
    {
        _hautatutakoa = (Mahaia)((MahaiControl)sender!).Tag;
        Eguneratu();
    }

    private void Erreserba_Click(object sender, RoutedEventArgs e)
    {
        if (_hautatutakoa == null) return;

        if (!Datuak.ErreserbaEgin(_hautatutakoa.Id, _erabiltzailea.Id, Data, Zerbitzua))
            MessageBox.Show("Mahai hau jada erreserbatuta dago.");

        Eguneratu();
    }

    private void Ezabatu_Click(object sender, RoutedEventArgs e)
    {
        if (_hautatutakoa == null) return;

        Datuak.ErreserbaEzabatu(_hautatutakoa.Id, Data, Zerbitzua);
        Eguneratu();
    }
}