using ProductService.Data;
using ProductService.Models;

namespace ProductService.Repositories;

public class TaxRepository
{
    private readonly AppDbContext _context;

    public TaxRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(TaxConfig tax)
    {
        _context.TaxConfigs.Add(tax);
        _context.SaveChanges();
    }

    public List<TaxConfig> GetAll()
    {
        return _context.TaxConfigs.ToList();
    }

    public TaxConfig? GetById(int id)
    {
        return _context.TaxConfigs.Find(id);
    }

    public void Update()
    {
        _context.SaveChanges();
    }

    public void Delete(TaxConfig tax)
    {
        _context.TaxConfigs.Remove(tax);
        _context.SaveChanges();
    }
}