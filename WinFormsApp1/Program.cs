using System.Drawing.Imaging;

namespace WinFormsApp1
{


    internal static class Program
    {


        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }
        public static void CaptureScreenshotByMouseClick()
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            using (Bitmap bitmap = new Bitmap(screenWidth, screenHeight))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                }

                string filePath = $@"C:\NAPOMINATOR\screenshots\screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";

                if (!System.IO.Directory.Exists(@"C:\NAPOMINATOR\screenshots"))
                    System.IO.Directory.CreateDirectory(@"C:\NAPOMINATOR\screenshots");

                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)50);

                ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);
                bitmap.Save(filePath, jpegCodec, encoderParameters);
            }
        }


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            GlobalMouseHook.OnMouseClick += CaptureScreenshotByMouseClick;

            GlobalMouseHook.Start();


            Application.Run(new Form1());
        }
    }
}