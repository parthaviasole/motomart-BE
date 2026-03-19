using motomart_BE.Models;
using motomart_BE.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace motomart_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IFileService _fileService;

        public ProductsController(IProductService productService, IFileService fileService)
        {
            _productService = productService;
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null, 
            [FromQuery] string? name = null,
            [FromQuery] string? type = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            var products = await _productService.GetProducts(pageNumber, pageSize, searchTerm, name, type, minPrice, maxPrice);
            
            var result = new {
                items = products,
                currentPage = products.CurrentPage,
                totalPages = products.TotalPages,
                pageSize = products.PageSize,
                totalCount = products.TotalCount,
                hasPrevious = products.HasPrevious,
                hasNext = products.HasNext
            };

            return Ok(result);
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetProductTypes()
        {
            var types = await _productService.GetProductTypes();
            return Ok(types);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProduct(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            var newProduct = await _productService.CreateProduct(product);
            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, newProduct);
        }

        [HttpPost("{id}/upload-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadProductImage(int id, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            
            var product = await _productService.GetProduct(id);
            if (product == null) return NotFound("Product not found.");

            var url = await _fileService.UploadFile(file, "product-images");
            product.ImageUrl = url;
            await _productService.UpdateProduct(product);

            return Ok(new { url });
        }

        [HttpPost("upload-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            var url = await _fileService.UploadFile(file, "product-images");
            return Ok(new { url });
        }

        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            using var stream = file.OpenReadStream();
            var products = await _productService.ImportFromExcel(stream);
            return Ok(products);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (id != product.Id) return BadRequest();
            var updatedProduct = await _productService.UpdateProduct(product);
            return Ok(updatedProduct);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productService.DeleteProduct(id);
            return NoContent();
        }
    }
}
