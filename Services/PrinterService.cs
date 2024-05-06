using System.Drawing.Printing;

namespace printer_2.Services
{
    public interface IPrinterService
    {
        string[] GetPrinters();
    }
    public class PrinterService : IPrinterService
    {
        public string[] GetPrinters()
        {
            PrinterSettings settings = new PrinterSettings();
            string[] printerNames = PrinterSettings.InstalledPrinters.Cast<string>().ToArray();

            return printerNames;
        }
    }
}
