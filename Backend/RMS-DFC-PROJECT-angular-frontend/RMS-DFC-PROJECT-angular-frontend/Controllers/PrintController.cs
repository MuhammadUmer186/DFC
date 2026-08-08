using Microsoft.AspNetCore.Mvc;
using Printing.Services;
using RestaurantSystem.DTOs;

namespace Printing.Controllers
{
    [ApiController]
    [Route("api/print")]
    public class PrintController : ControllerBase
    {
        private readonly PrintService _printService;

        public PrintController(PrintService printService)
        {
            _printService = printService;
        }

        [HttpPost("receipt")]
        public IActionResult PrintReceipt([FromBody] PrintReceiptDto dto, [FromQuery] string printer)
        {
            if (dto == null || dto.Items.Count == 0)
                return BadRequest("No items to print.");

            PrinterType type = printer.ToLower() switch
            {
                "usb1" => PrinterType.Usb1,
                "usb2" => PrinterType.Usb2,
                "ethernet" => PrinterType.Ethernet,
                "bluetooth" => PrinterType.Bluetooth,
                _ => throw new ArgumentException("Invalid printer type")
            };

            bool result = _printService.PrintReceipt(dto, type);

            if (result)
                return Ok(new { message = "Receipt sent to printer successfully." });
            else
                return StatusCode(500, new { message = "Failed to print receipt." });
        }
    }
}
