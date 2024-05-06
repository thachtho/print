using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;
using Newtonsoft.Json;
using DevExpress.Pdf;
using printer_2.Services;

public struct PrinterOptions
{
    public string[] Collate;
    public string[] Duplexing;
    public short MaxCopy;
    public bool SupportsColor;
    public string[] PaperSheets;
    public string[] Resolutions;
}


public class PrinterInfo
{
    public string PrinterName { get; set; }
    public List<PaperSizeInfo> PaperSizes { get; set; }

    public DefaultPageSettings defaultPageSettings { get; set; }
}

public class PaperSizeInfo
{
    public string paperName { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}

public class PrinterRequest
{
    public string PrinterName { get; set; }
}

public struct Settings
{
    //PageSettings
    public bool Landscape { get; set; }
    public string printer { get; set; }

    public string width { get; set; }

    public string height { get; set; }

    public byte[] buffer { get; set; }
}

public class DefaultPageSettings
{
    public int CanDuplex { get; set; }

    //PageSettings
    public int Landscape { get; set; }

    public PaperSizeDefault paperSize { get; set; }
}

public class PaperSizeDefault
{
    public int width { get; set; }
    public int height { get; set; }
    public string PaperName { get; set; }
}


namespace printer_2.Controllers
{
    [ApiController]
    [Route("api/printer")]
    public class PrinterController : ControllerBase
    {
        private readonly IPrinterService _printerService;

        public PrinterController(IPrinterService printerService)
        {
            _printerService = printerService;
        }

        [HttpGet("list")]
        public IActionResult GetPrinters()
        {
            try
            {
                string[] printerNames = _printerService.GetPrinters();
                return Ok(printerNames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("info")]
        public IActionResult GetPrinterInfo([FromBody] PrinterRequest request)
        {
            try
            {
                string printerName = request.PrinterName;
                PrinterSettings printerSettings = new PrinterSettings
                {
                    PrinterName = printerName
                };

                if (!PrinterSettings.InstalledPrinters.Cast<string>().Any(p => p.Equals(printerName, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound($"Máy in '{printerName}' không được tìm thấy.");
                }

                PrinterInfo printerInfo = new PrinterInfo
                {
                    PrinterName = printerName,
                    defaultPageSettings = getDefaultSetting(printerSettings),
                    PaperSizes = GetPaperSizes(printerSettings),
                };

                return Ok(printerInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            } 
        }

        [HttpPost]
        [Route("print")]
        public IActionResult handlePrint([FromBody] Settings request)
        {
           return PrintPDF(request);
        }

        public IActionResult PrintPDF(Settings printerSettings)
        {

            try
            {
                var printer = printerSettings.printer;
                var width = printerSettings.width;
                var height = printerSettings.height;
                var landscape = PdfPrintPageOrientation.Portrait;
                byte[] buffer = printerSettings.buffer;

                MemoryStream stream = new MemoryStream(buffer);

                if (printerSettings.Landscape)
                {
                    landscape = PdfPrintPageOrientation.Landscape;
                }

                PdfDocumentProcessor documentProcessor = new PdfDocumentProcessor();
                documentProcessor.LoadDocument(stream);
                PdfPrinterSettings pdfPrinterSettings = new PdfPrinterSettings();
                pdfPrinterSettings.PageOrientation = landscape;

                pdfPrinterSettings.Settings.DefaultPageSettings.PaperSize = new PaperSize("Custom", Convert.ToInt32(width), Convert.ToInt32(height));
                pdfPrinterSettings.Settings.Duplex = Duplex.Simplex;
                pdfPrinterSettings.Settings.PrinterName = printer;
                PrinterSettings settings = pdfPrinterSettings.Settings;
                documentProcessor.Print(pdfPrinterSettings);

                return Ok(true);
            }
            catch (Exception e)
            {
                PrintJson(e);
                return StatusCode(500, $"Internal Server Error: {e.Message}");
            }
        }

        private DefaultPageSettings getDefaultSetting(PrinterSettings printerSettings) {
            var setting = printerSettings.DefaultPageSettings;

            PaperSizeDefault paperSizeDefault = new PaperSizeDefault()
            {
                width = setting.PaperSize.Width,
                height = setting.PaperSize.Height,
                PaperName = setting.PaperSize.PaperName,
            };
            DefaultPageSettings sizeInfo = new DefaultPageSettings
            {
                CanDuplex = printerSettings.CanDuplex ? 1 : 0,
                Landscape = setting.Landscape ? 1 : 0,
                paperSize = paperSizeDefault
            };

            return sizeInfo;
        }

        private List<PaperSizeInfo> GetPaperSizes(PrinterSettings printerSettings)
        {
            List<PaperSizeInfo> paperSizes = new List<PaperSizeInfo>();

            foreach (PaperSize paperSize in printerSettings.PaperSizes)
            {
                PaperSizeInfo sizeInfo = new PaperSizeInfo
                {
                    paperName = paperSize.PaperName,
                    Width = paperSize.Width,
                    Height = paperSize.Height
                };

                paperSizes.Add(sizeInfo);
            }

            return paperSizes;
        }

        public static bool PrintJson(object printerOptions)
        {
            Console.Write(JsonConvert.SerializeObject(printerOptions, Formatting.None,
                           new JsonSerializerSettings()
                           {
                               ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                           }));
            return true;
        }
    }
}
