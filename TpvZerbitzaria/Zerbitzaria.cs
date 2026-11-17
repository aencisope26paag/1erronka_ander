using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TpvPartekatua;

namespace TpvZerbitzaria;

public class Zerbitzaria
{
    private const int GehienezkoBezeroak = 10;

    private DatuBasea _db;
    private int _bezeroKopurua = 0;

    public Zerbitzaria(DatuBasea db)
    {
        _db = db;
    }

    // Se queda escuchando; cada cliente que llega recibe su propio hilo.
    public void Hasi()
    {
        TcpListener entzulea = new TcpListener(IPAddress.Any, Protokoloa.Portua);
        entzulea.Start();
        Console.WriteLine("Zerbitzaria entzuten: " + Protokoloa.Portua + " portuan.");

        while (true)
        {
            TcpClient bezeroa = entzulea.AcceptTcpClient();

            Thread haria = new Thread(BezeroaKudeatu);
            haria.IsBackground = true;
            haria.Start(bezeroa);
        }
    }

    // Lo que hace el hilo de cada cliente: lee una línea, contesta una línea, y repite.
    private void BezeroaKudeatu(object? parametroa)
    {
        TcpClient? bezeroa = parametroa as TcpClient;
        if (bezeroa == null) return;

        int kopurua = Interlocked.Increment(ref _bezeroKopurua);
        bool beteta = kopurua > GehienezkoBezeroak;
        Console.WriteLine("Bezero bat konektatu da. Aktiboak: " + kopurua);

        try
        {
            using (bezeroa)
            {
                NetworkStream stream = bezeroa.GetStream();
                StreamReader irakurlea = new StreamReader(stream);
                StreamWriter idazlea = new StreamWriter(stream);
                idazlea.AutoFlush = true;

                string? lerroa = irakurlea.ReadLine();
                while (lerroa != null)
                {
                    Erantzuna erantzuna;
                    if (beteta)
                    {
                        erantzuna = new Erantzuna();
                        erantzuna.Akatsa = "Zerbitzaria beteta dago (gehienez 10 bezero).";
                    }
                    else
                    {
                        erantzuna = Prozesatu(lerroa);
                    }

                    idazlea.WriteLine(JsonSerializer.Serialize(erantzuna));

                    if (beteta) break; // al cliente de más se le avisa y se le desconecta

                    lerroa = irakurlea.ReadLine();
                }
            }
        }
        catch (IOException)
        {
            // El cliente se ha desconectado de golpe: no pasa nada.
        }
        finally
        {
            int geratzen = Interlocked.Decrement(ref _bezeroKopurua);
            Console.WriteLine("Bezero bat deskonektatu da. Aktiboak: " + geratzen);
        }
    }

    // Convierte la línea JSON en una petición y devuelve la respuesta.
    // Si algo falla, el cliente recibe un error y el hilo no se cae.
    private Erantzuna Prozesatu(string lerroa)
    {
        try
        {
            Eskaera? eskaera = JsonSerializer.Deserialize<Eskaera>(lerroa);
            if (eskaera == null)
            {
                Erantzuna hutsa = new Erantzuna();
                hutsa.Akatsa = "Eskaera hutsik.";
                return hutsa;
            }
            return Exekutatu(eskaera);
        }
        catch (Exception ex)
        {
            Erantzuna errorea = new Erantzuna();
            errorea.Akatsa = "Zerbitzariaren errorea: " + ex.Message;
            return errorea;
        }
    }

    // Según el comando, llama al método correspondiente de DatuBasea.
    private Erantzuna Exekutatu(Eskaera e)
    {
        Erantzuna er = new Erantzuna();

        switch (e.Komandoa)
        {
            case "Login":
                er.Erabiltzailea = _db.Login(e.Izena, e.Pasahitza);
                break;

            case "Erabiltzaileak":
                er.Erabiltzaileak = _db.Erabiltzaileak();
                break;

            case "ErabiltzaileaGorde":
                er.Mezua = _db.ErabiltzaileaGorde(e.Erabiltzailea);
                break;

            case "ErabiltzaileaEzabatu":
                er.Mezua = _db.ErabiltzaileaEzabatu(e.Id, e.UnekoaId);
                break;

            case "Mahaiak":
                er.Mahaiak = _db.Mahaiak();
                break;

            case "ErreserbaLortu":
                er.Erreserba = _db.ErreserbaLortu(e.MahaiId, e.Data, e.Zerbitzua);
                break;

            case "ErreserbaEgin":
                er.Emaitza = _db.ErreserbaEgin(e.MahaiId, e.ErabiltzaileId, e.Data, e.Zerbitzua);
                break;

            case "ErreserbaEzabatu":
                _db.ErreserbaEzabatu(e.MahaiId, e.Data, e.Zerbitzua);
                break;

            case "ErreserbaAldatu":
                er.Mezua = _db.ErreserbaAldatu(e.Id, e.MahaiId, e.Data, e.Zerbitzua);
                break;

            case "ErreserbenZerrenda":
                er.Zerrenda = _db.ErreserbenZerrenda(e.Data, e.Zerbitzua);
                break;

            case "Produktuak":
                er.Produktuak = _db.Produktuak();
                break;

            case "ProduktuaGorde":
                er.Mezua = _db.ProduktuaGorde(e.Produktua);
                break;

            case "ProduktuaEzabatu":
                _db.ProduktuaEzabatu(e.Id);
                break;

            case "Erosi":
                {
                    string mezua;
                    er.Emaitza = _db.Erosi(e.Lerroak, out mezua);
                    er.Mezua = mezua;
                    break;
                }

            default:
                er.Akatsa = "Komando ezezaguna: " + e.Komandoa;
                break;
        }

        return er;
    }
}