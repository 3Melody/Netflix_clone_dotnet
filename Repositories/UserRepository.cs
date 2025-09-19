using MySqlConnector;
using Netflix_clone_dotnet.Models;

namespace Netflix_clone_dotnet.Repositories;

public static class UserRepository
{
    public static async Task CreateUserAsync(string connectionString, User user)
    {
        using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();

        var sql = "INSERT INTO Users (Username, PasswordHash, Email ) VALUES (@u,@p,@e)";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@u", user.Username);
        cmd.Parameters.AddWithValue("@p", user.PasswordHash);
        cmd.Parameters.AddWithValue("@e", user.Email);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<User?> GetUserByUsernameAsync(string connectionString, string username)
    {
        using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();

        var sql = "SELECT Id, Username, PasswordHash , Role , Email FROM Users WHERE Username=@u";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@u", username);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32("Id"),
                Username = reader.GetString("Username"),
                PasswordHash = reader.GetString("PasswordHash"),
                Email = reader.GetString("Email"),
                Role = reader.GetString("Role")
            };
        }

        return null;
    }

    public static async Task UpdatePasswordAsync(string connectionString, int userId, string newPasswordHash)
    {
        using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();

        var sql = "UPDATE Users SET PasswordHash = @p WHERE Id = @id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@p", newPasswordHash);
        cmd.Parameters.AddWithValue("@id", userId);
        await cmd.ExecuteNonQueryAsync();
    }
}
