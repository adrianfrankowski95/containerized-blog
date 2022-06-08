namespace Blog.Services.Emailing.API.Config;

public class RabbitMqConfig
{
    public const string Section = "RabbitMq";
    public string Host { get; set; }
    public string VirtualHost { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public const string ReceiveEndpoint = "emailing-api";
}