
namespace Netflix_clone_dotnet.Controllers
{
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Netflix_clone_dotnet.Repositories;

[Authorize] // ต้อง login
[ApiController]
[Route("favorites")]
public class FavoritesController : ControllerBase
{
    private readonly string _connectionString;

    public FavoritesController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    [HttpPost("add/{movieId}")]
    public async Task<IActionResult> AddFavorite(int movieId)
    {
        var username = User.Identity?.Name;
        if (username == null) return Unauthorized();

        var dbUser = await UserRepository.GetUserByUsernameAsync(_connectionString, username);
        await FavoritesRepository.AddFavoriteAsync(_connectionString, dbUser.Id, movieId);

        return Ok(new { message = "Added to favorites" });
    }

    [HttpGet]
    public async Task<IActionResult> GetFavorites()
    {
        var username = User.Identity?.Name;
        if (username == null) return Unauthorized();

        var dbUser = await UserRepository.GetUserByUsernameAsync(_connectionString, username);
        var favoriteIds = await FavoritesRepository.GetFavoritesAsync(_connectionString, dbUser.Id);

        return Ok(favoriteIds);
    }

    [HttpDelete("delete/{FavoId}")]
    public async Task<IActionResult> RemoveFavorite(int FavoId)
    {
        var username = User.Identity?.Name;
        if (username == null) return Unauthorized();

        await FavoritesRepository.DeleteFavoriteAsync(_connectionString, FavoId);

        return Ok(new { message = "Removed from favorites" });
    }
}
}
