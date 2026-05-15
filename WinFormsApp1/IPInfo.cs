using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;


namespace Napominator;


public class IpInfoDTO
{
    public string? query { get; set; }
    public string? country { get; set; }
    public string? countryCode { get; set; }
    public string? http_response_status1 { get; set; }
    public string? http_response_status2 { get; set; }
    public string? http_response_status3 { get; set; } 

    public string? http_response_1 { get; set; }
    public string? http_response_2 { get; set; }
    public string? http_response_3 { get; set; }

    public string? Ping_RoundtripTime { get; set; }

    public string? Ping_Status { get; set; }
}

public class IpInfo : IDisposable
{
    readonly Dictionary<string, HttpClient> httpClients;
    bool _usePing = false;
    bool _showContentLength = false;

    public string urlTestIP1 { get; set; } = "http://ip-api.com/json/";
    public string urlTestIP2 { get; set; } = "https://rutor.info";
    public string urlTestIP3 { get; set; } = "https://youtube.com";
    public string Proxy { get; set; } = "";
    int httpClientTimeoutSeconds = 0;
    IConfigService? _settings;
    HttpClientHandler? handlerHttpClient;

    public IpInfo(IConfigService settings)
    {
        _settings = settings;
        httpClients = new Dictionary<string, HttpClient>();
    }

    public void Create(bool usePing, bool showContentLength, int httpTimeout)
    {
        _usePing = usePing;
        _showContentLength = showContentLength;


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
    }

    public async Task<IpInfoDTO> Process()
    {
        IpInfoDTO ipInfoData = new IpInfoDTO();

        var t1 = Task.Run(async () =>
        {
            // try URL 1
            var stopwatch = Stopwatch.StartNew();
            try
            {
                HttpResponseMessage response = await httpClients[Proxy].GetAsync(urlTestIP1);

                if (response.IsSuccessStatusCode)
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(httpClientTimeoutSeconds));
                    string jsonResponse = await response.Content.ReadAsStringAsync(cts.Token);
                    ipInfoData = JsonSerializer.Deserialize<IpInfoDTO>(jsonResponse) ?? new IpInfoDTO();
                    ipInfoData.http_response_1 = jsonResponse;

                    stopwatch.Stop();
                    long headerTimeMs = stopwatch.ElapsedMilliseconds;
                    ipInfoData.http_response_status1 = $"{response.StatusCode.ToString()} {headerTimeMs} ms";

                    if (_usePing && ipInfoData.query != null)
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
                    ipInfoData.http_response_status1 = $"ERR:{response.StatusCode}";
            }
            catch (OperationCanceledException)
            {
                ipInfoData.http_response_status1 = "TIMEOUT";
            }
            catch (Exception ex)
            {
                ipInfoData.http_response_status1 = "ERR";
            }
            stopwatch.Stop();
        });

        // try URL 2
        var t2 = Task.Run(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                HttpResponseMessage response = await httpClients[Proxy].GetAsync(urlTestIP2, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    if (_showContentLength)
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(httpClientTimeoutSeconds));
                        string jsonResponse = await response.Content.ReadAsStringAsync(cts.Token);
                        ipInfoData.http_response_2 = jsonResponse;
                    }

                    stopwatch.Stop();
                    long headerTimeMs = stopwatch.ElapsedMilliseconds;
                    ipInfoData.http_response_status2 = $"{response.StatusCode.ToString()} {headerTimeMs} ms";
                }
                else
                    ipInfoData.http_response_status2 = $"ERR:{response.StatusCode}";
            }
            catch (OperationCanceledException)
            {
                ipInfoData.http_response_status2 = "TIMEOUT";
            }
            catch (Exception ex)
            {
                ipInfoData.http_response_status2 = "ERR";
            }
            stopwatch.Stop();
        });

        // try URL 3
        var t3 = Task.Run(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                HttpResponseMessage response = await httpClients[Proxy].GetAsync(urlTestIP3, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    if (_showContentLength)
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(httpClientTimeoutSeconds));
                        string jsonResponse = await response.Content.ReadAsStringAsync(cts.Token);
                        ipInfoData.http_response_3 = jsonResponse;
                    }

                    stopwatch.Stop();
                    long headerTimeMs = stopwatch.ElapsedMilliseconds;
                    ipInfoData.http_response_status3 = $"{response.StatusCode.ToString()} {headerTimeMs} ms";
                }
                else
                    ipInfoData.http_response_status3 = $"ERR:{response.StatusCode}";
            }
            catch (OperationCanceledException)
            {
                ipInfoData.http_response_status3 = "TIMEOUT";
            }
            catch (Exception ex)
            {
                ipInfoData.http_response_status3 = "ERR";
            }
            stopwatch.Stop();

        });

        await Task.WhenAll(t1, t2, t3);

        return ipInfoData;
    }
}