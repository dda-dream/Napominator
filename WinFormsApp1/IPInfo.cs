using Emgu.CV.Aruco;
using Napominator;
using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;

public class IpInfoData
{
    public string query { get; set; } = "";
    public string countryCode { get; set; } = "";
    public string country { get; set; } = "";
}

public class IpInfo
{
    private readonly HttpClient httpClient;
    HttpClientHandler handlerHttpClient;
    string proxy;
    string url = AppSettings.Network.IpInfoUrl;

    public IpInfo(string proxy)
	{
        this.proxy = proxy;
        handlerHttpClient = new HttpClientHandler
        {
            Proxy = new WebProxy(proxy),
            UseProxy = true
        };
        httpClient = new HttpClient(handlerHttpClient);
        httpClient.Timeout = TimeSpan.FromSeconds(AppSettings.Network.HttpClientTimeoutSeconds);
    }

    public async Task<(IpInfoData, Dictionary<string, string>)> Process()
    {
        IpInfoData ipInfoData = new IpInfoData();
        Dictionary<string, string> replyResult = new Dictionary<string, string>();

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                ipInfoData = JsonSerializer.Deserialize<IpInfoData>(jsonResponse);

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

        if (handlerHttpClient != null)
            handlerHttpClient.Dispose();
        if (httpClient != null)
            httpClient.Dispose();

        return (ipInfoData, replyResult);
    }
}

