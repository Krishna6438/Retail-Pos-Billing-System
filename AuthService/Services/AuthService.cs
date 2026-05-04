using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Services;

public class AuthService
{
    private readonly UserRepository _repo;
    private readonly JwtService _jwt;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(UserRepository repo, JwtService jwt, IPasswordHasher<User> passwordHasher)
    {
        _repo = repo;
        _jwt = jwt;
        _passwordHasher = passwordHasher;
    }

    public string Register(RegisterRequestDTO dto)
    {
        if (_repo.ExistsByEmail(dto.Email))
            throw new Exception("User already exists with this email");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            RoleId = 3, // Always register as 'User'
            StoreId = dto.StoreId > 0 ? dto.StoreId : 1
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _repo.AddUser(user);
        return "User registered";
    }

    public AuthResponseDTO? Login(LoginRequestDTO dto)
    {
        var user = _repo.GetByEmail(dto.Email);

        if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            return null;

        var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (passwordVerification == PasswordVerificationResult.Failed)
        {
            // Allow one-time migration from the previous SHA256 storage scheme.
            if (user.PasswordHash != HashPassword(dto.Password))
                return null;

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            _repo.UpdateUser(user);
        }
        else if (passwordVerification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            _repo.UpdateUser(user);
        }

        var token = _jwt.GenerateToken(user);

        return new AuthResponseDTO
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            Role = user.RoleId.ToString()
        };
    }

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }
}
