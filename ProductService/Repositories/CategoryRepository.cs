using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Repositories;

public class CategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Category category)
    {
        _context.Categories.Add(category);
        _context.SaveChanges();
    }

    public List<Category> GetAll()
    {
        return _context.Categories.ToList();
    }

    public Category? GetById(int id)
    {
        return _context.Categories
            .Include(c => c.Products) 
            .FirstOrDefault(c => c.Id == id);
    }

    public void Update()
    {
        _context.SaveChanges();
    }

    public void Delete(Category category)
    {
        _context.Categories.Remove(category);
        _context.SaveChanges();
    }
}