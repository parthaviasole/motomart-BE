using motomart_BE.Models;

namespace motomart_BE.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Guid userId, Guid addressId, string paymentType, List<CartItemDto> items);
        Task<PagedResult<Order>> GetUserOrdersAsync(Guid userId, int pageNumber, int pageSize);
        Task<PagedResult<Order>> GetAllOrdersAsync(int pageNumber, int pageSize);
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
