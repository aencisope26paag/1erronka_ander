namespace TpvPartekatua;

public class Erabiltzailea
{
    public long Id { get; set; }
    public string Izena { get; set; } = "";
    public string Rola { get; set; } = "Erabiltzailea"; // Admin edo Erabiltzailea
}

public class Produktua
{
    public long Id { get; set; }
    public string Izena { get; set; } = "";
    public decimal Prezioa { get; set; }
    public int Stocka { get; set; }
    public string Kategoria { get; set; } = "";
}

public class Mahaia
{
    public long Id { get; set; }
    public int Zenbakia { get; set; }
    public int Eserlekuak { get; set; }
    public string Forma { get; set; } = "Karratua"; // Biribila, Karratua edo Luzea
}

public class Erreserba
{
    public long Id { get; set; }
    public long MahaiId { get; set; }
    public long ErabiltzaileId { get; set; }
    public DateTime Data { get; set; }
    public string Zerbitzua { get; set; } = "Bazkaria"; // Bazkaria edo Afaria
}