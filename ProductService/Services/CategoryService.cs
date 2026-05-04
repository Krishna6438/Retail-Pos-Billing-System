using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;

namespace ProductService.Services;

public class CategoryService
{
    private readonly CategoryRepository _repo;

    public CategoryService(CategoryRepository repo)
    {
        _repo = repo;
    }

    // Create
    public string Add(CreateCategoryDTO dto)
    {
        var category = new Category
        {
            Name = dto.Name
        };

        _repo.Add(category);
        return "Category added successfully";
    }

    // Get All
    public List<Category> GetAll()
    {
        return _repo.GetAll();
    }

    //  Get By Id
    public Category? GetById(int id)
    {
        return _repo.GetById(id);
    }

    //  Update
    public string Update(UpdateCategoryDTO dto)
    {
        var category = _repo.GetById(dto.Id);

        if (category == null)
            throw new Exception("Category not found");

        category.Name = dto.Name;

        _repo.Update();

        return "Category updated";
    }

    //  Delete
    public string Delete(int id)
    {
        var category = _repo.GetById(id);

        if (category == null)
            throw new Exception("Category not found");

        _repo.Delete(category);

        return "Category deleted";
    }
}