using System.IO;
using System.Windows.Media.Imaging;

namespace TpvBezeroa;

public static class Irudiak
{
    // Convierte los bytes de una imagen en una imagen de WPF.
    // Devuelve null si no hay imagen o no es válida.
    public static BitmapImage? Sortu(byte[]? datuak)
    {
        if (datuak == null || datuak.Length == 0)
        {
            return null;
        }

        try
        {
            BitmapImage irudia = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(datuak))
            {
                irudia.BeginInit();
                irudia.CacheOption = BitmapCacheOption.OnLoad;
                irudia.StreamSource = stream;
                irudia.EndInit();
            }
            irudia.Freeze();
            return irudia;
        }
        catch (Exception)
        {
            return null;
        }
    }
}