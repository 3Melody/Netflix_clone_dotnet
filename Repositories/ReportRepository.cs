using MySqlConnector;
using Netflix_clone_dotnet.Models;

namespace Netflix_clone_dotnet.Repositories
{
    public static class ReportRepository
    {

        public static async Task<List<FavoriteItem>> GetFavoritesReport(string connectionString, string startDate , string endDate)
        {
            var result = new List<FavoriteItem>();

            using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "select FavoId , MovieId , Category from userfavorites WHERE Create_at BETWEEN @start AND @end;",
                conn
            );
            cmd.Parameters.AddWithValue("@start", startDate);
            cmd.Parameters.AddWithValue("@end", endDate);

            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
            result.Add(new FavoriteItem
            {
                FavoId = reader.GetInt32(0),
                MovieId = reader.GetInt32(1),
                Category = reader.GetInt32(2)
            });
            }

            
            return result;
        }

    }
}