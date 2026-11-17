using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using TpvPartekatua;

namespace TpvBezeroa;

// Todos los datos se piden al servidor por TCP.
// Las ventanas siguen llamando a estos mismos métodos de siempre.
public static class Datuak
{
    // ---------- Conexión con el servidor ----------

    private static TcpClient? _bezeroa;
    private static StreamReader? _irakurlea;
    private static StreamWriter? _idazlea;

    // Se conecta la primera vez que hace falta; después reutiliza la conexión.
    private static void Konektatu()
    {
        if (_bezeroa != null) return;

        try
        {
            TcpClient bezeroa = new TcpClient(Protokoloa.Helbidea, Protokoloa.Portua);
            NetworkStream stream = bezeroa.GetStream();

            _irakurlea = new StreamReader(stream);
            _idazlea = new StreamWriter(stream);
            _idazlea.AutoFlush = true;
            _bezeroa = bezeroa;
        }
        catch (SocketException)
        {
            throw new Exception("Ezin da zerbitzarira konektatu. Zerbitzaria martxan dago?");
        }
    }

    public static void Deskonektatu()
    {
        if (_bezeroa != null)
        {
            _bezeroa.Close();
        }
        _bezeroa = null;
        _irakurlea = null;
        _idazlea = null;
    }

    // Manda una petición (una línea JSON) y espera la respuesta (otra línea JSON).
    private static Erantzuna Eskatu(Eskaera eskaera)
    {
        Konektatu();

        StreamWriter? idazlea = _idazlea;
        StreamReader? irakurlea = _irakurlea;
        if (idazlea == null || irakurlea == null)
        {
            throw new Exception("Ez dago konexiorik zerbitzariarekin.");
        }

        try
        {
            idazlea.WriteLine(JsonSerializer.Serialize(eskaera));
            string? lerroa = irakurlea.ReadLine();

            if (lerroa == null)
            {
                Deskonektatu();
                throw new Exception("Zerbitzariak konexioa itxi du.");
            }

            Erantzuna? erantzuna = JsonSerializer.Deserialize<Erantzuna>(lerroa);
            if (erantzuna == null)
            {
                throw new Exception("Zerbitzariaren erantzuna baliogabea da.");
            }

            if (erantzuna.Akatsa != null)
            {
                Deskonektatu(); // la próxima llamada volverá a conectar
                throw new Exception(erantzuna.Akatsa);
            }

            return erantzuna;
        }
        catch (IOException)
        {
            Deskonektatu();
            throw new Exception("Konexioa galdu da zerbitzariarekin.");
        }
    }

    // ---------- Erabiltzaileak ----------

    public static Erabiltzailea? Login(string izena, string pasahitza)
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "Login";
        eskaera.Izena = izena;
        eskaera.Pasahitza = pasahitza;
        return Eskatu(eskaera).Erabiltzailea;
    }

    public static List<Erabiltzailea> Erabiltzaileak()
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "Erabiltzaileak";
        return Eskatu(eskaera).Erabiltzaileak;
    }

    // Crea (Id = 0) o modifica un usuario. Devuelve un mensaje de error, o null si todo va bien.
    public static string? ErabiltzaileaGorde(Erabiltzailea erabiltzailea)
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "ErabiltzaileaGorde";
        eskaera.Erabiltzailea = erabiltzailea;
        return Eskatu(eskaera).Mezua;
    }

    // Borra un usuario y sus reservas. No se puede borrar a uno mismo.
    public static string? ErabiltzaileaEzabatu(long id, long unekoaId)
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "ErabiltzaileaEzabatu";
        eskaera.Id = id;
        eskaera.UnekoaId = unekoaId;
        return Eskatu(eskaera).Mezua;
    }

    // ---------- Mahaiak eta erreserbak ----------

    public static List<Mahaia> Mahaiak()
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "Mahaiak";
        return Eskatu(eskaera).Mahaiak;
    }

    public static Erreserba? ErreserbaLortu(long mahaiId, DateTime data, string zerbitzua)
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "ErreserbaLortu";
        eskaera.MahaiId = mahaiId;
        eskaera.Data = data;
        eskaera.Zerbitzua = zerbitzua;
        return Eskatu(eskaera).Erreserba;
    }

    public static bool ErreserbaEgin(long mahaiId, long erabiltzaileId, DateTime data, string zerbitzua)
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "ErreserbaEgin";
        eskaera.MahaiId = mahaiId;
        eskaera.ErabiltzaileId = erabiltzaileId;
        eskaera.Data = data;
        eskaera.Zerbitzua = zerbitzua;
        return Eskatu(eskaera).Emaitza;
    }

    public static void ErreserbaEzabatu(long mahaiId, DateTime data, string zerbitzua)
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "ErreserbaEzabatu";
        eskaera.MahaiId = mahaiId;
        eskaera.Data = data;
        eskaera.Zerbitzua = zerbitzua;
        Eskatu(eskaera);
    }

    // Cambia una reserva a otra mesa, fecha o servicio. (La usaremos en el paso 4.)
    // Devuelve un mensaje de error, o null si todo va bien.
    public static string? ErreserbaAldatu(long erreserbaId, long mahaiBerriaId, DateTime dataBerria, string zerbitzuaBerria)
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "ErreserbaAldatu";
        eskaera.Id = erreserbaId;
        eskaera.MahaiId = mahaiBerriaId;
        eskaera.Data = dataBerria;
        eskaera.Zerbitzua = zerbitzuaBerria;
        return Eskatu(eskaera).Mezua;
    }

    // Texto para la lista "Egindako erreserbak"
    public static List<string> ErreserbenZerrenda(DateTime data, string zerbitzua)
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "ErreserbenZerrenda";
        eskaera.Data = data;
        eskaera.Zerbitzua = zerbitzua;
        return Eskatu(eskaera).Zerrenda;
    }

    // ---------- Produktuak ----------

    public static List<Produktua> Produktuak()
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "Produktuak";
        return Eskatu(eskaera).Produktuak;
    }

    // Crea (Id = 0) o modifica un producto. Devuelve un mensaje de error, o null si todo va bien.
    public static string? ProduktuaGorde(Produktua produktua)
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "ProduktuaGorde";
        eskaera.Produktua = produktua;
        return Eskatu(eskaera).Mezua;
    }

    public static void ProduktuaEzabatu(long id)
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "ProduktuaEzabatu";
        eskaera.Id = id;
        Eskatu(eskaera);
    }

    // Confirma la compra: el servidor comprueba el stock y lo descuenta.
    public static bool Erosi(List<TicketLerroa> lerroak, out string mezua)
    {
        Eskaera eskaera = new Eskaera();
        eskaera.Komandoa = "Erosi";
        eskaera.Lerroak = lerroak;

        Erantzuna erantzuna = Eskatu(eskaera);
        if (erantzuna.Mezua == null)
        {
            mezua = "";
        }
        else
        {
            mezua = erantzuna.Mezua;
        }
        return erantzuna.Emaitza;
    }
}