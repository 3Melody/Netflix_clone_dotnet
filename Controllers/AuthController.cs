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
             new Claim(ClaimTypes.Role, dbUser.Role)
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

        string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();

        var token = GenerateJwt(user.Username);
        await ReportRepository.CreateLoginLogAsync(_connectionString, dbUser, ipAddress);
        return Ok(new { token });
    }


    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] UsernameEmailDto dto)
    {
        var user = await UserRepository.GetUserByUsernameAsync(_connectionString, dto.Username);
        if (user == null || user.Email != dto.Email)
            return BadRequest(new { message = "Username or Email not match" });

        // Generate OTP 6 หลัก
        var otp = new Random().Next(100000, 999999).ToString();
        var expireAt = DateTime.UtcNow.AddMinutes(5);

        await OtpRepository.SaveOtpAsync(_connectionString, user.Id, otp, expireAt);

        // ส่ง OTP ไป email
        await EmailService.SendOtpEmail(user.Email, otp);

        return Ok(new { message = "OTP sent to your email" });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var user = await UserRepository.GetUserByUsernameAsync(_connectionString, dto.Username);
        if (user == null) return BadRequest(new { message = "User not found" });

        var valid = await OtpRepository.ValidateOtpAsync(_connectionString, user.Id, dto.Otp);
        if (!valid) return BadRequest(new { message = "Invalid or expired OTP" });

        return Ok(new { message = "OTP verified" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await UserRepository.GetUserByUsernameAsync(_connectionString, dto.Username);
        if (user == null) return BadRequest(new { message = "User not found" });

        // อัปเดตรหัสผ่าน
        await UserRepository.UpdatePasswordAsync(_connectionString, user.Id, dto.NewPassword);
        return Ok(new { message = "Password updated" });
    }

    // DTOs
    public class UsernameEmailDto { public string Username { get; set; } public string Email { get; set; } }
    public class VerifyOtpDto { public string Username { get; set; } public string Otp { get; set; } }
    public class ResetPasswordDto { public string Username { get; set; } public string NewPassword { get; set; } }

}
