using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Napominator
{
    public class AppSettings
    {
        
        private static readonly Lazy<IConfiguration> _configuration = new
            (
                () =>
                        {
                            string ENV = Environment.GetEnvironmentVariable("NAPOMINATOR_ENVIRONMENT");

                            var builder = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", optional: true)
                                .AddEnvironmentVariables();

                            if(!String.IsNullOrEmpty(ENV))
                                builder.AddJsonFile($"appsettings.{ENV}.json", optional: true);

                            return builder.Build();
                        }
            );

        public static class Network
        {
            public static string IpInfoUrl =>
                _configuration.Value["Network:IpInfoUrl"] ?? "http://ip-api.com/json/";

            public static string Proxy
            {
                get
                {
                    string ret;
                    ret = _configuration.Value["Network:Proxy"] ?? string.Empty;
                    if (String.IsNullOrEmpty(ret))
                        ret = "http://10.66.66.42:8888";
                    return ret;
                }
            }

            public static int HttpClientTimeoutSeconds =>
                int.TryParse(_configuration.Value["Network:HttpClientTimeoutSeconds"], out var timeout) ? timeout : 10;
        }
    }
}
