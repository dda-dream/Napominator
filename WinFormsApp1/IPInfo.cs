using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;


namespace Napominator;


public class IpInfoDTO
{
    public string query { get; set; } = "";
    public string countryCode { get; set; } = "";
    public string country { get; set; } = "";
    public string http_response_status1 { get; set; } = "";
    public string http_response_status2 { get; set; } = "";
    public string http_response_status3 { get; set; } = "";
}

public class IpInfo : IDisposable
{
    Dictionary<string, HttpClient> httpClients;
    //private HttpClient httpClient;
    bool usePing = false;

    HttpClientHandler handlerHttpClient;
    public string urlTestIP1 { get; set; } = "http://ip-api.com/json/";
    public string urlTestIP2 { get; set; } = "https://rutor.info";
    public string urlTestIP3 { get; set; } = "https://youtube.com";
    public string Proxy { get; set; } = "";
    int httpClientTimeoutSeconds = 0;
    IConfigService _settings;

    public IpInfo(IConfigService settings)
    {
        _settings = settings;
        httpClients = new Dictionary<string, HttpClient>();
    }

    public void Create(bool usePing, int httpTimeout)
    {
        urlTestIP1 = _settings.NetworkConfig.IpInfoUrl;
        if (String.IsNullOrEmpty(Proxy) && !String.IsNullOrEmpty(_settings.NetworkConfig.Proxy))
            Proxy = _settings.NetworkConfig.Proxy;

        if (httpTimeout > 0)
            httpClientTimeoutSeconds = httpTimeout;
        else
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
        // try URL 1
        try
        {
            HttpResponseMessage response = await httpClients[Proxy].GetAsync(urlTestIP1);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                ipInfoData = JsonSerializer.Deserialize<IpInfoDTO>(jsonResponse);

                ipInfoData.http_response_status1 = response.StatusCode.ToString();

                if (usePing)
                {
                    PingReply reply;
                    using (Ping pingSender = new Ping())
                    {
                        reply = await pingSender.SendPingAsync(ipInfoData.query, 5000);
                        replyResult.Add("RoundtripTime", reply.RoundtripTime.ToString());
                        replyResult.Add("Status", reply.Status.ToString());
                    }
                }
            }
            else
            {
                ipInfoData.http_response_status1 = "ERR:"+ response.StatusCode;
            }
        }
        catch (Exception ex)
        {
            ipInfoData.http_response_status1 = "ERR";
        }

        // try URL 2
        try
        {
            HttpResponseMessage response = await httpClients[Proxy].GetAsync(urlTestIP2);

            if (response.IsSuccessStatusCode)
            {
                //string jsonResponse = await response.Content.ReadAsStringAsync();
                ipInfoData.http_response_status2 = response.StatusCode.ToString();
            }
            else
            {
                ipInfoData.http_response_status2 = "ERR:" + response.StatusCode;
            }
        }
        catch (Exception ex)
        {
            ipInfoData.http_response_status2 = "ERR";
        }

        // try URL 3
        try
        {
            HttpResponseMessage response = await httpClients[Proxy].GetAsync(urlTestIP3);

            if (response.IsSuccessStatusCode)
            {
                //string jsonResponse = await response.Content.ReadAsStringAsync();
                ipInfoData.http_response_status3 = response.StatusCode.ToString();
            }
            else
            {
                ipInfoData.http_response_status3 = "ERR:" + response.StatusCode;
            }
        }
        catch (Exception ex)
        {
            ipInfoData.http_response_status3 = "ERR";
        }



        return (ipInfoData, replyResult);
    }
}