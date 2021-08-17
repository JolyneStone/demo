using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintDemo
{
    public static class PrinterFactory
    {
        public static IEnumerable<string> GetAllPrints()
        {
            return PrinterSettings.InstalledPrinters.Cast<string>();
        }

        public static Printer GetPrinter(string printerName)
        {
            if (string.IsNullOrEmpty(printerName)) throw new ArgumentException(nameof(printerName));
            return new Printer(printerName);
        }
    }
}
