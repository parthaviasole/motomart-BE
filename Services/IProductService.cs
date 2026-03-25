using motomart_BE.Helpers;
using motomart_BE.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace motomart_BE.Services
{
    public interface IProductService
    {
        Task<PagedList<Product>> GetProducts(int pageNumber, int pageSize, string? searchTerm = null, string? name = null, string? type = null, decimal? minPrice = null, decimal? maxPrice = null);
        Task<List<string>> GetProductTypes();
        Task<object> GetProductTypesSummary();
        Task<Product?> GetProduct(int id);
        Task<Product> CreateProduct(Product product);
        Task<Product> UpdateProduct(Product product);
        Task DeleteProduct(int id);
        Task<List<Product>> ImportProducts(Stream stream, string fileName);
    }
}
