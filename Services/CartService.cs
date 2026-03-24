using Microsoft.EntityFrameworkCore;
using motomart_BE.Data;
using motomart_BE.Models;

namespace motomart_BE.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItem>> GetUserCartAsync(Guid userId)
        {
            return await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<CartItem> AddToCartAsync(Guid userId, int productId, int quantity)
        {
            var existingItem = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                await _context.SaveChangesAsync();
                return existingItem;
            }

            var newItem = new CartItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = productId,
                Quantity = quantity
            };
            _context.CartItems.Add(newItem);
            await _context.SaveChangesAsync();
            return newItem;
        }

        public async Task<CartItem?> UpdateCartItemAsync(Guid userId, int productId, int quantity)
        {
            var existingItem = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
            if (existingItem == null) return null;

            existingItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            return existingItem;
        }

        public async Task<bool> RemoveFromCartAsync(Guid userId, int productId)
        {
            var existingItem = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
            if (existingItem == null) return false;

            _context.CartItems.Remove(existingItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
