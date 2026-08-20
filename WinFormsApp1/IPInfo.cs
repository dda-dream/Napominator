using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Text.Json;
using System.Xml.Linq;


namespace Napominator;


public class IpInfoDTO_CountryCode
{
    public string? query { get; set; }
    public string? country { get; set; }
    public string? countryCode { get; set; }
}
public class IpInfoDTO
{
    public string? query { get; set; }
    public string? country { get; set; }
    public string? countryCode { get; set; }



    public ConcurrentDictionary<string, string> http_response_status { get; set; } = new ConcurrentDictionary<string, string>();
    public ConcurrentDictionary<string, string> http_response { get; set; } = new ConcurrentDictionary<string, string>();



    public string? Ping_RoundtripTime { get; set; }

    public string? Ping_Status { get; set; }
}

public class IpInfo : IDisposable
{
    private readonly ConcurrentDictionary<string, HttpClient> httpClients = new ConcurrentDictionary<string, HttpClient>();
    bool _usePing = false;
    bool _showContentLength = false;

    List<(string Name, string Url)> urlTestIP = new List<(string Name, string Url)>
    {
        ("countrycode","http://ip-api.com/json"),
        ("rutor","https://rutor.info"),
        ("ytb","https://youtube.com"),
        ("grok","https://grok.com\""),
        ("gemini","https://gemini.google.com"),
        ("claude","https://claude.ai"),
        ("whatsapp","https://web.whatsapp.com/"),
    };

    public string Proxy { get; set; } = "";
    int httpClientTimeoutSeconds = 0;
    IAppSettings? _settings;
    HttpClientHandler? handlerHttpClient;

    public IpInfo(IAppSettings settings)
    {
        _settings = settings;
    }

    public void Create(bool usePing, bool showContentLength, int httpTimeout)
    {
        _usePing = usePing;
        _showContentLength = showContentLength;

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

            if (httpClients.TryAdd(Proxy, httpClient) == false)
            {
                Console.WriteLine("[ERROR] if(httpClients.TryAdd(Proxy, httpClient) == false)");
                Console.ReadKey();
            }
        }

        urlTestIP = _settings.TestProxyUrls;
    }



    public void Dispose()
    {
        if (handlerHttpClient != null)
            handlerHttpClient.Dispose();
    }

    public async Task<IpInfoDTO> Process()
    {
        IpInfoDTO ipInfoData = new IpInfoDTO();
        List<Task> tasks = new List<Task>();

        foreach (var url in urlTestIP)
        {
            var i = url;

            var t = Task.Run(async Task<string> () =>
            {
                ipInfoData.http_response[i.Name] = "";
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    using HttpResponseMessage response = await httpClients[Proxy].GetAsync(i.Url, HttpCompletionOption.ResponseHeadersRead);

                    if (response.IsSuccessStatusCode)
                    {
                        if (i.Name == "countrycode" || _showContentLength)
                        {
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(httpClientTimeoutSeconds));
                            string jsonResponse = await response.Content.ReadAsStringAsync(cts.Token);
                            ipInfoData.http_response[i.Name] = jsonResponse;
                            if (i.Name == "countrycode")
                            {
                                IpInfoDTO_CountryCode ipInfoDTO_CountryCode = JsonSerializer.Deserialize<IpInfoDTO_CountryCode>(jsonResponse) ?? new IpInfoDTO_CountryCode();
                                ipInfoData.query = ipInfoDTO_CountryCode.query;
                                ipInfoData.country = ipInfoDTO_CountryCode.country;
                                ipInfoData.countryCode = ipInfoDTO_CountryCode.countryCode;
                            }
                        }
                        stopwatch.Stop();
                        long headerTimeMs = stopwatch.ElapsedMilliseconds;
                        ipInfoData.http_response_status[i.Name] = $"{response.StatusCode} {headerTimeMs} ms";

                        if (_usePing && ipInfoData.query != null && i.Name == "countrycode")
                        {
                            PingReply reply;
                            using (Ping pingSender = new Ping())
                            {
                                reply = await pingSender.SendPingAsync(ipInfoData.query, 5000);
                                ipInfoData.Ping_RoundtripTime = reply.RoundtripTime.ToString();
                                ipInfoData.Ping_Status = reply.Status.ToString();
                            }
                        }
                    }
                    else
                    {
                        ipInfoData.http_response_status[i.Name] = $"ERR:{response.StatusCode}";
                    }
                }
                catch (OperationCanceledException)
                {
                    ipInfoData.http_response_status[i.Name] = "TIMEOUT";
                }
                catch (Exception ex)
                {
                    ipInfoData.http_response_status[i.Name] = "ERR";
                }
                finally
                {
                    stopwatch.Stop();
                }

                return $"{i} - done";
            });

            tasks.Add(t);
        }

        await Task.WhenAll(tasks);

        return ipInfoData;
    }
}