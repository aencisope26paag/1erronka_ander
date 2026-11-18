using Microsoft.Data.Sqlite;
using TpvPartekatua;

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
            Kategoria TEXT NOT NULL,
            Irudia BLOB
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

    // ---------- LOCK BAKARRA ----------
    // Todos los métodos públicos meten su cuerpo dentro de este lock:
    // solo un cliente a la vez toca la base de datos.
    private static readonly object _blokeoa = new object();

    private SqliteConnection Ireki()
    {
        SqliteConnection konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();
        return konexioa;
    }

    // ---------- Erabiltzaileak ----------

    public Erabiltzailea? Login(string izena, string pasahitza)
    {
        lock (_blokeoa)
        {
            using var konexioa = Ireki();
            using var komandoa = konexioa.CreateCommand();
            komandoa.CommandText = "SELECT Id, Izena, Rola FROM Erabiltzaileak WHERE Izena = $i AND Pasahitza = $p;";
            komandoa.Parameters.AddWithValue("$i", izena);
            komandoa.Parameters.AddWithValue("$p", pasahitza);

            using var irakurlea = komandoa.ExecuteReader();
            if (irakurlea.Read())
            {
                Erabiltzailea u = new Erabiltzailea();
                u.Id = irakurlea.GetInt64(0);
                u.Izena = irakurlea.GetString(1);
                u.Rola = irakurlea.GetString(2);
                return u;
            }
            return null;
        }
    }

    public List<Erabiltzailea> Erabiltzaileak()
    {
        List<Erabiltzailea> zerrenda = new List<Erabiltzailea>();
        lock (_blokeoa)
        {
            using var konexioa = Ireki();
            using var komandoa = konexioa.CreateCommand();
            komandoa.CommandText = "SELECT Id, Izena, Pasahitza, Rola FROM Erabiltzaileak ORDER BY Id;";

            using var irakurlea = komandoa.ExecuteReader();
            while (irakurlea.Read())
            {
                Erabiltzailea u = new Erabiltzailea();
                u.Id = irakurlea.GetInt64(0);
                u.Izena = irakurlea.GetString(1);
                u.Pasahitza = irakurlea.GetString(2);
                u.Rola = irakurlea.GetString(3);
                zerrenda.Add(u);
            }
        }
        return zerrenda;
    }

    // Crea (Id = 0) o modifica un usuario. Devuelve un mensaje de error, o null si todo va bien.
    public string? ErabiltzaileaGorde(Erabiltzailea e)
    {
        if (string.IsNullOrWhiteSpace(e.Izena) || string.IsNullOrWhiteSpace(e.Pasahitza))
        {
            return "Izena eta pasahitza beharrezkoak dira.";
        }

        lock (_blokeoa)
        {
            using var konexioa = Ireki();

            using var galdera = konexioa.CreateCommand();
            galdera.CommandText = "SELECT COUNT(*) FROM Erabiltzaileak WHERE Izena = $i AND Id <> $id;";
            galdera.Parameters.AddWithValue("$i", e.Izena);
            galdera.Parameters.AddWithValue("$id", e.Id);
            long kopurua = Convert.ToInt64(galdera.ExecuteScalar());
            if (kopurua > 0)
            {
                return "Izen hori hartuta dago.";
            }

            using var komandoa = konexioa.CreateCommand();
            if (e.Id == 0)
            {
                komandoa.CommandText = "INSERT INTO Erabiltzaileak (Izena, Pasahitza, Rola) VALUES ($i, $p, $r);";
            }
            else
            {
                komandoa.CommandText = "UPDATE Erabiltzaileak SET Izena = $i, Pasahitza = $p, Rola = $r WHERE Id = $id;";
                komandoa.Parameters.AddWithValue("$id", e.Id);
            }
            komandoa.Parameters.AddWithValue("$i", e.Izena);
            komandoa.Parameters.AddWithValue("$p", e.Pasahitza);
            komandoa.Parameters.AddWithValue("$r", e.Rola);
            komandoa.ExecuteNonQuery();
            return null;
        }
    }

    // Borra un usuario y sus reservas. No se puede borrar a uno mismo.
    public string? ErabiltzaileaEzabatu(long id, long unekoaId)
    {
        if (id == unekoaId)
        {
            return "Ezin duzu zure burua ezabatu.";
        }

        lock (_blokeoa)
        {
            using var konexioa = Ireki();
            using var komandoa = konexioa.CreateCommand();
            komandoa.CommandText = """
                DELETE FROM Erreserbak WHERE ErabiltzaileId = $id;
                DELETE FROM Erabiltzaileak WHERE Id = $id;
                """;
            komandoa.Parameters.AddWithValue("$id", id);
            komandoa.ExecuteNonQuery();
            return null;
        }
    }

    // ---------- Mahaiak ----------

    public List<Mahaia> Mahaiak()
    {
        List<Mahaia> zerrenda = new List<Mahaia>();
        lock (_blokeoa)
        {
            using var konexioa = Ireki();
            using var komandoa = konexioa.CreateCommand();
            komandoa.CommandText = "SELECT Id, Zenbakia, Eserlekuak, Forma FROM Mahaiak ORDER BY Zenbakia;";

            using var irakurlea = komandoa.ExecuteReader();
            while (irakurlea.Read())
            {
                Mahaia m = new Mahaia();
                m.Id = irakurlea.GetInt64(0);
                m.Zenbakia = irakurlea.GetInt32(1);
                m.Eserlekuak = irakurlea.GetInt32(2);
                m.Forma = irakurlea.GetString(3);
                zerrenda.Add(m);
            }
        }
        return zerrenda;
    }

    // ---------- Erreserbak ----------

    public Erreserba? ErreserbaLortu(long mahaiId, DateTime data, string zerbitzua)
    {
        lock (_blokeoa)
        {
            using var konexioa = Ireki();
            using var komandoa = konexioa.CreateCommand();
            komandoa.CommandText = "SELECT Id, MahaiId, ErabiltzaileId, Data, Zerbitzua FROM Erreserbak " +
                                   "WHERE MahaiId = $m AND Data = $d AND Zerbitzua = $z;";
            komandoa.Parameters.AddWithValue("$m", mahaiId);
            komandoa.Parameters.AddWithValue("$d", data.ToString("yyyy-MM-dd"));
            komandoa.Parameters.AddWithValue("$z", zerbitzua);

            using var irakurlea = komandoa.ExecuteReader();
            if (irakurlea.Read())
            {
                Erreserba r = new Erreserba();
                r.Id = irakurlea.GetInt64(0);
                r.MahaiId = irakurlea.GetInt64(1);
                r.ErabiltzaileId = irakurlea.GetInt64(2);
                r.Data = DateTime.Parse(irakurlea.GetString(3));
                r.Zerbitzua = irakurlea.GetString(4);
                return r;
            }
            return null;
        }
    }

    public bool ErreserbaEgin(long mahaiId, long erabiltzaileId, DateTime data, string zerbitzua)
    {
        lock (_blokeoa)
        {
            // (El lock es del mismo hilo, así que llamar a otro método con lock no da problemas.)
            if (ErreserbaLortu(mahaiId, data, zerbitzua) != null)
            {
                return false; // ya está ocupada
            }

            using var konexioa = Ireki();
            using var komandoa = konexioa.CreateCommand();
            komandoa.CommandText = "INSERT INTO Erreserbak (MahaiId, ErabiltzaileId, Data, Zerbitzua) VALUES ($m, $e, $d, $z);";
            komandoa.Parameters.AddWithValue("$m", mahaiId);
            komandoa.Parameters.AddWithValue("$e", erabiltzaileId);
            komandoa.Parameters.AddWithValue("$d", data.ToString("yyyy-MM-dd"));
            komandoa.Parameters.AddWithValue("$z", zerbitzua);
            komandoa.ExecuteNonQuery();
            return true;
        }
    }

    public void ErreserbaEzabatu(long mahaiId, DateTime data, string zerbitzua)
    {
        lock (_blokeoa)
        {
            using var konexioa = Ireki();
            using var komandoa = konexioa.CreateCommand();
            komandoa.CommandText = "DELETE FROM Erreserbak WHERE MahaiId = $m AND Data = $d AND Zerbitzua = $z;";
            komandoa.Parameters.AddWithValue("$m", mahaiId);
            komandoa.Parameters.AddWithValue("$d", data.ToString("yyyy-MM-dd"));
            komandoa.Parameters.AddWithValue("$z", zerbitzua);
            komandoa.ExecuteNonQuery();
        }
    }

    // Modifica una reserva: la cambia a otra mesa, fecha o servicio.
    // Devuelve un mensaje de error, o null si todo va bien.
    public string? ErreserbaAldatu(long erreserbaId, long mahaiBerriaId, DateTime dataBerria, string zerbitzuaBerria)
    {
        lock (_blokeoa)
        {
            using var konexioa = Ireki();

            using var galdera = konexioa.CreateCommand();
            galdera.CommandText = "SELECT COUNT(*) FROM Erreserbak WHERE MahaiId = $m AND Data = $d AND Zerbitzua = $z AND Id <> $id;";
            galdera.Parameters.AddWithValue("$m", mahaiBerriaId);
            galdera.Parameters.AddWithValue("$d", dataBerria.ToString("yyyy-MM-dd"));
            galdera.Parameters.AddWithValue("$z", zerbitzuaBerria);
            galdera.Parameters.AddWithValue("$id", erreserbaId);
            long kopurua = Convert.ToInt64(galdera.ExecuteScalar());
            if (kopurua > 0)
            {
                return "Mahai hori jada erreserbatuta dago.";
            }

            using var komandoa = konexioa.CreateCommand();
            komandoa.CommandText = "UPDATE Erreserbak SET MahaiId = $m, Data = $d, Zerbitzua = $z WHERE Id = $id;";
            komandoa.Parameters.AddWithValue("$m", mahaiBerriaId);
            komandoa.Parameters.AddWithValue("$d", dataBerria.ToString("yyyy-MM-dd"));
            komandoa.Parameters.AddWithValue("$z", zerbitzuaBerria);
            komandoa.Parameters.AddWithValue("$id", erreserbaId);
            komandoa.ExecuteNonQuery();
            return null;
        }
    }

    // Texto para la lista "Egindako erreserbak"
    public List<string> ErreserbenZerrenda(DateTime data, string zerbitzua)
    {
        List<string> zerrenda = new List<string>();
        lock (_blokeoa)
        {
            using var konexioa = Ireki();
            using var komandoa = konexioa.CreateCommand();
            komandoa.CommandText = """
                SELECT m.Zenbakia, u.Izena
                FROM Erreserbak r
                JOIN Mahaiak m ON m.Id = r.MahaiId
                JOIN Erabiltzaileak u ON u.Id = r.ErabiltzaileId
                WHERE r.Data = $d AND r.Zerbitzua = $z
                ORDER BY m.Zenbakia;
                """;
            komandoa.Parameters.AddWithValue("$d", data.ToString("yyyy-MM-dd"));
            komandoa.Parameters.AddWithValue("$z", zerbitzua);

            using var irakurlea = komandoa.ExecuteReader();
            while (irakurlea.Read())
            {
                zerrenda.Add(irakurlea.GetInt32(0) + ". mahaia - " + irakurlea.GetString(1));
            }
        }
        return zerrenda;
    }

    // ---------- Produktuak ----------

    public List<Produktua> Produktuak()
    {
        List<Produktua> zerrenda = new List<Produktua>();
        lock (_blokeoa)
        {
            using var konexioa = Ireki();
            using var komandoa = konexioa.CreateCommand();
            komandoa.CommandText = "SELECT Id, Izena, Prezioa, Stocka, Kategoria, Irudia FROM Produktuak ORDER BY Id;";

            using var irakurlea = komandoa.ExecuteReader();
            while (irakurlea.Read())
            {
                Produktua p = new Produktua();
                p.Id = irakurlea.GetInt64(0);
                p.Izena = irakurlea.GetString(1);
                p.Prezioa = Convert.ToDecimal(irakurlea.GetDouble(2));
                p.Stocka = irakurlea.GetInt32(3);
                p.Kategoria = irakurlea.GetString(4);

                if (!irakurlea.IsDBNull(5))
                {
                    p.Irudia = (byte[])irakurlea.GetValue(5);
                }

                zerrenda.Add(p);
            }
        }
        return zerrenda;
    }

    // Crea (Id = 0) o modifica un producto. Devuelve un mensaje de error, o null si todo va bien.
    public string? ProduktuaGorde(Produktua p)
    {
        if (string.IsNullOrWhiteSpace(p.Izena))
        {
            return "Izena beharrezkoa da.";
        }
        if (p.Prezioa < 0 || p.Stocka < 0)
        {
            return "Prezioa eta stocka ezin dira negatiboak izan.";
        }

        lock (_blokeoa)
        {
            using var konexioa = Ireki();

            using var galdera = konexioa.CreateCommand();
            galdera.CommandText = "SELECT COUNT(*) FROM Produktuak WHERE Izena = $i AND Id <> $id;";
            galdera.Parameters.AddWithValue("$i", p.Izena);
            galdera.Parameters.AddWithValue("$id", p.Id);
            long kopurua = Convert.ToInt64(galdera.ExecuteScalar());
            if (kopurua > 0)
            {
                return "Produktu hori jada badago.";
            }

            using var komandoa = konexioa.CreateCommand();
            if (p.Id == 0)
            {
                komandoa.CommandText = "INSERT INTO Produktuak (Izena, Prezioa, Stocka, Kategoria, Irudia) VALUES ($i, $p, $s, $k, $irudia);";
            }
            else
            {
                komandoa.CommandText = "UPDATE Produktuak SET Izena = $i, Prezioa = $p, Stocka = $s, Kategoria = $k, Irudia = $irudia WHERE Id = $id;";
                komandoa.Parameters.AddWithValue("$id", p.Id);
            }
            komandoa.Parameters.AddWithValue("$i", p.Izena);
            komandoa.Parameters.AddWithValue("$p", Convert.ToDouble(p.Prezioa));
            komandoa.Parameters.AddWithValue("$s", p.Stocka);
            komandoa.Parameters.AddWithValue("$k", p.Kategoria);

            if (p.Irudia == null)
            {
                komandoa.Parameters.AddWithValue("$irudia", DBNull.Value);
            }
            else
            {
                komandoa.Parameters.AddWithValue("$irudia", p.Irudia);
            }

            komandoa.ExecuteNonQuery();
            return null;
        }
    }

    public void ProduktuaEzabatu(long id)
    {
        lock (_blokeoa)
        {
            using var konexioa = Ireki();
            using var komandoa = konexioa.CreateCommand();
            komandoa.CommandText = "DELETE FROM Produktuak WHERE Id = $id;";
            komandoa.Parameters.AddWithValue("$id", id);
            komandoa.ExecuteNonQuery();
        }
    }

    // Confirma la compra: comprueba que hay stock para todo y lo descuenta.
    public bool Erosi(List<TicketLerroa> lerroak, out string mezua)
    {
        lock (_blokeoa)
        {
            using var konexioa = Ireki();

            // 1. Comprobar que hay stock suficiente en todas las líneas
            foreach (TicketLerroa lerroa in lerroak)
            {
                using var galdera = konexioa.CreateCommand();
                galdera.CommandText = "SELECT Stocka FROM Produktuak WHERE Id = $id;";
                galdera.Parameters.AddWithValue("$id", lerroa.ProduktuId);
                object? emaitza = galdera.ExecuteScalar();

                if (emaitza == null || Convert.ToInt32(emaitza) < lerroa.Kantitatea)
                {
                    mezua = "Ez dago stock nahikorik: " + lerroa.Izena;
                    return false;
                }
            }

            // 2. Descontar el stock
            foreach (TicketLerroa lerroa in lerroak)
            {
                using var eguneraketa = konexioa.CreateCommand();
                eguneraketa.CommandText = "UPDATE Produktuak SET Stocka = Stocka - $k WHERE Id = $id;";
                eguneraketa.Parameters.AddWithValue("$k", lerroa.Kantitatea);
                eguneraketa.Parameters.AddWithValue("$id", lerroa.ProduktuId);
                eguneraketa.ExecuteNonQuery();
            }

            mezua = "";
            return true;
        }
    }
}