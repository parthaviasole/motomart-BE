using motomart_BE.Data;
using motomart_BE.Helpers;
using motomart_BE.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace motomart_BE.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<Product>> GetProducts(int pageNumber, int pageSize, string? searchTerm = null, string? name = null, string? type = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || (p.Details != null && p.Details.Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(p => p.Type == type);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            return await PagedList<Product>.CreateAsync(query.OrderByDescending(p => p.CreatedAt), pageNumber, pageSize);
        }

        public async Task<List<string>> GetProductTypes()
        {
            return await _context.Products
                .Select(p => p.Type)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<Product> GetProduct(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProduct(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Product>> ImportFromExcel(Stream excelStream)
        {
            var products = new List<Product>();
            using (var workbook = new XLWorkbook(excelStream))
            {
                var worksheet = workbook.Worksheets.First();
                var rows = worksheet.RowsUsed().Skip(1); // Skip header

                foreach (var row in rows)
                {
                    var product = new Product
                    {
                        Type = row.Cell(1).GetString(),
                        Name = row.Cell(2).GetString(),
                        Details = row.Cell(3).GetString(),
                        Price = row.Cell(4).GetValue<decimal>(),
                        Quantity = row.Cell(5).GetValue<int>(),
                        ImageUrl = row.Cell(6).GetString(),
                        CreatedAt = DateTime.UtcNow
                    };
                    products.Add(product);
                }
            }

            if (products.Any())
            {
                _context.Products.AddRange(products);
                await _context.SaveChangesAsync();
            }

            return products;
        }
    }
}
