using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace Napominator;

internal static class Program
{
    static Mutex mutex = new Mutex(true, Application.ExecutablePath.Replace("\\",""));
    static bool mutexHasCapture = false;
    public static ServiceProvider _serviceProvider;


    [STAThread]
    static void Main()
    {
        


        var services = new ServiceCollection();
        services.AddTransient<IpInfo>();
        services.AddSingleton<MainForm>();
        services.AddTransient<IConfigService, AppSettings>();

        _serviceProvider = services.BuildServiceProvider();

        try
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                mutexHasCapture = true;
                ApplicationConfiguration.Initialize();


                var mainForm = _serviceProvider.GetRequiredService<MainForm>();
                //var mainForm = new MainForm();
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