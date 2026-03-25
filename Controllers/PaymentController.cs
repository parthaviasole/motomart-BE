using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using motomart_BE.Models;
using motomart_BE.Services;
using System.Security.Claims;
using motomart_BE.Data;
using Microsoft.EntityFrameworkCore;

namespace motomart_BE.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService, AppDbContext context, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("create-order")]
        public IActionResult CreateRazorpayOrder([FromBody] PaymentRequest request)
        {
            try
            {
                var keyId = _configuration["Razorpay:KeyId"];
                if (string.IsNullOrEmpty(keyId) || keyId == "rzp_test_placeholder")
                {
                    return BadRequest(new { message = "Razorpay API keys are not configured. Please update appsettings.json with valid credentials." });
                }

                var razorpayOrderId = _paymentService.CreateOrder(request.Amount, request.ReceiptId);
                return Ok(new 
                { 
                    orderId = razorpayOrderId,
                    amount = (int)(request.Amount * 100),
                    currency = "INR",
                    keyId = keyId
                });
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (message.Contains("authentication failed", StringComparison.OrdinalIgnoreCase))
                {
                    message = "Razorpay authentication failed. Please check your Key ID and Key Secret in appsettings.json.";
                }
                return BadRequest(new { message = message });
            }
        }

        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerificationRequest request)
        {
            bool isValid = _paymentService.VerifyPayment(request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);

            if (isValid)
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.RazorpayOrderId == request.RazorpayOrderId);
                if (order != null)
                {
                    order.RazorpayPaymentId = request.RazorpayPaymentId;
                    order.RazorpaySignature = request.RazorpaySignature;
                    order.PaymentStatus = "Paid";
                    order.Status = "Confirmed";
                    await _context.SaveChangesAsync();
                }
                return Ok(new { status = "success" });
            }

            return BadRequest(new { status = "failed", message = "Payment verification failed" });
        }
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string ReceiptId { get; set; } = string.Empty;
    }

    public class PaymentVerificationRequest
    {
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
    }
}
