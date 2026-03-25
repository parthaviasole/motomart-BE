using motomart_BE.Data;
using motomart_BE.Helpers;
using motomart_BE.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Npgsql;

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
                searchTerm = searchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) || 
                    p.Type.ToLower().Contains(searchTerm) || 
                    (p.Details != null && p.Details.ToLower().Contains(searchTerm)));
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

        public async Task<object> GetProductTypesSummary()
        {
            return await _context.Products
                .GroupBy(p => p.Type)
                .Select(g => new {
                    Type = g.Key,
                    Count = g.Count()
                })
                .OrderBy(t => t.Type)
                .ToListAsync();
        }

        public async Task<Product?> GetProduct(int id)
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

        public async Task<List<Product>> ImportProducts(Stream stream, string fileName)
        {
            var importedProducts = new List<Product>();
            var extension = Path.GetExtension(fileName).ToLower();

            if (extension == ".csv")
            {
                using var reader = new StreamReader(stream);
                var header = await reader.ReadLineAsync(); // Skip header
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length < 6) continue;

                    var product = new Product
                    {
                        Type = values[0].Trim(),
                        Name = values[1].Trim(),
                        Details = values[2].Trim(),
                        Price = decimal.TryParse(values[3], out var p) ? p : 0,
                        Quantity = int.TryParse(values[4], out var q) ? q : 0,
                        ImageUrl = values[5].Trim(),
                        CreatedAt = DateTime.UtcNow
                    };
                    importedProducts.Add(product);
                }
            }
            else // Assume Excel (.xlsx, .xls)
            {
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.First();
                    var rows = worksheet.RowsUsed().Skip(1); // Skip header

                    foreach (var row in rows)
                    {
                        var product = new Product
                        {
                            Type = row.Cell(1).GetString().Trim(),
                            Name = row.Cell(2).GetString().Trim(),
                            Details = row.Cell(3).GetString().Trim(),
                            Price = row.Cell(4).GetValue<decimal>(),
                            Quantity = row.Cell(5).GetValue<int>(),
                            ImageUrl = row.Cell(6).GetString().Trim(),
                            CreatedAt = DateTime.UtcNow
                        };
                        importedProducts.Add(product);
                    }
                }
            }

            if (importedProducts.Any())
            {
                // Sync sequence first to be safe
                try
                {
                    await _context.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('products', 'Id'), COALESCE((SELECT MAX(\"Id\") FROM products), 0) + 1, false);");
                }
                catch { /* Ignore if sequence sync fails initially */ }

                foreach (var importedProduct in importedProducts)
                {
                    var existingProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.Name.ToLower() == importedProduct.Name.ToLower() && p.Type.ToLower() == importedProduct.Type.ToLower());

                    if (existingProduct != null)
                    {
                        existingProduct.Details = importedProduct.Details;
                        existingProduct.Price = importedProduct.Price;
                        existingProduct.Quantity = importedProduct.Quantity;
                        existingProduct.ImageUrl = importedProduct.ImageUrl;
                    }
                    else
                    {
                        _context.Products.Add(importedProduct);
                    }
                }

                try 
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    // If we still get a PK violation, clear the tracker and try a sequence sync + retry
                    _context.ChangeTracker.Clear();
                    
                    // Force sequence sync
                    await _context.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('products', 'Id'), COALESCE((SELECT MAX(\"Id\") FROM products), 0) + 1, false);");
                    
                    // Re-run the upsert logic after clearing tracker
                    foreach (var importedProduct in importedProducts)
                    {
                        var existingProduct = await _context.Products
                            .FirstOrDefaultAsync(p => p.Name.ToLower() == importedProduct.Name.ToLower() && p.Type.ToLower() == importedProduct.Type.ToLower());

                        if (existingProduct != null)
                        {
                            existingProduct.Details = importedProduct.Details;
                            existingProduct.Price = importedProduct.Price;
                            existingProduct.Quantity = importedProduct.Quantity;
                            existingProduct.ImageUrl = importedProduct.ImageUrl;
                        }
                        else
                        {
                            _context.Products.Add(importedProduct);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return importedProducts;
        }
    }
}
