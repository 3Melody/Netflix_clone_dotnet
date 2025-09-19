using MySqlConnector;

public static class OtpRepository
{
    public static async Task SaveOtpAsync(string connectionString, int userId, string otp, DateTime expireAt)
    {
        using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();

        var cmd = new MySqlCommand(
            "INSERT INTO user_otps (UserId, OtpCode, ExpireAt) VALUES (@u, @otp, @exp)", conn);
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@otp", otp);
        cmd.Parameters.AddWithValue("@exp", expireAt);

        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<bool> ValidateOtpAsync(string connectionString, int userId, string otp)
    {
        using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();

        var cmd = new MySqlCommand(
            "SELECT COUNT(*) FROM user_otps WHERE UserId=@u AND OtpCode=@otp AND ExpireAt > NOW() AND IsUsed=0", conn);
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@otp", otp);

        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        if (count > 0)
        {
            // Mark OTP as used
            var updateCmd = new MySqlCommand(
                "UPDATE user_otps SET IsUsed=1 WHERE UserId=@u AND OtpCode=@otp", conn);
            updateCmd.Parameters.AddWithValue("@u", userId);
            updateCmd.Parameters.AddWithValue("@otp", otp);
            await updateCmd.ExecuteNonQueryAsync();

            return true;
        }
        return false;
    }
}
