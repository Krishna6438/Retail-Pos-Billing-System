using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs.Tax;
using ProductService.Services;

namespace ProductService.Controllers;

[Authorize]
[ApiController]
[Route("tax")]
public class TaxController : ControllerBase
{
    private readonly TaxService _service;

    public TaxController(TaxService service)
    {
        _service = service;
    }

    //  CREATE
    [HttpPost]
    public IActionResult Add([FromBody] CreateTaxDTO dto)
    {
        return Ok(new { message = _service.Add(dto) });
    }

    //  GET ALL
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAll());
    }

    //  GET BY ID
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var result = _service.GetById(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    //  UPDATE
    [HttpPut]
    public IActionResult Update([FromBody] UpdateTaxDTO dto)
    {
        return Ok(new { message = _service.Update(dto) });
    }

    //  DELETE
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        return Ok(new { message = _service.Delete(id) });
    }
}