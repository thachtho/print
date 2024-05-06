using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using printer_2.Services;
using DevExpress.Pdf.Native.BouncyCastle.Asn1.Ocsp;

namespace printer_2.Controllers
{
    public class TrangThaiDto 
    {
        public bool status { get; set; }
    }

    [ApiController]
    [Route("api/vantay")]
    public class VantayController : Controller
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        private readonly IVantayService _vantayService;

        public VantayController(IVantayService vantayService)
        {
            _vantayService = vantayService;
        }

        [HttpGet("dangky")]
        public IActionResult Dangky()
        {
            try
            {
                Console.WriteLine("Đăng ký thành công!");
                _vantayService.SetTrangThai(true);
                return Ok("Đăng ký thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("set-trang-thai")]
        public IActionResult Huy([FromBody] TrangThaiDto request)
        {
            try
            {
                Console.WriteLine(11111);
                Console.WriteLine(request.status);
                _vantayService.SetTrangThai(request.status);
  
                return Ok("Thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpGet("get-trang-thai-usb")]
        public IActionResult getTrangThaiUsb()
        {
            try
            {
                return Ok("Lay danh sach thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpGet("remove-sensor")]
        public IActionResult RemoveSensor()
        {
            try
            {
                _vantayService.removeDataSensor();
                return Ok("Remove Thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}
