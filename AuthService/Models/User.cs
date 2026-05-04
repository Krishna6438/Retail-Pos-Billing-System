namespace AuthService.Models;

public class User
{
    public int Id { get; set; }

    public string? Name { get; set; }
    public string? Email { get; set; }

    public string? PasswordHash { get; set; }  

    public int RoleId { get; set; }
    public int StoreId { get; set; }

    public Role? Role { get; set; }
    public Store? Store { get; set; }
}