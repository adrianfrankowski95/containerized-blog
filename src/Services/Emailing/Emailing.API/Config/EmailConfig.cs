namespace Blog.Services.Emailing.API.Config;

public class EmailConfig
{
    public const string Section = "Email";
    public string From { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool RequireAuthentication { get; set; }
    public string SocketOptions { get; set; }
}