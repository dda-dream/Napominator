using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Reflection;

namespace Napominator
{
    internal static class Program
    {
        static Mutex mutex = new Mutex(true, Application.ExecutablePath.Replace("\\",""));//System.IO.Path.GetFileName(Application.ExecutablePath));
        static bool mutexHasCapture = false;

        [STAThread]
        static void Main()
        {
            try
            {
                if (mutex.WaitOne(TimeSpan.Zero, true))
                {
                    mutexHasCapture = true;
                    ApplicationConfiguration.Initialize();
                    Application.Run(new MainForm());

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