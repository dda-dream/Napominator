using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Reflection;

namespace Napominator
{
    internal static class Program
    {
        static Mutex mutex = new Mutex(true, Application.ExecutablePath.Replace("\\",""));
        static bool mutexHasCapture = false;
        public static ServiceProvider _serviceProvider;

        [STAThread]
        static void Main()
        {
            var services = new ServiceCollection();
            services.AddAppServices();
            services.AddTransient<IpInfo>();
            services.AddTransient<MainForm>();
            _serviceProvider = services.BuildServiceProvider();

            try
            {
                if (mutex.WaitOne(TimeSpan.Zero, true))
                {
                    mutexHasCapture = true;
                    ApplicationConfiguration.Initialize();


                    var mainForm = _serviceProvider.GetRequiredService<MainForm>();
                    Application.Run(mainForm);

                    //Application.Run(new MainForm());

                    mutex.ReleaseMutex();
                }
            }
            finally
            {
                if (mutex != null)
                {
                    if(mutexHasCapture)
                        mutex.ReleaseMutex();

                    mutex.Dispose();
                }
            }
        }
    }
}