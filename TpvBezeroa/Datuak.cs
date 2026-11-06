using TpvPartekatua;

namespace TpvBezeroa;

// DATOS DE PRUEBA EN MEMORIA.
// Más adelante, el interior de estos métodos pedirá los datos al servidor.
public static class Datuak
{
    private static readonly List<(long Id, string Izena, string Pasahitza, string Rola)> _erabiltzaileak = new()
    {
        (1, "admin", "admin123", "Admin"),
        (2, "ane", "ane123", "Erabiltzailea")
    };

    private static readonly List<Mahaia> _mahaiak = new()
    {
        new() { Id = 1, Zenbakia = 1, Eserlekuak = 2, Forma = "Biribila" },
        new() { Id = 2, Zenbakia = 2, Eserlekuak = 4, Forma = "Karratua" },
        new() { Id = 3, Zenbakia = 3, Eserlekuak = 6, Forma = "Luzea" },
        new() { Id = 4, Zenbakia = 4, Eserlekuak = 2, Forma = "Karratua" },
        new() { Id = 5, Zenbakia = 5, Eserlekuak = 4, Forma = "Biribila" },
        new() { Id = 6, Zenbakia = 6, Eserlekuak = 8, Forma = "Luzea" }
    };

    private static readonly List<Erreserba> _erreserbak = new();
    private static long _hurrengoErreserbaId = 1;

    // ---------- Login ----------

    public static Erabiltzailea? Login(string izena, string pasahitza)
    {
        foreach (var u in _erabiltzaileak)
        {
            if (u.Izena == izena && u.Pasahitza == pasahitza)
                return new Erabiltzailea { Id = u.Id, Izena = u.Izena, Rola = u.Rola };
        }
        return null;
    }

    // ---------- Mahaiak eta erreserbak ----------

    public static List<Mahaia> Mahaiak() => _mahaiak.ToList();

    public static Erreserba? ErreserbaLortu(long mahaiId, DateTime data, string zerbitzua)
        => _erreserbak.FirstOrDefault(r => r.MahaiId == mahaiId
                                        && r.Data.Date == data.Date
                                        && r.Zerbitzua == zerbitzua);

    public static bool ErreserbaEgin(long mahaiId, long erabiltzaileId, DateTime data, string zerbitzua)
    {
        if (ErreserbaLortu(mahaiId, data, zerbitzua) != null) return false; // jada okupatuta

        _erreserbak.Add(new Erreserba
        {
            Id = _hurrengoErreserbaId++,
            MahaiId = mahaiId,
            ErabiltzaileId = erabiltzaileId,
            Data = data.Date,
            Zerbitzua = zerbitzua
        });
        return true;
    }

    public static void ErreserbaEzabatu(long mahaiId, DateTime data, string zerbitzua)
    {
        var erreserba = ErreserbaLortu(mahaiId, data, zerbitzua);
        if (erreserba != null) _erreserbak.Remove(erreserba);
    }

    // Testu-zerrenda "Egindako erreserbak" koadrorako
    public static List<string> ErreserbenZerrenda(DateTime data, string zerbitzua)
    {
        var zerrenda = new List<string>();
        foreach (var r in _erreserbak.Where(x => x.Data.Date == data.Date && x.Zerbitzua == zerbitzua))
        {
            var mahaia = _mahaiak.First(m => m.Id == r.MahaiId);
            var erabiltzailea = _erabiltzaileak.First(u => u.Id == r.ErabiltzaileId);
            zerrenda.Add($"{mahaia.Zenbakia}. mahaia - {erabiltzailea.Izena}");
        }
        return zerrenda;
    }
}