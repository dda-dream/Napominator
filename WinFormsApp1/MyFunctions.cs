using Emgu.CV;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Napominator;


class Functions
{

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    [DllImport("user32.dll")]
    public static extern void LockWorkStation();
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

    public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    public const uint KEYEVENTF_KEYUP = 0x0002;
    public const byte VK_MEDIA_NEXT_TRACK = 0xB0;
    public const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
    public const byte VK_MEDIA_PREV_TRACK = 0xB1;
    public const int SW_RESTORE = 9;


    LogController logController;

    HttpClient client;
    public string USERNAME { get; }
    List<string> settingsLines = new List<string>();
    List<string> settingsNotifyLines = new List<string>();
    IPHostEntry host;
    IPAddress ip;
    string ip_last_digit;
    RabbitMQConnection rabbitMQConnection;


    public Functions(LogController logController)
    {
        this.logController = logController;
        
        USERNAME = username2USERNAME(System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1]);
        ArgumentException.ThrowIfNullOrEmpty(USERNAME);
        //TODO: debug
        //HACK: Если нужно запустить под другим пользователем, то менять тут.
        //USERNAME = "p";
        //TODO: debug

        host = Dns.GetHostEntry(Dns.GetHostName());
        ip = host.AddressList.FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork);
        ip_last_digit = ip.ToString().Split(".")[3];

        rabbitMQConnection = new RabbitMQConnection(ip_last_digit, "fbdda.duckdns.org", "q", "guestSuperPuper_!", logController);
    }




    public void TakeScreenshotFromWEBCameraViaEmguCV()
    {
        try
        {
            VideoCapture myVideoCapture = new VideoCapture("rtsp://padmin:Qpassword@192.168.2.67:554/stream1");
            Mat frame = new Mat();
            bool ret = myVideoCapture.Read(frame);
            if (ret)
            {
                string path = System.AppContext.BaseDirectory + "\\photos\\";
                path += "\\EmguCV_kuhcam_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".jpg";
                frame.Save(path);
                myVideoCapture.Dispose();
                frame.Dispose();
            }
        }
        catch { }
    }


    public void KillProcess(string _name, bool _add_textBox_Log)
    {
        if (_name == "")
            return;
        
        logController.Log("s: KillProcess(): _name=" + _name, EventLogEntryType.Information);
        
        foreach (var process in Process.GetProcessesByName(_name))
            {
                string process_username = GetProcessOwner(process.Id).Split('\\')[1];
                string current_username = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                if (process_username == current_username)
                {
                    if (_add_textBox_Log)
                        logController.Log("KillProcess(): Killing start. Id = " + process.Id, EventLogEntryType.Warning);
                    try
                    {
                        //System.Diagnostics.Process.Start ("taskkill.exe", "/f /im " + _name);
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        logController.Log("KillProcess(): exception: " + ex.ToString(), EventLogEntryType.Error);
                    }
                    if (_add_textBox_Log)
                        logController.Log("KillProcess(): Killing end: " + process.ProcessName, EventLogEntryType.Information);
                }
            }
            logController.Log("e: KillProcess(): _name=" + _name, EventLogEntryType.Information);
        }
    public string GetProcessOwner(int processId)
    {
        try
        {
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList.Cast<ManagementObject>())
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    // return DOMAIN\user
                    return argList[1] + "\\" + argList[0];
                }
            }
        }
        catch (Exception)
        {
            logController.Log("err: GetProcessOwner(): processId=" + processId.ToString(), EventLogEntryType.Information);
            return "ERR_DOMAIN\\ERR_OWNER";
        }

        return "NO_DOMAIN\\NO_OWNER";
    }
    public void DisableControls(Control con)
    {
        foreach (Control c in con.Controls)
        {
            DisableControls(c);
        }
        if (con.Name != "MainForm")
            con.Enabled = false;
    }
    public void EnableControls(Control? con)
    {
        if (con != null)
        {
            con.Enabled = true;
            EnableControls(con!.Parent);
        }
    }







    public async Task<string[]> ReadSettingFile(string settingFileName = "Settings.txt")
    {
        string[] lines = [];
        string RabbitMQRequestStatus;

        //=> if RabbitMQ commands 
        (RabbitMQRequestStatus, lines) = await rabbitMQConnection.GetConfig();

        if (RabbitMQRequestStatus == "SendCommandGetConfig")
            return lines;
        if (RabbitMQRequestStatus == "GetResponse" && lines.Length > 0)
            return lines;
        //<= if RabbitMQ commands 

        if (client == null)
        {
            var handler = new HttpClientHandler
            {
                UseProxy = false,
                Proxy = null
            };
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            client = new HttpClient(handler);
        }

        try
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            client.DefaultRequestHeaders.Add("Accept", "text/plain, */*");
            //client.DefaultRequestHeaders.ExpectContinue = false;

            string url = $"https://fbdda.duckdns.org:5005/napominator/Get/{ip_last_digit}";

            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            HttpResponseMessage response = await client.GetAsync(url, cts.Token);
            string content = await response.Content.ReadAsStringAsync(cts.Token);
            content = content.Replace("\t", "");

            lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }
        catch (OperationCanceledException)
        {
            logController.Log("ReadSettingFile:Запрос был отменен по таймауту.");
        }
        catch (HttpRequestException ex)
        {
            logController.Log($"ReadSettingFile:Ошибка запроса: {ex.Message}");
        }
        catch (Exception ex)
        {
            logController.Log($"ReadSettingFile:Непредвиденная ошибка: {ex.Message}");
        }
        finally
        {
        }
        
        if (lines.Length <= 1) //if no HTTP response
        {
            try
            {
                if (System.IO.File.Exists(settingFileName))
                {
                    lines = System.IO.File.ReadAllLines(settingFileName);
                }
            }
            catch { }
        }
        return lines;
    }
    public void ParseSettingFile(string[] lines)
    {
        if (lines.Length <= 0)
            return;
     

        bool startFound = false;
        bool startNotifyFound = false;
        settingsLines = new List<string>();
        settingsNotifyLines = new List<string>();
        foreach (string line in lines)
        {
            if (line.Contains("[USER][ANYCOMPUTER][" + USERNAME + "][START]"))
            {
                startFound = true;
                continue;
            }
            if (startFound && line.Contains("[NOTIFYTEXTSTART]"))
            {
                startNotifyFound = true;
                continue;
            }

            if (line.Contains("[NOTIFYTEXTEND]"))
            {
                startNotifyFound = false;
                continue;
            }
            if (line.Contains("[USER][ANYCOMPUTER][" + USERNAME + "][END]"))
                break;

            if (startFound == true && startNotifyFound == false)
                settingsLines.Add(line.TrimStart().TrimEnd());
            if (startNotifyFound == true)
                settingsNotifyLines.Add(line.TrimStart().TrimEnd());
        }

        return;
    }
    public DateTime GetDateTimeFromSettings(string _settingName)// [allowed time from] [allowed to time]
    {
        DateTime ret = DateTime.MinValue;

        foreach (string line in (List<string>)settingsLines)
        {
            if (line.Contains(_settingName))
            {
                ret = DateTime.Parse(line.Replace(_settingName, ""));
                break;
            }
        }
        return ret;
    }
    public string GetStringFromSettings(string _settingName)
    {
        /*
        string? ret = settingsLines.Where(u => u.Contains(_settingName)).FirstOrDefault();
        ret = ret == null ? "" : ret.Replace(_settingName, "");
        */
        string ret="";
        foreach (string line in (List<string>)settingsLines)
        {
            if (line.Contains(_settingName))
            {
                ret = line.Replace(_settingName, "");
                break;
            }
        }
        
        return ret;
    }
    public double GetDoubleFromSettings(string _settingName)
    {
        double ret = 0;

        foreach (string line in (List<string>)settingsLines)
        {
            if (line.Contains(_settingName))
            {
                ret = Double.Parse(line.Replace(_settingName, ""));
                break;
            }
        }
        return ret;
    }
    string username2USERNAME(string _username)
    {
        string ret = "ERROR";

        if (_username.ToLower() == "r" || _username.ToLower().Contains("lobur"))
            ret = "i";
        if (_username.ToLower() == "d" || _username.ToLower().Contains("dementev_d"))
            ret = "d";
        if (_username.ToLower() == "p" || _username.ToLower().Contains("user"))
            ret = "p";
        return ret;
    }
    public bool CheckStringContainsInList(string _searchItem, string _whereToSearch)
    {
        bool foundany = false;
        List<String> listNoWriteToFile = _whereToSearch.Split(",").ToList();
        foreach (String item in listNoWriteToFile)
        {
            if (item != "" && _searchItem.Contains(item))
            {
                foundany = true;
                break;
            }
        }
        return foundany;
    }
    public string Parse_NotifyText()
    {
        string[] text = settingsNotifyLines.ToArray();
        string ret = "";
        string[] arr = text;
        int curLine = 0;
        float curWeekOfMthFloat;

        curWeekOfMthFloat = (float)DateTime.Now.Day / 7;
        if (curWeekOfMthFloat > (DateTime.Now.Day / 7))
            curWeekOfMthFloat++;

        int curWeekOfMth = (int)curWeekOfMthFloat;
        int curDayOfWeek = ((int)DateTime.Now.DayOfWeek);
        int textWeekOfMth = 0, textDayOfWeek = 0;
        while (curLine < arr.Length)
        {
            if (arr[curLine].Contains("[shedule_week_of_mth]"))
            {
                textWeekOfMth = Int16.Parse(arr[curLine].Replace("[shedule_week_of_mth]", ""));
                curLine++;
                continue;
            }
            if (arr[curLine].Contains("[shedule_day_of_week]"))
            {
                textDayOfWeek = Int16.Parse(arr[curLine].Replace("[shedule_day_of_week]", ""));
                curLine++;
                continue;
            }

            if (textWeekOfMth == -1 && textDayOfWeek == -1)
                ret = ret + arr[curLine] + Environment.NewLine;
            if (textWeekOfMth == curWeekOfMth && textDayOfWeek == curDayOfWeek)
                ret = ret + arr[curLine] + Environment.NewLine;

            curLine++;
        }
        return ret;
    }

    public string GetActiveWindowTitle()
    {
        const int nChars = 256;
        StringBuilder Buff = new StringBuilder(nChars);
        IntPtr handle = GetForegroundWindow();

        if (GetWindowText(handle, Buff, nChars) > 0)
        {
            return Buff.ToString();
        }
        return "";
    }


    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        var codecs = ImageCodecInfo.GetImageEncoders();
        return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
    }

    public void ManageFolderSize(string path, double maxFolderSizeInGB, int days)
    {
        try
        {
            if (!Directory.Exists(path))
                return;

            long maxFolderSizeInBytes = (long)(maxFolderSizeInGB * 1024 * 1024 * 1024);

            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                 .Select(f => new FileInfo(f))
                                 .OrderBy(f => f.CreationTime)
                                 .ToList();

            long currentFolderSize = files.Sum(f => f.Length);

            if (currentFolderSize > maxFolderSizeInBytes)
            {
                DateTime cutoffDate = DateTime.Now.AddDays(-days);

                foreach (var file in files)
                {
                    if (file.CreationTime < cutoffDate)
                    {
                        try
                        {
                            file.Delete();
                            currentFolderSize -= file.Length;
                        }
                        catch { }
                    }

                    if (currentFolderSize <= maxFolderSizeInBytes)
                        break;
                }

                if (currentFolderSize > maxFolderSizeInBytes)
                {
                    foreach (var file in files.Where(f => f.CreationTime >= cutoffDate))
                    {
                        try
                        {
                            file.Delete();
                            currentFolderSize -= file.Length;
                        }
                        catch { }

                        if (currentFolderSize <= maxFolderSizeInBytes)
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
        }
    }
    public void CaptureScreenshotByMouseClick()
    {
        Task.Run(() => CaptureScreenshotAsync());
    }

    private void CaptureScreenshotAsync()
    {
        string dirToStore = @"C:\NAPOMINATOR\screenshots";
        long jpgQuality = (long)GetDoubleFromSettings("[JpgQuality]");
        if (jpgQuality < 1)
            return;

        try
        {
            long JpgScreenshotsDirSizeInGb = (long)GetDoubleFromSettings("[JpgScreenshotsDirSizeInGb]");
            JpgScreenshotsDirSizeInGb = JpgScreenshotsDirSizeInGb == 0 ? 1 : JpgScreenshotsDirSizeInGb;

            Task.Run(() => ManageFolderSize(dirToStore, JpgScreenshotsDirSizeInGb, 7));

            int screenWidth = 0, screenHeight = 0;
            if (Screen.PrimaryScreen != null)
                (screenWidth, screenHeight) = (Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            using (Bitmap bitmap = new Bitmap(screenWidth, screenHeight))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                }


                string filePath = $@"C:\NAPOMINATOR\screenshots\{USERNAME}_scr_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                if (!System.IO.Directory.Exists(dirToStore))
                    System.IO.Directory.CreateDirectory(dirToStore);

                using (EncoderParameters encoderParameters = new EncoderParameters(1))
                {
                    encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpgQuality);
                    ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);
                    bitmap.Save(filePath, jpegCodec, encoderParameters);
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }


}

