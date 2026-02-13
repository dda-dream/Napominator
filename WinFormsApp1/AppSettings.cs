using Emgu.CV.Ocl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Napominator.AppSettings;

namespace Napominator
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<IConfigService, AppSettings>();

            return services;
        }
    }

    public interface IConfigService
    {
        IConfiguration Config { get; }
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

            if (!String.IsNullOrEmpty(ENV))
                builder.AddJsonFile($"appsettings.{ENV}.json", optional: true);

            _configuration = builder.Build();
            
            NetworkConfig = new NetworkConfig(this);
        }

        public IConfiguration Config
        {
            get {
                return _configuration;
            }
        }
        public NetworkConfig NetworkConfig { get; }


    }


    public class NetworkConfig
    {
        public string IpInfoUrl { get; }
        public string Proxy { get; }
        public int HttpClientTimeoutSeconds { get; }

        public NetworkConfig(AppSettings appSettings)
        {

            var s = appSettings.Config["Network:IpInfoUrl"];
            if (string.IsNullOrEmpty(s))
                s = "http://ip-api.com/json/";
            IpInfoUrl = s;

            s = appSettings.Config["Network:Proxy"];
            if (String.IsNullOrEmpty(s))
                s = "http://10.66.66.42:8888";
            Proxy = s;

            var i = appSettings.Config.GetValue<int>("Network:HttpClientTimeoutSeconds");
            if (i <= 0)
                i = 10;
            HttpClientTimeoutSeconds = i;
        }
    }

}
