using motomart_BE.Models;

namespace motomart_BE.Services
{
    public interface ICartService
    {
        Task<IEnumerable<CartItem>> GetUserCartAsync(Guid userId);
        Task<CartItem> AddToCartAsync(Guid userId, int productId, int quantity);
        Task<CartItem?> UpdateCartItemAsync(Guid userId, int productId, int quantity);
        Task<bool> RemoveFromCartAsync(Guid userId, int productId);
        Task ClearCartAsync(Guid userId);
    }
}
