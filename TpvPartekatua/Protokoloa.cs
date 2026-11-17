namespace TpvPartekatua;

public static class Protokoloa
{
    public const string Helbidea = "127.0.0.1";
    public const int Portua = 13000;
}

// Cliente -> servidor: el comando y los datos que necesite
public class Eskaera
{
    public string Komandoa { get; set; } = "";

    public string Izena { get; set; } = "";
    public string Pasahitza { get; set; } = "";

    public long Id { get; set; }
    public long UnekoaId { get; set; }
    public long MahaiId { get; set; }
    public long ErabiltzaileId { get; set; }
    public DateTime Data { get; set; }
    public string Zerbitzua { get; set; } = "";

    public Erabiltzailea Erabiltzailea { get; set; } = new Erabiltzailea();
    public Produktua Produktua { get; set; } = new Produktua();
    public List<TicketLerroa> Lerroak { get; set; } = new List<TicketLerroa>();
}

// Servidor -> cliente: el resultado
public class Erantzuna
{
    public string? Akatsa { get; set; }      // error del servidor (comando desconocido, servidor lleno...)
    public string? Mezua { get; set; }       // mensaje de la operación (null = todo bien)
    public bool Emaitza { get; set; }        // resultado sí/no (reservar, comprar...)

    public Erabiltzailea? Erabiltzailea { get; set; }
    public Erreserba? Erreserba { get; set; }
    public List<Erabiltzailea> Erabiltzaileak { get; set; } = new List<Erabiltzailea>();
    public List<Mahaia> Mahaiak { get; set; } = new List<Mahaia>();
    public List<Produktua> Produktuak { get; set; } = new List<Produktua>();
    public List<string> Zerrenda { get; set; } = new List<string>();
}