using motomart_BE.Models;

namespace motomart_BE.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Guid userId, Guid addressId, string paymentType, List<CartItemDto> items, 
            string? razorpayOrderId = null, string? razorpayPaymentId = null, string? razorpaySignature = null);
        Task<PagedResult<Order>> GetUserOrdersAsync(Guid userId, int pageNumber, int pageSize, string? searchTerm = null, DateTime? date = null, string? status = null);
        Task<PagedResult<Order>> GetAllOrdersAsync(int pageNumber, int pageSize, string? searchTerm = null, DateTime? date = null, string? status = null);
        Task<Order?> GetOrderByIdAsync(Guid orderId);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string status);
        Task<bool> VerifyOtpAsync(Guid orderId, string otp);
        Task<bool> ResendOtpAsync(Guid orderId);
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
