using System.Windows;
using System.Windows.Threading;

namespace TpvBezeroa
{
    public partial class App : Application
    {
        // Cualquier error no controlado (por ejemplo, el servidor caído) se muestra como mensaje.
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Errorea", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Datuak.Deskonektatu();
        }
    }
}