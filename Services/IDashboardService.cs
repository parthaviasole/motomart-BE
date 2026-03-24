using motomart_BE.Models;
using System.Threading.Tasks;

namespace motomart_BE.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }

    public class DashboardStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProducts { get; set; }
    }
}
