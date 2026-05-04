using ProductService.DTOs.Tax;
using ProductService.Models;
using ProductService.Repositories;

namespace ProductService.Services;

public class TaxService
{
    private readonly TaxRepository _repo;

    public TaxService(TaxRepository repo)
    {
        _repo = repo;
    }

    //  CREATE
    public string Add(CreateTaxDTO dto)
    {
        var tax = new TaxConfig
        {
            Name = dto.Name,
            TaxPercentage = dto.TaxPercentage
        };

        _repo.Add(tax);

        return "Tax created successfully";
    }

    //  GET ALL
    public List<TaxConfig> GetAll()
    {
        return _repo.GetAll();
    }

    //  GET BY ID
    public TaxConfig? GetById(int id)
    {
        return _repo.GetById(id);
    }

    //  UPDATE
    public string Update(UpdateTaxDTO dto)
    {
        var tax = _repo.GetById(dto.Id);

        if (tax == null)
            throw new Exception("Tax not found");

        tax.Name = dto.Name;
        tax.TaxPercentage = dto.TaxPercentage;

        _repo.Update();

        return "Tax updated";
    }

    //  DELETE
    public string Delete(int id)
    {
        var tax = _repo.GetById(id);

        if (tax == null)
            throw new Exception("Tax not found");

        _repo.Delete(tax);

        return "Tax deleted";
    }
}