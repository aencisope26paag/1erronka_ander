using System.Windows;
using System.Windows.Controls;
using TpvPartekatua;

namespace TpvBezeroa;

public partial class ErreserbakWindow : Window
{
    private Erabiltzailea _erabiltzailea;
    private Mahaia? _hautatutakoa;

    // Si no es null, estamos en "modo modificar" con esta reserva
    private Erreserba? _aldatzen = null;
    private string _aldatzenTestua = "";

    public ErreserbakWindow(Erabiltzailea erabiltzailea)
    {
        InitializeComponent();
        _erabiltzailea = erabiltzailea;
        ErabiltzaileaTextBlock.Text = "Erabiltzailea: " + _erabiltzailea.Izena;

        DataPicker.SelectedDate = DateTime.Today;
        ZerbitzuaCombo.SelectedIndex = 0;

        // Se conectan después de dar los valores iniciales, para que no se disparen antes de tiempo.
        DataPicker.SelectedDateChanged += Data_Aldatuta;
        ZerbitzuaCombo.SelectionChanged += Zerbitzua_Aldatuta;

        Eguneratu();
    }

    private void Data_Aldatuta(object? sender, SelectionChangedEventArgs e)
    {
        Eguneratu();
    }

    private void Zerbitzua_Aldatuta(object sender, SelectionChangedEventArgs e)
    {
        Eguneratu();
    }

    // Fecha elegida (si se borra, se usa la de hoy)
    private DateTime Data
    {
        get
        {
            if (DataPicker.SelectedDate == null)
            {
                return DateTime.Today;
            }
            return DataPicker.SelectedDate.Value;
        }
    }

    private string Zerbitzua
    {
        get
        {
            ComboBoxItem item = (ComboBoxItem)ZerbitzuaCombo.SelectedItem;
            return (string)item.Content;
        }
    }

    // Redibuja todas las mesas según la fecha y el servicio elegidos
    private void Eguneratu()
    {
        MahaiPanela.Children.Clear();

        foreach (Mahaia mahaia in Datuak.Mahaiak())
        {
            MahaiControl kontrola = new MahaiControl();
            kontrola.Zenbakia = mahaia.Zenbakia;
            kontrola.Eserlekuak = mahaia.Eserlekuak;
            kontrola.Forma = mahaia.Forma;
            kontrola.Erreserbatuta = Datuak.ErreserbaLortu(mahaia.Id, Data, Zerbitzua) != null;
            kontrola.Hautatuta = _hautatutakoa != null && _hautatutakoa.Id == mahaia.Id;
            kontrola.Tag = mahaia;
            kontrola.Aukeratuta += Mahaia_Aukeratuta;
            MahaiPanela.Children.Add(kontrola);
        }

        ErreserbenZerrenda.ItemsSource = Datuak.ErreserbenZerrenda(Data, Zerbitzua);
        HautatutakoaErakutsi();
    }

    // Actualiza el panel derecho: textos y qué botones están activos
    // Actualiza el panel derecho: textos y qué botones están activos
    private void HautatutakoaErakutsi()
    {
        bool erreserbatuta = false;
        bool nirea = false;   // true si la reserva es del usuario actual

        if (_hautatutakoa == null)
        {
            HautatutakoaTextBlock.Text = "Hautatutako mahaia: -";
            EdukieraTextBlock.Text = "";
        }
        else
        {
            HautatutakoaTextBlock.Text = "Hautatutako mahaia: " + _hautatutakoa.Zenbakia.ToString("00");
            EdukieraTextBlock.Text = "Edukiera: " + _hautatutakoa.Eserlekuak + " eserleku";

            Erreserba? erreserba = Datuak.ErreserbaLortu(_hautatutakoa.Id, Data, Zerbitzua);
            if (erreserba != null)
            {
                erreserbatuta = true;
                nirea = erreserba.ErabiltzaileId == _erabiltzailea.Id;
            }
        }

        bool hautatuta = _hautatutakoa != null;

        if (_aldatzen == null)
        {
            // Modo normal
            ErreserbaButton.Content = "Erreserba egin";
            AldatuButton.Content = "Erreserba aldatu";

            if (erreserbatuta && !nirea)
            {
                AldaketaTextBlock.Text = "Erreserba hau beste erabiltzaile batena da: ezin duzu aldatu edo ezabatu.";
            }
            else
            {
                AldaketaTextBlock.Text = "";
            }

            ErreserbaButton.IsEnabled = hautatuta && !erreserbatuta;
            AldatuButton.IsEnabled = hautatuta && erreserbatuta && nirea;
            EzabatuButton.IsEnabled = hautatuta && erreserbatuta && nirea;
        }
        else
        {
            // Modo modificar
            ErreserbaButton.Content = "Aldaketa berretsi";
            AldatuButton.Content = "Aldaketa utzi";
            AldaketaTextBlock.Text = "Aldatzen: " + _aldatzenTestua +
                ". Aukeratu mahai libre bat (data edo zerbitzua ere alda ditzakezu) eta sakatu 'Aldaketa berretsi'.";

            ErreserbaButton.IsEnabled = hautatuta && !erreserbatuta;
            AldatuButton.IsEnabled = true;
            EzabatuButton.IsEnabled = false;
        }
    }

    private void Mahaia_Aukeratuta(object? sender, EventArgs e)
    {
        MahaiControl? kontrola = sender as MahaiControl;
        if (kontrola == null) return;

        _hautatutakoa = (Mahaia)kontrola.Tag;
        Eguneratu();
    }

    // "Erreserba egin" en modo normal; "Aldaketa berretsi" en modo modificar
    private void Erreserba_Click(object sender, RoutedEventArgs e)
    {
        if (_hautatutakoa == null) return;

        if (_aldatzen == null)
        {
            bool ondo = Datuak.ErreserbaEgin(_hautatutakoa.Id, _erabiltzailea.Id, Data, Zerbitzua);
            if (!ondo)
            {
                MessageBox.Show("Mahai hau jada erreserbatuta dago.");
            }
        }
        else
        {
            string? errorea = Datuak.ErreserbaAldatu(_aldatzen.Id, _hautatutakoa.Id, Data, Zerbitzua);
            if (errorea != null)
            {
                MessageBox.Show(errorea);
            }
            else
            {
                _aldatzen = null;
                MessageBox.Show("Erreserba aldatuta.");
            }
        }

        Eguneratu();
    }

    // "Erreserba aldatu" empieza a modificar; "Aldaketa utzi" lo cancela
    // "Erreserba aldatu" empieza a modificar; "Aldaketa utzi" lo cancela
    private void Aldatu_Click(object sender, RoutedEventArgs e)
    {
        if (_aldatzen != null)
        {
            _aldatzen = null;
            Eguneratu();
            return;
        }

        if (_hautatutakoa == null) return;

        Erreserba? erreserba = Datuak.ErreserbaLortu(_hautatutakoa.Id, Data, Zerbitzua);
        if (erreserba == null) return;

        if (erreserba.ErabiltzaileId != _erabiltzailea.Id)
        {
            MessageBox.Show("Zure erreserbak soilik alda ditzakezu.");
            return;
        }

        _aldatzen = erreserba;
        _aldatzenTestua = _hautatutakoa.Zenbakia + ". mahaia, " + Data.ToString("yyyy-MM-dd") + ", " + Zerbitzua;
        _hautatutakoa = null; // hay que elegir la mesa nueva
        Eguneratu();
    }

    private void Ezabatu_Click(object sender, RoutedEventArgs e)
    {
        if (_hautatutakoa == null) return;

        Erreserba? erreserba = Datuak.ErreserbaLortu(_hautatutakoa.Id, Data, Zerbitzua);
        if (erreserba == null) return;

        if (erreserba.ErabiltzaileId != _erabiltzailea.Id)
        {
            MessageBox.Show("Zure erreserbak soilik ezaba ditzakezu.");
            return;
        }

        Datuak.ErreserbaEzabatu(_hautatutakoa.Id, Data, Zerbitzua);
        Eguneratu();
    }
}