using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public static class EmailService
{
    // แนะนำให้ใช้ environment variables สำหรับ email/password จริง ๆ
    private static string smtpHost = "smtp.gmail.com"; // ตัวอย่าง Gmail
    private static int smtpPort = 587; 
    private static string smtpUser = "plug.jessadaporn@gmail.com"; // ใส่อีเมลของคุณ
    private static string smtpPass = "kzld ottk alot hrzw "; // ใช้ App Password ของ Gmail

    public static async Task SendOtpEmail(string toEmail, string otp)
    {
        var message = new MailMessage();
        message.From = new MailAddress(smtpUser);
        message.To.Add(toEmail);
        message.Subject = "Your OTP Code";
        message.Body = $"Your OTP code is: {otp}. It expires in 5 minutes.";

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
    }
}
