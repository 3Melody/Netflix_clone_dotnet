
namespace Netflix_clone_dotnet.Controllers
{
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Netflix_clone_dotnet.Repositories;

    [Authorize] // ต้อง login
    [ApiController]
    [Route("report")]
    public class ReportController : ControllerBase
    {
        private readonly string _connectionString;

        public ReportController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }



        [HttpGet("favorites")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFavorites([FromQuery] string startDate, [FromQuery] string endDate)
        {
            var username = User.Identity?.Name;
            if (username == null) return Unauthorized();

            var dbUser = await UserRepository.GetUserByUsernameAsync(_connectionString, username);

            if (!DateTime.TryParse(startDate, out var start)) return BadRequest("Invalid startDate");
            if (!DateTime.TryParse(endDate, out var end)) return BadRequest("Invalid endDate");

            var startDateTime = start.Date;
            var endDateTime = end.Date.AddDays(1).AddTicks(-1);

            Console.WriteLine($"start: {startDateTime.ToString("yyyy-MM-dd HH:mm:ss")}, end: {endDateTime.ToString("yyyy-MM-dd HH:mm:ss")}");

            var favoriteIds = await ReportRepository.GetFavoritesReport(_connectionString, startDateTime.ToString("yyyy-MM-dd HH:mm:ss"), endDateTime.ToString("yyyy-MM-dd HH:mm:ss"));

            return Ok(favoriteIds);
        }

        [HttpGet("loginLogs")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLoginLogs([FromQuery] string startDate)
        {
            var username = User.Identity?.Name;
            if (username == null) return Unauthorized();

            if (!DateTime.TryParse(startDate, out var start)) return BadRequest("Invalid startDate");


            var favoriteIds = await ReportRepository.GetLoginReport(_connectionString, startDate);

            return Ok(favoriteIds);
        }
        


    }
}
