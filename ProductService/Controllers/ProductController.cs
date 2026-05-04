using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;
[Authorize]
[ApiController]
[Route("products")]
public class ProductController : ControllerBase
{
    private readonly ProductService.Services.ProductService _service;

    public ProductController(ProductService.Services.ProductService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult AddProduct(CreateProductDTO dto)
    {
        var result = _service.AddProduct(dto);
        return Ok(result);
    }
    
    //  GET ALL PRODUCTS
    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _service.GetAll();
        return Ok(result);
    }

    [HttpGet("barcode/{barcode}")]
    public IActionResult GetByBarcode(string barcode)
    {
        var result = _service.GetByBarcode(barcode);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
    [HttpGet("id/{id}")]
    public IActionResult GetById(int id)
    {
        var result = _service.GetById(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
    
    [HttpPut("inventory/{productId}")]
    public IActionResult UpdateInventory(int productId, int quantity)
    {
        var result = _service.UpdateInventory(productId, quantity);

        if (!result)
            return BadRequest("Insufficient stock");

        return Ok("Inventory updated");
    }
    
    [HttpGet("low-stock")]
    public IActionResult GetLowStock()
    {
        var result = _service.GetLowStockProducts();
        return Ok(result);
    }
    
    [HttpGet("category/{categoryId}")]
    public IActionResult GetByCategory(int categoryId)
    {
        return Ok(_service.GetByCategory(categoryId));
    }
    
    

//  DELETE PRODUCT
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var result = _service.DeleteProduct(id);
        return Ok(new { message = result });
    }
}
