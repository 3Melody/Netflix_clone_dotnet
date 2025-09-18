using Microsoft.AspNetCore.Mvc;
using Netflix_clone_dotnet.Models;
using Netflix_clone_dotnet.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Netflix_clone_dotnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly string _connectionString;

    public AuthController(IConfiguration config)
    {
        _config = config;
        _connectionString = _config.GetConnectionString("DefaultConnection")!;
    }

    private string GenerateJwt(string username)
    {
        var dbUser = UserRepository.GetUserByUsernameAsync(_connectionString, username).Result;
        var key = _config["Jwt:Key"];
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var expireMinutes = int.Parse(_config["Jwt:ExpireMinutes"]!);

       var claims = new[]
        {
            new Claim(ClaimTypes.Name, dbUser.Username),
        };
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.Now.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        var exist = await UserRepository.GetUserByUsernameAsync(_connectionString, user.Username);
        if (exist != null) return BadRequest(new { message = "Username already exists" });

        await UserRepository.CreateUserAsync(_connectionString, user);
        return Ok(new { message = "User registered" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User user)
    {
        var dbUser = await UserRepository.GetUserByUsernameAsync(_connectionString, user.Username);
        if (dbUser == null || dbUser.PasswordHash != user.PasswordHash)
            return Unauthorized();

        var token = GenerateJwt(user.Username);
        return Ok(new { token });
    }
}
