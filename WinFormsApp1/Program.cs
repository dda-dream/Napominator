using System.Drawing.Imaging;
using System.Reflection;

namespace Napominator
{
    internal static class Program
    {
        static Mutex mutex = new Mutex(true, Application.ExecutablePath.Replace("\\",""));//System.IO.Path.GetFileName(Application.ExecutablePath));
        
        [STAThread]
        static void Main()
        {
            try
            {
                if (mutex.WaitOne(TimeSpan.Zero, true))
                {
                    ApplicationConfiguration.Initialize();
                    Application.Run(new MainForm());

                    mutex.ReleaseMutex();
                }
            }
            finally
            {
                if (mutex != null)
                {
                    //mutex.ReleaseMutex();
                    mutex.Dispose();
                }
            }
        }
    }
}