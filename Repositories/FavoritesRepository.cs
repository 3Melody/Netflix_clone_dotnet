using MySqlConnector;

namespace Netflix_clone_dotnet.Repositories
{
    public static class FavoritesRepository
    {
        public static async Task AddFavoriteAsync(string connectionString, int userId, int movieId)
        {
            using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("INSERT INTO userfavorites (UserId, MovieId) VALUES (@u,@m)", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", movieId);
            await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<List<int>> GetFavoritesAsync(string connectionString, int userId)
        {
            var result = new List<int>();

            using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT MovieId FROM userfavorites WHERE UserId = @u AND Status = 1",
                conn
            );
            cmd.Parameters.AddWithValue("@u", userId);

            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(reader.GetInt32(0)); // MovieId
            }

            return result;
        }


        public static async Task DeleteFavoriteAsync(string connectionString, int FavoId)
        {
            using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("UPDATE userfavorites SET Status = 2 WHERE FavoId = @f", conn);
            cmd.Parameters.AddWithValue("@f", FavoId);
            await cmd.ExecuteNonQueryAsync();
        }

    }
}