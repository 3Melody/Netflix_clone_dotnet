using MySqlConnector;
using Netflix_clone_dotnet.Models;

namespace Netflix_clone_dotnet.Repositories
{
    public static class ReportRepository
    {

        public static async Task<List<FavoriteItem>> GetFavoritesReport(string connectionString, string startDate, string endDate)
        {
            var result = new List<FavoriteItem>();

            using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "select  Category , count(*) as Total  from userfavorites WHERE Create_at BETWEEN @start AND @end  group by Category order by Total desc;",
                conn
            );
            cmd.Parameters.AddWithValue("@start", startDate);
            cmd.Parameters.AddWithValue("@end", endDate);

            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new FavoriteItem
                {
                    Category = reader.GetInt32(0),
                    Total = reader.GetInt32(1)
                });
            }


            return result;
        }
        

          public static async Task<List<LoginItem>> GetLoginReport(string connectionString, string startDate )
        {
            var result = new List<LoginItem>();

            using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT DATE(create_at) AS login_date, COUNT(*) AS total"
               + " FROM login_history where year(create_at) = year(@start) and month(create_at) = month(@start) "
               + " GROUP BY DATE(create_at)"
               + " ORDER BY total DESC;",
                conn
            );
            cmd.Parameters.AddWithValue("@start", startDate);

            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
            result.Add(new LoginItem
            {
               login_date = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                total = reader.GetInt32(1)
            });
            }

            
            return result;
        }
        public static async Task CreateLoginLogAsync(string connectionString, User user ,string ipAddress)
        {
            using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            var sql = "insert into login_history (user_id , ip_address,create_at , status) values (@id,@ip,now(),1);";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", user.Id);
            cmd.Parameters.AddWithValue("@ip", ipAddress);
            await cmd.ExecuteNonQueryAsync();
        }

    }
}