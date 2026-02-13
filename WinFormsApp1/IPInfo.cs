using Emgu.CV.Aruco;
using Microsoft.Extensions.DependencyInjection;
using Napominator;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using static Napominator.AppSettings;

public class IpInfoDTO
{
    public string query { get; set; } = "";
    public string countryCode { get; set; } = "";
    public string country { get; set; } = "";
}

public class IpInfo : IDisposable
{
    private readonly HttpClient httpClient;
    HttpClientHandler handlerHttpClient;
    public string url { get; set; } = "";
    public string Proxy { get; set; } = "";
    int httpClientTimeoutSeconds=0;

    public IpInfo(IConfigService _settings)
	{
        var settings = (AppSettings)_settings;

        url = settings.NetworkConfig.IpInfoUrl;
        if (String.IsNullOrEmpty(Proxy) && !String.IsNullOrEmpty(settings.NetworkConfig.Proxy))
            Proxy = settings.NetworkConfig.Proxy;

        httpClientTimeoutSeconds = settings.NetworkConfig.HttpClientTimeoutSeconds;

        handlerHttpClient = new HttpClientHandler
        {
            Proxy = new WebProxy(Proxy),
            UseProxy = true
        };
        httpClient = new HttpClient(handlerHttpClient);
        httpClient.Timeout = TimeSpan.FromSeconds(httpClientTimeoutSeconds);
    }

    public void Dispose()
    {
        if (handlerHttpClient != null)
            handlerHttpClient.Dispose();
        if (httpClient != null)
            httpClient.Dispose();
    }

    public async Task<(IpInfoDTO, Dictionary<string, string>)> Process()
    {
        IpInfoDTO ipInfoData = new IpInfoDTO();
        Dictionary<string, string> replyResult = new Dictionary<string, string>();

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                ipInfoData = JsonSerializer.Deserialize<IpInfoDTO>(jsonResponse);

                PingReply reply;
                using (Ping pingSender = new Ping())
                {
                    reply = await pingSender.SendPingAsync(ipInfoData.query, 5000);
                    replyResult.Add("RoundtripTime", reply.RoundtripTime.ToString());
                    replyResult.Add("Status", reply.Status.ToString());
                }
            }
            else
            {
                Console.WriteLine("Ошибка при получении данных: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Исключение: " + ex.Message);
        }

        return (ipInfoData, replyResult);
    }
}

