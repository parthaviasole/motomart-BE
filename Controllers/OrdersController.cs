using Microsoft.AspNetCore.Mvc;
using motomart_BE.Models;
using motomart_BE.Services;
using System.Security.Claims;

namespace motomart_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated"));
            try
            {
                var order = await _orderService.CreateOrderAsync(userId, dto.AddressId, dto.PaymentType, dto.Items);
                return Ok(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Order creation failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserOrders(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] DateTime? date = null,
            [FromQuery] string? status = null)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated"));
            var orders = await _orderService.GetUserOrdersAsync(userId, pageNumber, pageSize, searchTerm, date, status);
            return Ok(orders);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] DateTime? date = null,
            [FromQuery] string? status = null)
        {
            // Add Admin check if needed
            var orders = await _orderService.GetAllOrdersAsync(pageNumber, pageSize, searchTerm, date, status);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, status);
            if (!success) return BadRequest("Could not update status");
            return Ok();
        }

        [HttpPost("{id}/verify-otp")]
        public async Task<IActionResult> VerifyOtp(Guid id, [FromBody] string otp)
        {
            var success = await _orderService.VerifyOtpAsync(id, otp);
            if (!success) return BadRequest("Invalid OTP");
            return Ok();
        }

        [HttpPost("{id}/resend-otp")]
        public async Task<IActionResult> ResendOtp(Guid id)
        {
            var success = await _orderService.ResendOtpAsync(id);
            if (!success) return BadRequest("Could not resend OTP");
            return Ok();
        }
    }

    public class CreateOrderDto
    {
        public Guid AddressId { get; set; }
        public string PaymentType { get; set; } = "COD";
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}
