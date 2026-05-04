using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;

[Authorize]
[ApiController]
[Route("categories")]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _service;

    public CategoryController(CategoryService service)
    {
        _service = service;
    }

    //  POST /categories
    [HttpPost]
    public IActionResult Add(CreateCategoryDTO dto)
    {
        return Ok(new { message = _service.Add(dto) });
    }

    //  GET /categories
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAll());
    }

    //  GET /categories/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var result = _service.GetById(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    //  PUT /categories
    [HttpPut]
    public IActionResult Update(UpdateCategoryDTO dto)
    {
        return Ok(new { message = _service.Update(dto) });
    }

    //  DELETE /categories/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        return Ok(new { message = _service.Delete(id) });
    }
}
