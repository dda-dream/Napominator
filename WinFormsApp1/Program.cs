using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace Napominator;




class Publisher
{
    public event EventHandler SomethingHappened;

    public void Raise() => SomethingHappened?.Invoke(this, EventArgs.Empty);
}



internal static class Program
{
    static Mutex mutex = new Mutex(true, Application.ExecutablePath.Replace("\\",""));
    static bool mutexHasCapture = false;
    public static ServiceProvider _serviceProvider;


    [STAThread]
    static void Main()
    {
        //.
        // added by d on 35
        try //1 //3
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                mutexHasCapture = true;

                var services = new ServiceCollection();
                services.AddTransient<IpInfo>();
                services.AddSingleton<MainForm>();
                services.AddTransient<IAppSettings, AppSettings>();
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