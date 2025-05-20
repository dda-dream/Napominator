using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Napominator
{
    partial class Form1
    {
        public static void TakeScreenshotFromWEBCameraViaEmguCV()
        {
            try
            {
                VideoCapture myVideoCapture = new VideoCapture("rtsp://padmin:Qweqwe123@192.168.2.67:554/stream1");
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
        void KillProcess(string _name, bool _add_textBox_Log)
        {
            if (_name == "")
                return;
            this.Invoke((MethodInvoker)delegate { Add_textBox_Log("s: KillProcess(): _name=" + _name, true); });
            foreach (var process in Process.GetProcessesByName(_name))
            {
                string process_username = GetProcessOwner(process.Id).Split('\\')[1];
                string current_username = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                if (process_username == current_username)
                {
                    if (_add_textBox_Log)
                        this.Invoke((MethodInvoker)delegate { Add_textBox_Log("KillProcess(): Killing start. Id = " + process.Id, true, EventLogEntryType.Warning); });
                    try
                    {
                        //System.Diagnostics.Process.Start ("taskkill.exe", "/f /im " + _name);
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        this.Invoke((MethodInvoker)delegate { Add_textBox_Log("KillProcess(): exception: " + ex.ToString(), true, EventLogEntryType.Error); });
                    }
                    if (_add_textBox_Log)
                        this.Invoke((MethodInvoker)delegate { Add_textBox_Log("KillProcess(): Killing end: " + process.ProcessName, true); });
                }
            }
            this.Invoke((MethodInvoker)delegate { Add_textBox_Log("e: KillProcess(): _name=" + _name, true); });
        }
        private void DisableControls(Control con)
        {
            foreach (Control c in con.Controls)
            {
                DisableControls(c);
            }
            if (con.Name != "Form1")
                con.Enabled = false;
        }
        private void EnableControls(Control? con)
        {
            if (con != null)
            {
                con.Enabled = true;
                EnableControls(con!.Parent);
            }
        }

        List<string> settingsLines = new List<string>();
        List<string> settingsNotifyLines = new List<string>();
        void ReadSettingFile(string settingFileName = "Settings.txt")
        {
            try
            {
                bool startFound = false;
                bool startNotifyFound = false;
                if (File.Exists(settingFileName))
                {
                    settingsLines = new List<string>();
                    settingsNotifyLines = new List<string>();
                    string[] lines = File.ReadAllLines(settingFileName);
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
                }
            }
            catch
            {
                //мало ли файл будет открыт или изменен в один и тот же момент.
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
            string ret = "";

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
        bool CheckStringContainsInList(string _searchItem, string _whereToSearch)
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
        void Add_textBox_Log(string _text, bool _writeToLogFileOrRegistry = true, EventLogEntryType _mode = EventLogEntryType.Information)
        {
            string timeStr = DateTime.Now.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToLongTimeString() + ": ";


            if (_writeToLogFileOrRegistry)
            {
                WriteToRegistryLog(_text, _mode, USERNAME);
                /* NO WRITE TO LOG FILE
                string filename = "";
                if (_mode == "") filename = "log-" + USERNAME + ".txt";
                if (_mode == "logblock") filename = "logblock-" + USERNAME + ".txt";
                try
                {
                    var file = File.AppendText(filename);
                    file.AutoFlush = false;
                    file.WriteLine(timeStr + _text);
                    file.Close();
                }
                catch (Exception e) 
                {
                    eventLog.WriteEntry(username + " " + e.ToString(), EventLogEntryType.Error, 3, 1);
                }
                */
            }

            if (textBox_Log.Lines.Count() > 100)
                textBox_Log.Text = "";

            textBox_Log.Text = textBox_Log.Text + timeStr + _text + Environment.NewLine;
            textBox_Log.SelectionStart = textBox_Log.Text.Length;
            textBox_Log.SelectionLength = 0;
            textBox_Log.ScrollToCaret();
        }
        static void WriteToRegistryLog(string _text, EventLogEntryType _mode, string _USERNAME)
        {
            EventLog eventLog = new EventLog();

            if (!EventLog.Exists("NAPOMINATOR")) // RUN AS ADMIN FIRST TIME
            {
                MessageBox.Show(" RUN AS ADMIN FIRST TIME to allow create EventLog.CreateEventSource(NAPOMINATOR)");
                EventLog.CreateEventSource("NAPOMINATOR", "NAPOMINATOR");
            }
            eventLog.Source = "NAPOMINATOR";

            eventLog.WriteEntry(_USERNAME + " : " + _text, _mode, 1, 1);
        }
        string GetActiveWindowTitle()
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
                this.Invoke((MethodInvoker)delegate { Add_textBox_Log("err: GetProcessOwner(): processId=" + processId.ToString(), true); });
                return "ERR_DOMAIN\\ERR_OWNER";
            }

            return "NO_DOMAIN\\NO_OWNER";
        }
        string Parse_NotifyText()
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

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }
        public void ManageFolderSize(string path, double maxFolderSizeInGB, int days)
        {
            try
            {
                long maxFolderSizeInBytes = (long)(maxFolderSizeInGB * 1024 * 1024 * 1024);
                long currentFolderSize = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                             .Sum(file => new FileInfo(file).Length);

                if (currentFolderSize > maxFolderSizeInBytes)
                {
                    Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                             .Where(file => (DateTime.Now - new FileInfo(file).CreationTime).TotalDays > days)
                             .ToList()
                             .ForEach(File.Delete);
                }
            }
            catch (Exception ex)
            {
                // Optionally handle the exception or log it
            }
        }
        public void CaptureScreenshotByMouseClick()
        {
            string dirToStore = @"C:\NAPOMINATOR\screenshots";
            try
            {       
                //HACK: если JpgScreenshotsDirSizeInGb == 0, тогда минимальный размер скриншотов 1 Гиг.
                long JpgScreenshotsDirSizeInGb = (long)GetDoubleFromSettings("[JpgScreenshotsDirSizeInGb]");
                JpgScreenshotsDirSizeInGb = JpgScreenshotsDirSizeInGb == 0 ? 1 : JpgScreenshotsDirSizeInGb;

                ManageFolderSize(dirToStore, JpgScreenshotsDirSizeInGb, 7);

                int screenWidth=0, screenHeight=0;
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

                    EncoderParameters encoderParameters = new EncoderParameters(1);
                    long jpgQuality = (long)GetDoubleFromSettings("[JpgQuality]");
                    if (jpgQuality < 1)//HACK: если jpgQuality < 1 тогда не создавать скриншот.
                        return;

                    string s = (string)("jpgQuality=" + jpgQuality).ToString();

                    encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpgQuality);
                    ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);
                    bitmap.Save(filePath, jpegCodec, encoderParameters);
                }
            }
            catch { 
            }
        }
    }
}//namespace Napominator
