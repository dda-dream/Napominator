using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace Napominator;

internal static class Program
{
    static Mutex mutex = new Mutex(true, Application.ExecutablePath.Replace("\\",""));
    static bool mutexHasCapture = false;
    public static ServiceProvider _serviceProvider;


    [STAThread]
    static void Main()
    {


        try
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                mutexHasCapture = true;

                var services = new ServiceCollection();
                services.AddTransient<IpInfo>();
                services.AddSingleton<MainForm>();
                services.AddTransient<IConfigService, AppSettings>();
                _serviceProvider = services.BuildServiceProvider();

                ApplicationConfiguration.Initialize();

                var mainForm = _serviceProvider.GetRequiredService<MainForm>();
                Application.Run(mainForm);

                mutex.ReleaseMutex();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
        finally
        {
            if (mutex != null)
            {
                if (mutexHasCapture)
                    mutex.ReleaseMutex();

                mutex.Dispose();
            }

        }
    }
}