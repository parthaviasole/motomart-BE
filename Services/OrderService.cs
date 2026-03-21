using Microsoft.EntityFrameworkCore;
using motomart_BE.Data;
using motomart_BE.Models;

namespace motomart_BE.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public OrderService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<Order> CreateOrderAsync(Guid userId, Guid addressId, string paymentType, List<CartItemDto> items)
        {
            Console.WriteLine($"Creating order for User: {userId}, Address: {addressId}, Payment: {paymentType}");
            
            var user = await _context.Users.FindAsync(userId) ?? throw new Exception("User not found");
            var address = await _context.Addresses.FindAsync(addressId) ?? throw new Exception("Address not found");

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in items)
            {
                Console.WriteLine($"Processing Product ID: {item.ProductId}, Quantity: {item.Quantity}");
                var product = await _context.Products.FindAsync(item.ProductId) ?? throw new Exception($"Product {item.ProductId} not found");
                
                if (product.Quantity < item.Quantity)
                {
                    Console.WriteLine($"Insufficient quantity for {product.Name}. Available: {product.Quantity}, Requested: {item.Quantity}");
                    throw new Exception($"Insufficient quantity for {product.Name}");
                }

                // Update product quantity
                product.Quantity -= item.Quantity;
                
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                };
                
                totalAmount += orderItem.Price * orderItem.Quantity;
                orderItems.Add(orderItem);
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AddressId = addressId,
                TotalAmount = totalAmount,
                Status = "Pending",
                PaymentType = paymentType,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Order {order.Id} saved to database.");

            try
            {
                // Notify Admin
                var adminEmail = "parthaviasole4@gmail.com"; // From appsettings or config
                var body = $@"
                    <h1>New Order Placed</h1>
                    <p>Order ID: {order.Id}</p>
                    <p>Customer: {user.Name} ({user.Email})</p>
                    <p>Address: {address.Street}, {address.City}, {address.State}, {address.PostalCode}, {address.Country}</p>
                    <p>Total: ${order.TotalAmount}</p>
                    <p>Payment: {order.PaymentType}</p>
                ";
                await _emailService.SendEmailAsync(adminEmail, "New Order Placed", body);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the order creation
                Console.WriteLine($"Failed to send admin notification email: {ex.Message}");
            }

            return order;
        }

        public async Task<PagedResult<Order>> GetUserOrdersAsync(Guid userId, int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<Order>> GetAllOrdersAsync(int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string status)
        {
            var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return false;

            order.Status = status;

            if (status == "Out for Delivery")
            {
                // Generate OTP
                var otp = new Random().Next(100000, 999999).ToString();
                order.Otp = otp;

                // Send OTP to user's email
                var body = $@"
                    <h1>Your Order is Out for Delivery!</h1>
                    <p>Order ID: {order.Id}</p>
                    <p>Your OTP for delivery is: <strong>{otp}</strong></p>
                    <p>Please share this OTP with our delivery partner to receive your order.</p>
                ";
                await _emailService.SendEmailAsync(order.User!.Email, "Order Out for Delivery - OTP", body);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyOtpAsync(Guid orderId, string otp)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.Otp != otp) return false;

            order.Status = "Delivered";
            order.Otp = null; // Clear OTP once used
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResendOtpAsync(Guid orderId)
        {
            var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null || order.Status != "Out for Delivery") return false;

            var otp = new Random().Next(100000, 999999).ToString();
            order.Otp = otp;

            var body = $@"
                <h1>Resent: Your Delivery OTP</h1>
                <p>Order ID: {order.Id}</p>
                <p>Your new OTP for delivery is: <strong>{otp}</strong></p>
            ";
            await _emailService.SendEmailAsync(order.User!.Email, "Resent OTP - Order Delivery", body);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
