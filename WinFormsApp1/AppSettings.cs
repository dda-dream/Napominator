using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Runtime;

namespace Napominator;


public interface IAppSettings
{
    IConfiguration Config { get; }
    NetworkConfig? NetworkConfig { get; }
    RabbitMQConfig? RabbitMQConfig { get; }
    ChatConnectionConfig? ChatConnectionConfig { get; }
    NapominatorWebApi? NapominatorWebApi {  get; }
    List <(string Name, string Url)> TestProxyUrls { get; }
}


public class AppSettings : IAppSettings
{
    private readonly IConfiguration _configuration;
    public NetworkConfig? NetworkConfig { get; }
    public RabbitMQConfig? RabbitMQConfig { get; }
    public ChatConnectionConfig? ChatConnectionConfig { get; }
    public NapominatorWebApi? NapominatorWebApi { get; }
    public List<(string Name, string Url)> TestProxyUrls { get; }

    public AppSettings()
    { 
        string ENV = Environment.GetEnvironmentVariable("NAPOMINATOR_ENVIRONMENT") ?? "";

        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        if (!string.IsNullOrEmpty(ENV))
            builder.AddJsonFile($"appsettings.{ENV}.json", optional: true);

        _configuration = builder.Build();
        
        NetworkConfig = _configuration.GetSection("Network").Get<NetworkConfig>();
        RabbitMQConfig = _configuration.GetSection("RabbitMQ").Get<RabbitMQConfig>();
        ChatConnectionConfig = _configuration.GetSection("ChatConnection").Get<ChatConnectionConfig>();
        NapominatorWebApi = _configuration.GetSection("NapominatorWebApi").Get<NapominatorWebApi>();
        var testProxyUrlsDict = _configuration.GetSection("TestProxyUrls").Get<Dictionary<string, string>>();

        TestProxyUrls = testProxyUrlsDict?
            .Select(x => (Name: x.Key, Url: x.Value))
            .ToList() ?? new List<(string Name, string Url)>();

        ValidateAndThrow();
    }


    private void ValidateAndThrow()
    {
        if (NapominatorWebApi == null)
            throw new Exception("[ERROR] NapominatorWebApi == null");
        if(NapominatorWebApi.BaseUrl == null)
            throw new Exception("[ERROR] NapominatorWebApi.BaseUrl == null");
    }
    
    public IConfiguration Config
    {
        get {
            return _configuration;
        }
    }

}

public class RabbitMQConfig
{
    public string Host { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ChatConnectionConfig
{
    public string CheckUnreadUrl { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string CheckForUser { get; set; } = string.Empty;
    public int PeriodCheck {  get; set; }
}


public class NetworkConfig
{
    public string IpInfoUrl { get; set; } = "http://ip-api.com/json/";
    public string Proxy { get; set; } = "http://10.66.66.42:8888";
    public int HttpClientTimeoutSeconds { get; set; } = 10;
    public bool DebugEnabled { get; set;  } = false;
}

public class NapominatorWebApi
{
    public string BaseUrl { get; set; }
    public string GetEndpoint { get; set; }
}

