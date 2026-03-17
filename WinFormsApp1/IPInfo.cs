using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;


namespace Napominator;


public class IpInfoDTO
{
    public string query { get; set; } = "";
    public string countryCode { get; set; } = "";
    public string country { get; set; } = "";
}

public class IpInfo : IDisposable
{
    Dictionary<string, HttpClient> httpClients;
    //private HttpClient httpClient;


    HttpClientHandler handlerHttpClient;
    public string urlTestIP { get; set; } = "";
    public string Proxy { get; set; } = "";
    int httpClientTimeoutSeconds = 0;
    IConfigService _settings;

    public IpInfo(IConfigService settings)
    {
        _settings = settings;
        httpClients = new Dictionary<string, HttpClient>();
    }

    public void Create()
    {
        urlTestIP = _settings.NetworkConfig.IpInfoUrl;
        if (String.IsNullOrEmpty(Proxy) && !String.IsNullOrEmpty(_settings.NetworkConfig.Proxy))
            Proxy = _settings.NetworkConfig.Proxy;

        httpClientTimeoutSeconds = _settings.NetworkConfig.HttpClientTimeoutSeconds;

        handlerHttpClient = new HttpClientHandler
        {
            Proxy = new WebProxy(Proxy),
            UseProxy = true
        };

        if (httpClients.ContainsKey(Proxy) == false)
        {

            HttpClient httpClient = new HttpClient(handlerHttpClient);
            httpClient.Timeout = TimeSpan.FromSeconds(httpClientTimeoutSeconds);
            httpClients.Add(Proxy, httpClient);
        }
    }

    public void Dispose()
    {
        if (handlerHttpClient != null)
            handlerHttpClient.Dispose();
        //if (httpClient != null)
        //    httpClient.Dispose();
    }

    public async Task<(IpInfoDTO, Dictionary<string, string>)> Process()
    {
        IpInfoDTO ipInfoData = new IpInfoDTO();
        Dictionary<string, string> replyResult = new Dictionary<string, string>();

        try
        {
            HttpResponseMessage response = await httpClients[Proxy].GetAsync(urlTestIP);

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