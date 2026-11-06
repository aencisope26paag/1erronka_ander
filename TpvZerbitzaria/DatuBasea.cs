using Microsoft.Data.Sqlite;

namespace TpvZerbitzaria;

public class DatuBasea
{
    public string FitxategiBidea { get; } = Path.Combine(AppContext.BaseDirectory, "tpv.db");

    private string KonexioKatea => new SqliteConnectionStringBuilder
    {
        DataSource = FitxategiBidea
    }.ToString();

    private const string Taulak = """
        CREATE TABLE IF NOT EXISTS Erabiltzaileak (
            Id INTEGER PRIMARY KEY,
            Izena TEXT NOT NULL UNIQUE,
            Pasahitza TEXT NOT NULL,
            Rola TEXT NOT NULL CHECK(Rola IN ('Admin','Erabiltzailea'))
        );
        CREATE TABLE IF NOT EXISTS Produktuak (
            Id INTEGER PRIMARY KEY,
            Izena TEXT NOT NULL UNIQUE,
            Prezioa REAL NOT NULL CHECK(Prezioa >= 0),
            Stocka INTEGER NOT NULL CHECK(Stocka >= 0),
            Kategoria TEXT NOT NULL
        );
        CREATE TABLE IF NOT EXISTS Mahaiak (
            Id INTEGER PRIMARY KEY,
            Zenbakia INTEGER NOT NULL UNIQUE,
            Eserlekuak INTEGER NOT NULL CHECK(Eserlekuak > 0),
            Forma TEXT NOT NULL CHECK(Forma IN ('Biribila','Karratua','Luzea'))
        );
        CREATE TABLE IF NOT EXISTS Erreserbak (
            Id INTEGER PRIMARY KEY,
            MahaiId INTEGER NOT NULL,
            ErabiltzaileId INTEGER NOT NULL,
            Data TEXT NOT NULL,
            Zerbitzua TEXT NOT NULL CHECK(Zerbitzua IN ('Bazkaria','Afaria')),
            UNIQUE (MahaiId, Data, Zerbitzua)
        );
        """;

    // Datos de prueba (solo cuando el fichero de la BD se crea por primera vez)
    private const string HasierakoDatuak = """
        INSERT INTO Erabiltzaileak (Izena, Pasahitza, Rola) VALUES
            ('admin', 'admin123', 'Admin'),
            ('ander', 'ander123', 'Erabiltzailea');
        INSERT INTO Produktuak (Izena, Prezioa, Stocka, Kategoria) VALUES
            ('Estrella Galicia', 2.50, 50, 'Garagardoak'),
            ('Coca-Cola', 2.00, 40, 'Freskagarriak'),
            ('Kafe esnearekin', 1.60, 100, 'Kafeak'),
            ('Oliba-saltsa', 3.00, 20, 'Pintxoak');
        INSERT INTO Mahaiak (Zenbakia, Eserlekuak, Forma) VALUES
            (1, 2, 'Biribila'), (2, 4, 'Karratua'), (3, 6, 'Luzea'),
            (4, 2, 'Karratua'), (5, 4, 'Biribila'), (6, 8, 'Luzea');
        """;

    public void Hasieratu()
    {
        bool berria = !File.Exists(FitxategiBidea);

        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open(); // aquí se crea el fichero .db si no existe

        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = Taulak;
        komandoa.ExecuteNonQuery();

        if (berria)
        {
            komandoa.CommandText = HasierakoDatuak;
            komandoa.ExecuteNonQuery();
        }
    }
}