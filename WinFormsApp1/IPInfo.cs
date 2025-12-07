using Emgu.CV.Aruco;
using System;
using System.Net;
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
    private static readonly HttpClient httpClient = new HttpClient();
    string proxy;
    string url = "http://ip-api.com/json/";

    public IpInfo(string proxy)
	{
        this.proxy = proxy;
	}

    public async Task<(IpInfoData, Dictionary<string, string>)> Process()
    {
        IpInfoData ipInfoData = new IpInfoData();
        Dictionary<string, string> replyResult = new Dictionary<string, string>();

        var handler = new HttpClientHandler
        {
            Proxy = new WebProxy(proxy),
            UseProxy = true
        };

        try
        {
            var h = new HttpClient(handler);
            h.Timeout = TimeSpan.FromSeconds(10);

            HttpResponseMessage response = await h.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                ipInfoData = JsonSerializer.Deserialize<IpInfoData>(jsonResponse);

                PingReply reply;
                using (Ping pingSender = new Ping())
                {
                    reply = await pingSender.SendPingAsync(ipInfoData.query, 5000);
                    //reply = pingSender.Send(ipInfoData.query, 5000);
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

