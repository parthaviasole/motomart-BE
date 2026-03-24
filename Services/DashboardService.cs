using Microsoft.EntityFrameworkCore;
using motomart_BE.Data;
using System.Threading.Tasks;

namespace motomart_BE.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            var totalRevenue = await _context.Orders.Where(o => o.Status == "Delivered").SumAsync(o => o.TotalAmount);
            var totalProducts = await _context.Products.CountAsync();

            return new DashboardStatsDto
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                TotalRevenue = totalRevenue,
                TotalProducts = totalProducts
            };
        }
    }
}
