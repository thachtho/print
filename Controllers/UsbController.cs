using Microsoft.AspNetCore.Mvc;
using printer_2.Services;

namespace printer_2.Controllers
{
    [ApiController]
    [Route("api/usbvantay")]
    public class UsbController: Controller
    {
        private readonly IUsbService _usbService;

        public UsbController(IUsbService usbService)
        {
            _usbService = usbService;
        }


        [HttpGet("status")]
        public IActionResult getTrangThaiUsb()
        {
            try
            {
                var status = _usbService.getTrangThaiUsb();
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}
