using System;
using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using static System.Windows.Forms.Design.AxImporter;
using static WinFormsApp1.Form1;
using Emgu.CV;
using static System.Net.Mime.MediaTypeNames;
using System.Management;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        string __VERSION = "22.07.2024 23:24 ";
        Boolean allowFormClose = false;
        string USERNAME="";

        //WasapiCapture? audioSource;
        static WaveIn? audioSource;
        double soundLevel;
        double continuousBig;
        double shuminatorCount = 0, tot_shuminatorCount=0;

        static bool messageShown = false;

        [DllImport("user32.dll")]
        public static extern void LockWorkStation();

            public const int KEYEVENTF_EXTENTEDKEY = 1;
            public const int KEYEVENTF_KEYUP = 0;
            public const int VK_MEDIA_NEXT_TRACK = 0xB0;
            public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
            public const int VK_MEDIA_PREV_TRACK = 0xB1;

            [DllImport("user32.dll")]
            public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        public Form1()
        {
            InitializeComponent();
            Add_textBox_Log("NAPOMINATOR Form1()");
        }

        MMDevice? device = null;
        const int constCountNoMicrophoneDetected = 5;
        int count_NoMicrophoneDetected = constCountNoMicrophoneDetected;
        bool Start_NoiseDetector_executing = false;
        public void Start_NoiseDetector()
        {
            if(GetStringFromSettings("[Shuminator_Enabled]")=="0")
                return;

            if (Start_NoiseDetector_executing == true)
                return;
            Start_NoiseDetector_executing = true;

            Add_textBox_Log("s: Start_NoiseDetector()");

            try
            {
                //device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
                var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture,DeviceState.Active);
                foreach (var devitem in devices)
                {
                    Add_textBox_Log("Microphone name:" + devitem.FriendlyName + " mic level" + devitem.AudioEndpointVolume.MasterVolumeLevelScalar);
                    if (device is null && devitem.FriendlyName.Contains(GetStringFromSettings("[Shuminator_MicrophoneName]")))
                        device = devitem;
                }            
                Add_textBox_Log("Microphone selected. Name:" + device.FriendlyName + " mic level"+ device.AudioEndpointVolume.MasterVolumeLevelScalar);

                audioSource = new WaveIn();
                audioSource.DataAvailable += new EventHandler<WaveInEventArgs>(Microphone_DataAvailable);
                audioSource.RecordingStopped += new EventHandler<NAudio.Wave.StoppedEventArgs>(Microphone_RecordingStopped);
                audioSource.WaveFormat = new WaveFormat(8000, 2);
                audioSource.StartRecording();
            }
            catch
            {
                audioSource = null;
                Add_textBox_Log("No microphone detected.");
                if (GetDoubleFromSettings("[LockWorkStation_NoMicrophone]") == 1)
                {
                    Add_textBox_Log("œÓ‰ÍÎ˛˜Ë ÏËÍÓÙÓÌ!");
                    ShuminatorPlaySoundWarning("podkluchi_microfon.mp3", false);
                    Show_Message_To_Polina("œŒƒ Àﬁ◊» Ã» –Œ‘ŒÕ! \n —◊»“¿ﬁ ƒŒ 0-Îˇ! "+ count_NoMicrophoneDetected.ToString(), "œŒƒ Àﬁ◊» Ã» –Œ‘ŒÕ!", true, false, false, 5);
                    count_NoMicrophoneDetected--;
                    if (count_NoMicrophoneDetected == 0)
                    {
                        count_NoMicrophoneDetected = constCountNoMicrophoneDetected;
                        KillProcess(GetStringFromSettings("[KillProcess_ProcessName]"), true);
                        LockWorkStation();
                    }
                }
            }
            Start_NoiseDetector_executing = false;
            Add_textBox_Log("e: Start_NoiseDetector()");
        }
        static void TakeScreenshotViaEmguCV()
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
            catch  { }
        }

        bool microphone_DataAvailable_wait = false;
        DateTime microphone_DataAvailable_nextExec = DateTime.MinValue;
        void Microphone_DataAvailable(object? sender, WaveInEventArgs e)
        {
            if (device.AudioEndpointVolume.MasterVolumeLevelScalar < GetDoubleFromSettings("[LockWorkStation_Microphone_MasterVolumeLevelScalar]"))
            {
                this.Invoke((MethodInvoker)delegate { Add_textBox_Log("SHUMINATOR LockWorkStation: MasterVolumeLevelScalar!", true, EventLogEntryType.Error); });
                LockWorkStation();
            }

            
            if (messageShown == true)
                return;
            if (microphone_DataAvailable_wait == true)
            {
                if (DateTime.Now > microphone_DataAvailable_nextExec)
                {
                    microphone_DataAvailable_wait = false;
                } else {
                    return;
                }
            }
            
            double prevcontinuousBig = continuousBig;
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                soundLevel = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]) / 32768f;
                
                if (continuousBig < soundLevel)
                    continuousBig = soundLevel;
            }

            if (continuousBig > prevcontinuousBig)
            {
                this.Invoke((MethodInvoker)delegate { Add_textBox_Log("SHUMINATOR soundlevel up: " + continuousBig.ToString("N6"), true);
                    //notifyIcon1.BalloonTipText = "SHUMINATOR soundlevel up: " + continuousBig.ToString("N6");
                    //notifyIcon1.ShowBalloonTip(1000);
                    });            
            }

            double maxNoiseLevel = GetDoubleFromSettings("[ShuminatorPlaySoundWarning_MaxNoiseLevel]");
            string levelsStr = "Alert level: " + maxNoiseLevel.ToString("") + " cnt: " + shuminatorCount.ToString() + " total cnt: " + tot_shuminatorCount.ToString() + " maxDetected level: " + continuousBig.ToString("N6") + " current level: " + soundLevel.ToString("N6");
            tb_noiselevel.Invoke((MethodInvoker)delegate { tb_noiselevel.Text = levelsStr; tb_noiselevel.Update(); });

            if (continuousBig >= maxNoiseLevel)
            {
                Add_textBox_Log("Microphone name:" + device.FriendlyName + " level:"+ device.AudioEndpointVolume.MasterVolumeLevelScalar);
                shuminatorCount++;
                tot_shuminatorCount++;
                messageShown = true;

                this.Invoke((MethodInvoker)delegate { 
                    Add_textBox_Log("SHUMINATOR FIRED at soundLevel: " + levelsStr, true, EventLogEntryType.Warning);
                    Add_textBox_Log("Microphone name:" + device.FriendlyName + " mic level" + device.AudioEndpointVolume.MasterVolumeLevelScalar, true, EventLogEntryType.Warning );
                    });

                if (/*Get photo from cam*/1 == 1)
                {
                    this.Invoke((MethodInvoker)delegate { Add_textBox_Log("s: taking photo from camera. Sound levels:" + levelsStr, true, EventLogEntryType.Warning); });
                    TakeScreenshotViaEmguCV();
                    this.Invoke((MethodInvoker)delegate { Add_textBox_Log("e: taking photo from camera. Sound levels:" + levelsStr, true); });
                    //Get photo from cam
                }
                if (GetDoubleFromSettings("[ShuminatorPlaySoundWarning]") == 1)
                {
                    keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);    // Play/Pause
                    ShuminatorPlaySoundWarning("neshumi.mpeg");
                }
                if (GetDoubleFromSettings("[Show_Message_To_Polina]") == 1)
                {
                    if (USERNAME.Contains("d"))
                        Show_Message_To_Polina("Õ≈ ÿ”Ã»!!! \n ƒ¿… œŒ—œ¿“‹!!!", "Õ≈ ÿ”Ã»!!! " + levelsStr, true,  true, false, 15);
                    else
                        Show_Message_To_Polina("Õ≈ ÿ”Ã»!!! \n ƒ¿… œŒ—œ¿“‹!!!", "Õ≈ ÿ”Ã»!!! " + levelsStr, true, false, false, 15);
                }
                if (GetDoubleFromSettings("[LockWorkStation]") == 1)
                {
                    this.Invoke((MethodInvoker)delegate { Add_textBox_Log("LockWorkStation_shuminatorCount_check: ", true); });
                    if (shuminatorCount >= GetDoubleFromSettings("[LockWorkStation_shuminatorCount]"))
                    {
                        this.Invoke((MethodInvoker)delegate { Add_textBox_Log("LockWorkStation_shuminatorCount_exec: ", true, EventLogEntryType.Warning); });
                        shuminatorCount = 0;
                        LockWorkStation();
                    }
                }
                if (GetDoubleFromSettings("[KillProcess]") == 1)
                {
                    this.Invoke((MethodInvoker)delegate { Add_textBox_Log("KillProcess_check: " + GetStringFromSettings("[KillProcess_ProcessName]"), true); });

                    if (shuminatorCount >= GetDoubleFromSettings("[KillProcess_shuminatorCount]"))
                    {
                        this.Invoke((MethodInvoker)delegate { Add_textBox_Log("KillProcess_exec: " + GetStringFromSettings("[KillProcess_ProcessName]"), true, EventLogEntryType.Warning); });
                        KillProcess(GetStringFromSettings("[KillProcess_ProcessName]"), true);
                        shuminatorCount = 0;
                    }
                }
                messageShown = false;
                continuousBig = 0;
                microphone_DataAvailable_wait = true;
                microphone_DataAvailable_nextExec = DateTime.Now.AddSeconds(10);
            }
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
                        this.Invoke((MethodInvoker)delegate { Add_textBox_Log("KillProcess(): Killing start. Id = " + process.Id, true,EventLogEntryType.Warning); });
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

        static Mp3FileReader? reader0, reader1;
        static WaveOut? waveOut0, waveOut1;
        static void ShuminatorPlaySoundWarning(string _warningSound, bool _playOtherSounds = true)
        {
            try
            {
                if (_playOtherSounds)
                {
                    string path = System.AppContext.BaseDirectory;
                    path = path + "/sounds/";
                    Random rnd = new Random();
                    DirectoryInfo d = new DirectoryInfo(path);
                    FileInfo[] Files = d.GetFiles("*.mp3");

                    if (Files.ToList().Count > 0)
                    {
                        int i = rnd.Next(Files.ToList().Count);

                        reader1 = new Mp3FileReader(Files[i].FullName);
                        waveOut1 = new WaveOut();
                        waveOut1.Init(reader1);
                        waveOut1.Play();
                        waveOut1.PlaybackStopped += WaveOut1_PlaybackStopped;
                    }
                }
                reader0 = new Mp3FileReader(_warningSound);
                waveOut0 = new WaveOut();
                waveOut0.Init(reader0);
                waveOut0.Play();
                waveOut0.PlaybackStopped += WaveOut0_PlaybackStopped;
            } catch (Exception ex)
            {
                WriteToRegistryLog("ShuminatorPlaySoundWarning: "+ex.ToString() , EventLogEntryType.Error, "NO_USERNAME");
            }
        }
        private static void WaveOut1_PlaybackStopped(object? sender, NAudio.Wave.StoppedEventArgs e)
        {
            reader1.Dispose();
            waveOut1.Dispose();
        }
        private static void WaveOut0_PlaybackStopped(object? sender, NAudio.Wave.StoppedEventArgs e)
        {
            reader0.Dispose();
            waveOut0.Dispose();
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);    // Play/Pause
        }
        public static void WriteArrayToFile(double[,] rArr)
        {
            try
            {
                StreamWriter outputfile;
                string comma = "";

                File.WriteAllText("rArr.Txt", string.Empty);
                outputfile = File.AppendText("rArr.Txt");

                for (int i = 0; i < (rArr.GetLength(0)); i++)
                {
                    for (int j = 0; j < (rArr.GetLength(1)); j++)
                    {
                        outputfile.Write(comma);
                        outputfile.Write(rArr[i, j]);
                        comma = "   ";
                    }
                    comma = System.Environment.NewLine;
                }
                outputfile.Close();
            }
            catch { }
        }
        public static void FillArray(double[,] array, double _fillValue=0)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = _fillValue;
                }
            }
        }
        void Microphone_RecordingStopped(object? sender, NAudio.Wave.StoppedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate { Add_textBox_Log("microphone_RecordingStopped!!!!!!!!! ", true, EventLogEntryType.Error); });
            audioSource.Dispose();
            audioSource = null;
        }
        private void Button_StartStop_Click(object sender, EventArgs e)
        {
            StartTimer();
        }
        private void StartTimer()
        {
            if (button_StartStop.Text == "Start")
            {
                Rectangle resolution = Screen.PrimaryScreen!.Bounds;

                Add_textBox_Log("Started. version : 29.12.2022 11:50");
                timer1.Interval = 1000;// * Int32.Parse(textBox_Period.Text) * 1;
                timer1.Start();
                button_StartStop.Text = "Stop";
                //if (checkBox_Polina.CheckState == CheckState.Checked)
                //{
                //notifyIcon1.Visible = false;
                //button_StartStop.Visible = false;
                //this.Enabled = false;
                DisableControls(this);
                //}
                notifyIcon1.Icon = WinFormsApp1.Properties.Resources.Icon1;
                Add_textBox_Log("NAPOMINATOR startTimer() - start");
                if (checkBox_Polina.CheckState == CheckState.Checked)
                    if (GetDoubleFromSettings("[Enabled]") == 1)
                        Start_NoiseDetector();

                Hide();
            } else {
                Add_textBox_Log("Stopped");
                timer1.Stop();
                button_StartStop.Text = "Start";
                notifyIcon1.Visible = true;
                Add_textBox_Log("NAPOMINATOR startTimer() - stop");
                Show();
            }
            Add_textBox_Log("startTimer()-exit from func");
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
        /// <summary>
        /// <summary>
        /// ƒÓ·‡‚ÎˇÂÚ Á‡ÔËÒ¸ ‚ ÎÓ„ ÓÍÌÓ Ë ÂÂÒÚ.
        /// </summary>
        /// <param name="_mode"> ÂÊËÏ˚ _mode = Information, Warning, Error</param>
        /// </summary>
        void Add_textBox_Log(string _text, bool _writeToLogFileOrRegistry = true, EventLogEntryType _mode = EventLogEntryType.Information)
        {
            string timeStr = DateTime.Now.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToLongTimeString() + ": ";

            //string filename = "";
            //if (_mode == "") filename = "log-" + USERNAME + ".txt";
            //if (_mode == "logblock") filename = "logblock-" + USERNAME + ".txt";

            if (_writeToLogFileOrRegistry)
            {
                WriteToRegistryLog(_text, _mode, USERNAME);
                /* NO WRITE TO LOG FILE
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

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
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

        List<string> settingsLines = new List<string>();
        List<string> settingsNotifyLines = new List<string>();
        void ReadSettingFile(string settingFileName = "Settings.txt")
        {
            try {
                bool startFound = false;
                bool startNotifyFound = false;
                if (File.Exists(settingFileName))
                {
                    settingsLines = new List<string>();
                    settingsNotifyLines = new List<string>();
                    string[] lines = File.ReadAllLines(settingFileName); 
                    foreach (string line in lines)
                    {
                        if (line.Contains("[USER][ANYCOMPUTER]["+USERNAME+"][START]"))
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

                        if ( startFound == true && startNotifyFound == false)
                            settingsLines.Add(line.TrimStart().TrimEnd());
                        if (startNotifyFound == true)
                            settingsNotifyLines.Add(line.TrimStart().TrimEnd());
                    }
                }
            } catch { 
                //Ï‡ÎÓ ÎË Ù‡ÈÎ ·Û‰ÂÚ ÓÚÍ˚Ú ËÎË ËÁÏÂÌÂÌ ‚ Ó‰ËÌ Ë ÚÓÚ ÊÂ ÏÓÏÂÌÚ.
            }
            return;
        }
        DateTime GetDateTimeFromSettings(string _settingName)// [allowed time from] [allowed to time]
        {
            DateTime ret = DateTime.MinValue;

            foreach (string line in (List<string>) settingsLines)
            {
                if (line.Contains(_settingName))
                {
                    ret = DateTime.Parse(line.Replace(_settingName, ""));
                    break;
                }
            }
            return ret;
        }
        string GetStringFromSettings(string _settingName)
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
        double GetDoubleFromSettings(string _settingName)
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



        string Parse_NotifyText()
        {
            string[] text = settingsNotifyLines.ToArray();
            string ret="";
                string[] arr = text;
                int curLine = 0;
                int curWeekOfMth;
                if (DateTime.Now.Day < 15) //Í‡ÚÌÓÒÚ¸ 2 ÌÂ‰ÂÎË
                {
                    double tmp = (double)DateTime.Now.Day / 7;
                    if (tmp <= 1)
                        curWeekOfMth = 1;
                    else
                        curWeekOfMth = 2;
                }
                else
                {
                    double tmp = (double)DateTime.Now.Day / 2 / 7;
                    if (tmp <= 1)
                        curWeekOfMth = 1;
                    else
                        curWeekOfMth = 2;
                }
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

        string prev_curWinTitle = "";
        DateTime lastExec_Tick=DateTime.MinValue;
        DateTime lastReadSettingsFile = DateTime.MinValue;
        bool executing_Tick = false;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            string curWinTitle = GetActiveWindowTitle();
            curWinTitle = curWinTitle.ToLower();
            if (prev_curWinTitle != curWinTitle)
                Add_textBox_Log("curWindowsTitle=" + curWinTitle, true);
            prev_curWinTitle = curWinTitle;


            if (DateTime.Now > lastExec_Tick.AddSeconds(Int32.Parse(textBox_Period.Text)))
                lastExec_Tick = DateTime.Now;
            else
                return;

            if (executing_Tick == false)
                executing_Tick = true;
            else
                return;

            if (DateTime.Now > lastReadSettingsFile.AddSeconds(59))
            {
                lastReadSettingsFile = DateTime.Now;
                ReadSettingFile();
            }


            textBox_NotifyText.Text = Parse_NotifyText();

            DateTime dt_from, dt_to;
            DateTime dtFFile, dtTFile;
            dtFFile = GetDateTimeFromSettings("[allowed time from]");
            dtTFile = GetDateTimeFromSettings("[allowed time to]");

            dt_from = DateTime.Now;
            dt_from = dt_from.AddHours(-DateTime.Now.Hour);
            dt_from = dt_from.AddHours(dtFFile.Hour);
            dt_from = dt_from.AddMinutes(-DateTime.Now.Minute);
            dt_from = dt_from.AddMinutes(dtFFile.Minute);

            dt_to = DateTime.Now;
            dt_to = dt_to.AddHours(-DateTime.Now.Hour);
            dt_to = dt_to.AddHours(dtTFile.Hour);
            dt_to = dt_to.AddMinutes(-DateTime.Now.Minute);
            dt_to = dt_to.AddMinutes(dtTFile.Minute);

            //checkBox_Polina.CheckState = CheckState.Checked;//TODO: DEBUG
            if (checkBox_Polina.CheckState == CheckState.Checked)
            {//‰Îˇ œÓÎËÌ˚
                if (audioSource == null)
                {
                    //Start_NoiseDetector_executing = false;
                    //messageShown = false;                    
                    Start_NoiseDetector();
                }
                var timeAllowed = ((dt_from < dt_to) && (DateTime.Now > dt_from && DateTime.Now < dt_to));
                if (!timeAllowed)
                    timeAllowed = ((dt_from > dt_to) && (DateTime.Now > dt_to));
                if (timeAllowed)
                {//‡ÁÂ¯ÂÌÌÓÂ ‚ÂÏˇ
                    if( GetDoubleFromSettings("[BlockChrome]")==1 )
                        if (curWinTitle.Contains("chrome") || curWinTitle.Contains("edge") || curWinTitle.Contains("firefox"))
                        {
                            bool foundany = curWinTitle == "" || CheckStringContainsInList(curWinTitle, GetStringFromSettings("[ExcludeFromBlock]"));
                            if (!foundany)
                                Show_Message_To_Polina("BlockChrome" + Parse_NotifyText(), "Õ¿œŒÃ»Õ¿“Œ–! BlockChrome " + curWinTitle);
                        }

                    if ( GetDoubleFromSettings("[BlockTotal]")==1 )
                        Show_Message_To_Polina("TotalBlock" + Parse_NotifyText(), "Õ¿œŒÃ»Õ¿“Œ–! TotalBlock " + curWinTitle);
                }
                else
                {//ÌÂ ‡ÁÂ¯ÂÌÌÓÂ ‚ÂÏˇ
                    bool foundany = curWinTitle == "" || CheckStringContainsInList(curWinTitle, GetStringFromSettings("[ExcludeFromBlock]"));
                    if (!foundany)
                        Show_Message_To_Polina("Allowed time from " + dt_from.ToShortTimeString() + " to " + dt_to.ToShortTimeString() + Parse_NotifyText(), "Õ¿œŒÃ»Õ¿“Œ–! Allowed time " + curWinTitle);
                }
                //·ÎÓÍ ÔÓ ÒÔËÒÍÛ
                if( CheckStringContainsInList(curWinTitle, GetStringFromSettings("[Blocklist]")) )
                    Show_Message_To_Polina("BlockList", "Õ¿œŒÃ»Õ¿“Œ–! BlockList " + curWinTitle);
            }
            else
            {// MAMA and PAPA
             //·ÎÓÍ ÔÓ ÒÔËÒÍÛ
                if (checkBox_Mama.Checked == true)
                {
                    if (CheckStringContainsInList(curWinTitle, GetStringFromSettings("[Blocklist]")))
                        Show_Message_To_Polina("BlockList" + Parse_NotifyText(), "Õ¿œŒÃ»Õ¿“Œ–! BlockList " + curWinTitle);

                    if (DateTime.Now.Hour == 04 && DateTime.Now.Minute == 44)
                    {
                        LockWorkStation();
                        Add_textBox_Log("LockWorkStation by time " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "", true, EventLogEntryType.Warning);
                    }
                }
                if (checkBox_Papa.Checked == true || checkBox_Mama.Checked == true)
                {
                    Show_Message_To_Polina(Parse_NotifyText(), "Õ¿œŒÃ»Õ¿“Œ–!", false, true);
                }
            }
            executing_Tick = false;
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
        void Show_Message_To_Polina(string _messageToShow, string _formCaption, Boolean _showDesktop = true, Boolean _dontCloseWindow = false, bool _write_textBox_Log = true, int _notifyLenghCounter = 5)
        {
            Message_To_Polina Message_To_Polina = new Message_To_Polina();
            Message_To_Polina.Set_NotifyText(_messageToShow);
            Message_To_Polina.Set_counter(_notifyLenghCounter);
            Message_To_Polina.Set_FormCaption(_formCaption);
            Message_To_Polina.Set_ShowDesktop(_showDesktop);
            Message_To_Polina.Set_DontCloseWindow(_dontCloseWindow);
            Message_To_Polina.ShowDialog();
            Message_To_Polina.Focus();
            if(_write_textBox_Log)
                Add_textBox_Log("Block by " + _messageToShow, true);
        }
        private void NotifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
        }
        private void CheckBox_Polina_CheckedChanged(object? sender, EventArgs e)
        {
            checkBox_Mama.CheckedChanged -= this.CheckBox_Mama_CheckedChanged;
            checkBox_Papa.CheckedChanged -= this.CheckBox_Papa_CheckedChanged;

            checkBox_Mama.CheckState = CheckState.Unchecked;
            checkBox_Papa.CheckState = CheckState.Unchecked;
            textBox_NotifyText.Text = "!!!";
            textBox_Period.Text = "5";

            checkBox_Mama.CheckedChanged += this.CheckBox_Mama_CheckedChanged;
            checkBox_Papa.CheckedChanged += this.CheckBox_Papa_CheckedChanged;
            USERNAME = "p";
        }
        private void CheckBox_Mama_CheckedChanged(object? sender, EventArgs e)
        {
            checkBox_Papa.CheckedChanged -= this.CheckBox_Papa_CheckedChanged;
            checkBox_Polina.CheckedChanged -= this.CheckBox_Polina_CheckedChanged;

            checkBox_Papa.CheckState = CheckState.Unchecked;
            checkBox_Polina.CheckState = CheckState.Unchecked;
            textBox_NotifyText.Text = Parse_NotifyText();
            textBox_Period.Text = (60 * 60).ToString();

            checkBox_Papa.CheckedChanged += this.CheckBox_Papa_CheckedChanged;
            checkBox_Polina.CheckedChanged += this.CheckBox_Polina_CheckedChanged;
            USERNAME = "i";
        }
        private void CheckBox_Papa_CheckedChanged(object? sender, EventArgs e)
        {
            checkBox_Polina.CheckedChanged -= this.CheckBox_Polina_CheckedChanged;
            checkBox_Mama.CheckedChanged -= this.CheckBox_Mama_CheckedChanged;

            checkBox_Polina.CheckState = CheckState.Unchecked;
            checkBox_Mama.CheckState = CheckState.Unchecked;
            textBox_NotifyText.Text = Parse_NotifyText();
            textBox_Period.Text = (60 * 60).ToString();

            checkBox_Polina.CheckedChanged += this.CheckBox_Polina_CheckedChanged;
            checkBox_Mama.CheckedChanged += this.CheckBox_Mama_CheckedChanged;
            USERNAME = "d";
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!allowFormClose)
            {
                Add_textBox_Log("NAPOMINATOR Form1_FormClosing() - Dont allowed.");
                e.Cancel = true;
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.X))
            {
                Add_textBox_Log("allowFormClose = true");
                allowFormClose = true;
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        string username2USERNAME(string _username) 
        {
            string ret="ERROR";

            if (_username.ToLower() == "r" || _username.ToLower().Contains("lobur"))
                ret = "r";
            if (_username.ToLower() == "d" || _username.ToLower().Contains("dementev_d"))
                ret = "d";
            if (_username.ToLower().Contains("user"))
                ret = "p";
            return ret;
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            USERNAME = username2USERNAME(System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1]);

            Add_textBox_Log("NAPOMINATOR version:"+__VERSION);
            ReadSettingFile();

            if (USERNAME == "r")
                checkBox_Mama.Checked = true;
            if (USERNAME == "d")
                checkBox_Papa.Checked = true;
            if (USERNAME == "p")
                checkBox_Polina.Checked = true;

            string tmpUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];
            if (!tmpUser.Contains("d") )
                StartTimer();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Icon = WinFormsApp1.Properties.Resources.Icon1;
                Hide();
            }
        }
    }
}