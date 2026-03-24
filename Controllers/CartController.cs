using Microsoft.AspNetCore.Mvc;
using motomart_BE.Models;
using motomart_BE.Services;
using System.Security.Claims;

namespace motomart_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated"));
            var cart = await _cartService.GetUserCartAsync(userId);
            return Ok(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated"));
            var cartItem = await _cartService.AddToCartAsync(userId, dto.ProductId, dto.Quantity);
            return Ok(cartItem);
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateCartItem(int productId, [FromBody] int quantity)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated"));
            var cartItem = await _cartService.UpdateCartItemAsync(userId, productId, quantity);
            if (cartItem == null) return NotFound();
            return Ok(cartItem);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated"));
            var success = await _cartService.RemoveFromCartAsync(userId, productId);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated"));
            await _cartService.ClearCartAsync(userId);
            return Ok();
        }
    }

    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
