using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Runtime;

namespace Napominator;


public interface IConfigService
{
    IConfiguration Config { get; }
    NetworkConfig NetworkConfig { get; }
    RabbitMQConfig RabbitMQConfig { get; }
}


public class AppSettings : IConfigService
{
    private readonly IConfiguration _configuration;

    public AppSettings()
    { 
        string ENV = Environment.GetEnvironmentVariable("NAPOMINATOR_ENVIRONMENT");

        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        if (!string.IsNullOrEmpty(ENV))
            builder.AddJsonFile($"appsettings.{ENV}.json", optional: true);

        _configuration = builder.Build();
        
        NetworkConfig = _configuration.GetSection("Network").Get<NetworkConfig>();
        RabbitMQConfig = _configuration.GetSection("RabbitMQ").Get<RabbitMQConfig>();
    }

    public IConfiguration Config
    {
        get {
            return _configuration;
        }
    }


    public NetworkConfig NetworkConfig { get; }
    public RabbitMQConfig RabbitMQConfig { get; }
}

public class RabbitMQConfig
{
    public string Host { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class NetworkConfig
{
    public string IpInfoUrl { get; set; } = "http://ip-api.com/json/";
    public string Proxy { get; set; } = "http://10.66.66.42:8888";
    public int HttpClientTimeoutSeconds { get; set; } = 10;
    public bool DebugEnabled { get; set;  } = false;

}
