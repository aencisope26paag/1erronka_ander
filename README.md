# Elkarte gastronomikoaren TPV sistema

Elkarte gastronomiko baten kudeaketarako TPV (salmenta-puntuko terminala) sistema, **bezero-zerbitzari** arkitekturarekin. C# eta .NET 10 erabiliz garatua: WPF bezeroa, TCP zerbitzaria eta SQLite datu-basea.

- **Egilea:** Ander Enciso
- **Proiektua:** 1. Ebaluaketako erronka

## Zer egiten du

Sistemak bi erabiltzaile mota ditu:

| Rola | Zer egin dezake |
|---|---|
| **Administratzailea** | Erabiltzaileak sortu, aldatu eta ezabatu. Biltegiko stocka kudeatu: produktu berriak gehitu eta produktuak aldatu edo ezabatu (stocka, prezioa eta irudia barne). |
| **Erabiltzailea** | Kontsumoak aukeratu, tiketa erakutsi eta inprimatu (guztizko osoarekin). Erosketa baieztatzean, stocka datu-basean automatikoki eguneratzen da. Mahai-erreserbak egin, aldatu eta ezabatu (bakoitzak bere erreserbak soilik). |

Interfazeak: login-a, erabiltzaileen kudeaketa, stockaren kudeaketa, TPV (kontsumoak eta tiketa) eta mahai-erreserbak.

Mahaiak eta produktuak aukeratzeko **ikus-osagai propioak** erabiltzen dira: `MahaiControl` eta `ProduktuControl`.

## Arkitektura

```
 +----------------+      TCP (13000 portua)      +------------------+       +-----------+
 |  TpvBezeroa    | ---------------------------> |  TpvZerbitzaria  | ----> |  tpv.db   |
 |  (WPF, N bat)  | <--------------------------- |  (hari bat       | <---- |  (SQLite) |
 +----------------+       JSON mezuak            |   bezero bakoitzeko)     +-----------+
                                                 +------------------+
```

- **Zerbitzaria** da datu-basearekin hitz egiten duen bakarra. Bezero bakoitzari hari (`Thread`) independente bat esleitzen dio, eta **gehienez 10 bezero** onartzen ditu aldi berean.
- DBko sarbide guztiak **`lock` bakar baten** bidez babestuta daude, bi erabiltzailek aldi berean datuak aldatzean arazorik izan ez dadin.
- **Bezeroak** ez du inoiz datu-basea ukitzen: eskaerak (JSON lerro bat) bidaltzen dizkio zerbitzariari TCP bidez, eta erantzuna jasotzen du.

## Beharrezkoak

- Windows
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio (".NET desktop development" lan-zama gaituta) — gomendatua, baina ez derrigorrezkoa

## Aplikazioa martxan jarri

### 1. Biltegia jaitsi

```bash
git clone https://github.com/aencisope26paag/1erronka_ander.git
cd 1erronka_ander
```

### 2. Visual Studio-rekin

1. Ireki `1.Ebaluaketako erronka - Ander Enciso.slnx` fitxategia. NuGet paketeak automatikoki berrezartzen dira.
2. **Zerbitzaria abiarazi** (lehenengo): `TpvZerbitzaria` proiektuan eskuineko botoia → *Debug* → *Start New Instance*.
   Kontsola bat irekiko da, eta honako mezuak ikusi behar dira:
   ```
   Datu-basea prest: ...\tpv.db
   Zerbitzaria entzuten: 13000 portuan.
   ```
   Windows-ek suebakiaren baimena eska dezake: sakatu *Baimendu sarbidea*.
3. **Bezeroa abiarazi**: `TpvBezeroa` proiektuan eskuineko botoia → *Debug* → *Start New Instance*.
4. Sartu erabiltzaile baten datuekin (ikusi [Erabiltzaileak eta pasahitzak](#erabiltzaileak-eta-pasahitzak)).

> Zerbitzaria bezeroa baino **lehenago** abiarazi behar da. Bestela, login-ak hau erakutsiko du: *"Ezin da zerbitzarira konektatu. Zerbitzaria martxan dago?"*

### 3. Edo komando-lerrotik

Terminal batean (zerbitzaria):

```bash
dotnet run --project TpvZerbitzaria
```

Beste terminal batean (bezeroa):

```bash
dotnet run --project TpvBezeroa
```

### Hainbat bezero aldi berean

Ireki bezeroa hainbat aldiz: Visual Studio-n *Start New Instance* behin baino gehiagotan `TpvBezeroa` proiektuarekin, edo terminal gehiago `dotnet run --project TpvBezeroa` komandoarekin.

Gehienez **10 bezero** aldi berean. 11.ak mezu hau jasoko du: *"Zerbitzaria beteta dago (gehienez 10 bezero)."*

### Beste ordenagailu batetik konektatu

Zerbitzariaren helbidea eta portua `TpvPartekatua/Protokoloa.cs` fitxategian daude (`Helbidea = "127.0.0.1"`, `Portua = 13000`). Beste ordenagailu batetik konektatzeko:

1. Aldatu `Helbidea` zerbitzariaren IP helbidera.
2. Konpilatu bezeroa berriro.
3. Zerbitzariaren ordenagailuan, baimendu 13000 portua suebakian.

## Erabiltzaileak eta pasahitzak

| Erabiltzailea | Pasahitza | Rola |
|---|---|---|
| `admin` | `admin123` | Administratzailea |
| `ander` | `ander123` | Erabiltzailea |

`admin` eta `ander` hasierako datuekin sortzen dira (datu-basea lehen aldiz sortzen denean).

Gainera, administratzaileak erabiltzaile gehiago sor ditzake aplikaziotik.

## Datu-basea

- SQLite datu-basea, `tpv.db` fitxategian. Zerbitzariaren irteera-karpetan sortzen da (`TpvZerbitzaria/bin/Debug/net10.0/tpv.db`).
- Zerbitzaria lehen aldiz abiaraztean, fitxategia automatikoki sortzen da, taulekin eta hasierako datuekin (administratzailea, erabiltzaile bat, 4 produktu eta 6 mahai).
- Taulak: `Erabiltzaileak`, `Produktuak` (produktuaren irudia barne, `BLOB` gisa), `Mahaiak` eta `Erreserbak`.

### Backupak eta berreskuratzea

`Backups/` karpetan daude:

- `tpv_backup.db`: datu-baseen backupa (erabiltzaileak, produktuak irudiekin eta erreserbak).

Backupa berreskuratzeko:

1. Itxi zerbitzaria.
2. Ezabatu `TpvZerbitzaria/bin/Debug/net10.0/tpv.db` (existitzen bada).
3. Kopiatu `Backups/tpv_backup.db` karpeta horretara, `tpv.db` izenarekin.
4. Abiarazi zerbitzaria berriro.

Datu-basea hutsetik hasteko, ezabatu `tpv.db` eta abiarazi zerbitzaria: berriro sortuko da hasierako datuekin.

## Proiektuaren egitura

```
1erronka_ander/
├── 1.Ebaluaketako erronka - Ander Enciso.slnx
├── README.md
├── Backups/               Datu-baseen backupak
├── TpvPartekatua/         Liburutegia: ereduak eta mezuen protokoloa (bezeroak eta zerbitzariak erabiltzen dute)
├── TpvZerbitzaria/        Kontsola-aplikazioa: TCP zerbitzaria eta SQLite sarbidea (DatuBasea)
└── TpvBezeroa/            WPF aplikazioa: leihoak eta ikus-osagaiak
```

## Dokumentazioa

- Gida teknikoa: `docs/` karpetan
- Administratzailearen eta erabiltzailearen gida: `docs/` karpetan

## Ezagutzen diren mugak

Helburu didaktikoko proiektua da, eta sinplifikazio batzuk egin dira:

- Pasahitzak datu-basean testu lauan gordetzen dira (hash-ik gabe).
- TCP komunikazioa ez dago zifratuta.
- Erreserbak bakoitzak bereak soilik alda ditzan egiaztapena bezeroaren leihoan egiten da, ez zerbitzarian.
