using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq.Expressions;

//using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace Napominator
{
    public partial class MainForm : Form
    {
        LogController logController;

        //====================================
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        //====================================
        [DllImport("user32.dll")]
        public static extern void LockWorkStation();
        //====================================
        public const int KEYEVENTF_EXTENTEDKEY = 1;
        public const int KEYEVENTF_KEYUP = 0;
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        //====================================


        const string __VERSION = "ver 02.10.2025";
        Boolean allowFormClose = false;
        string USERNAME = "";

        static WaveIn? audioSource;
        double soundLevel;
        double continuousBig;
        double shuminatorCount = 0, tot_shuminatorCount = 0;
        static bool messageShown = false;

        public MainForm()
        {
            InitializeComponent();
            GlobalMouseHook.OnMouseClick += CaptureScreenshotByMouseClick;
            GlobalMouseHook.Start();
        }




        MMDevice? device = null;
        const int constCountNoMicrophoneDetected = 5;
        int count_NoMicrophoneDetected = constCountNoMicrophoneDetected;
        bool Start_NoiseDetector_executing = false;
        public void Start_NoiseDetector()
        {
            if (GetStringFromSettings("[Shuminator_Enabled]") == "0")
                return;

            if (Start_NoiseDetector_executing == true)
                return;
            Start_NoiseDetector_executing = true;

            logController.Add_textBox_Log("s: Start_NoiseDetector()");

            try
            {
                //device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
                var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                foreach (var devitem in devices)
                {
                    logController.Add_textBox_Log("Microphone name:" + devitem.FriendlyName + " mic level" + devitem.AudioEndpointVolume.MasterVolumeLevelScalar);
                    if (device is null && devitem.FriendlyName.Contains(GetStringFromSettings("[Shuminator_MicrophoneName]")))
                        device = devitem;
                }
                if (device is null)
                    throw new Exception("No microphone detected.");

                logController.Add_textBox_Log("Microphone selected. Name:" + device.FriendlyName + " mic level" + device.AudioEndpointVolume.MasterVolumeLevelScalar);

                audioSource = new WaveIn();
                audioSource.DataAvailable += new EventHandler<WaveInEventArgs>(Microphone_DataAvailable);
                audioSource.RecordingStopped += new EventHandler<NAudio.Wave.StoppedEventArgs>(Microphone_RecordingStopped);
                audioSource.WaveFormat = new WaveFormat(8000, 2);
                audioSource.StartRecording();
            }
            catch
            {
                audioSource = null;
                logController.Add_textBox_Log("No microphone detected.");
                if (GetDoubleFromSettings("[LockWorkStation_NoMicrophone]") == 1)
                {
                    logController.Add_textBox_Log("Подключи микрофон!");
                    ShuminatorPlaySoundWarning("podkluchi_microfon.mp3", false);
                    Show_Message_To_Polina("ПОДКЛЮЧИ МИКРОФОН! \n СЧИТАЮ ДО 0-ля! " + count_NoMicrophoneDetected.ToString(), "ПОДКЛЮЧИ МИКРОФОН!", true, false, false, 5);
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
            logController.Add_textBox_Log("e: Start_NoiseDetector()");
        }


        bool microphone_DataAvailable_wait = false;
        DateTime microphone_DataAvailable_nextExec = DateTime.MinValue;
        void Microphone_DataAvailable(object? sender, WaveInEventArgs e)
        {
            if (device.AudioEndpointVolume.MasterVolumeLevelScalar < GetDoubleFromSettings("[LockWorkStation_Microphone_MasterVolumeLevelScalar]"))
            {
                this.Invoke((MethodInvoker)delegate { logController.Add_textBox_Log("SHUMINATOR LockWorkStation: MasterVolumeLevelScalar!", true, EventLogEntryType.Error); });
                LockWorkStation();
            }


            if (messageShown == true)
                return;
            if (microphone_DataAvailable_wait == true)
            {
                if (DateTime.Now > microphone_DataAvailable_nextExec)
                {
                    microphone_DataAvailable_wait = false;
                }
                else
                {
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
                this.Invoke((MethodInvoker)delegate
                {
                    logController.Add_textBox_Log("SHUMINATOR soundlevel up: " + continuousBig.ToString("N6"), true);
                    //notifyIcon1.BalloonTipText = "SHUMINATOR soundlevel up: " + continuousBig.ToString("N6");
                    //notifyIcon1.ShowBalloonTip(1000);
                });
            }

            double maxNoiseLevel = GetDoubleFromSettings("[ShuminatorPlaySoundWarning_MaxNoiseLevel]");
            string levelsStr = "Alert level: " + maxNoiseLevel.ToString("") + " cnt: " + shuminatorCount.ToString() + " total cnt: " + tot_shuminatorCount.ToString() + " maxDetected level: " + continuousBig.ToString("N6") + " current level: " + soundLevel.ToString("N6");
            tb_noiselevel.Invoke((MethodInvoker)delegate { tb_noiselevel.Text = levelsStr; tb_noiselevel.Update(); });

            if (continuousBig >= maxNoiseLevel)
            {
                logController.Add_textBox_Log("Microphone name:" + device.FriendlyName + " level:" + device.AudioEndpointVolume.MasterVolumeLevelScalar);
                shuminatorCount++;
                tot_shuminatorCount++;
                messageShown = true;

                this.Invoke((MethodInvoker)delegate
                {
                    logController.Add_textBox_Log("SHUMINATOR FIRED at soundLevel: " + levelsStr, true, EventLogEntryType.Warning);
                    logController.Add_textBox_Log("Microphone name:" + device.FriendlyName + " mic level" + device.AudioEndpointVolume.MasterVolumeLevelScalar, true, EventLogEntryType.Warning);
                });

                if (/*Get photo from cam*/1 == 1)
                {
                    this.Invoke((MethodInvoker)delegate { logController.Add_textBox_Log("s: taking photo from camera. Sound levels:" + levelsStr, true, EventLogEntryType.Warning); });
                    TakeScreenshotFromWEBCameraViaEmguCV();
                    this.Invoke((MethodInvoker)delegate { logController.Add_textBox_Log("e: taking photo from camera. Sound levels:" + levelsStr, true); });
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
                        Show_Message_To_Polina("НЕ ШУМИ!!! \n ДАЙ ПОСПАТЬ!!!", "НЕ ШУМИ!!! " + levelsStr, true, true, false, 15);
                    else
                        Show_Message_To_Polina("НЕ ШУМИ!!! \n ДАЙ ПОСПАТЬ!!!", "НЕ ШУМИ!!! " + levelsStr, true, false, false, 15);
                }
                if (GetDoubleFromSettings("[LockWorkStation]") == 1)
                {
                    this.Invoke((MethodInvoker)delegate { logController.Add_textBox_Log("LockWorkStation_shuminatorCount_check: ", true); });
                    if (shuminatorCount >= GetDoubleFromSettings("[LockWorkStation_shuminatorCount]"))
                    {
                        this.Invoke((MethodInvoker)delegate { logController.Add_textBox_Log("LockWorkStation_shuminatorCount_exec: ", true, EventLogEntryType.Warning); });
                        shuminatorCount = 0;
                        LockWorkStation();
                    }
                }
                if (GetDoubleFromSettings("[KillProcess]") == 1)
                {
                    this.Invoke((MethodInvoker)delegate { logController.Add_textBox_Log("KillProcess_check: " + GetStringFromSettings("[KillProcess_ProcessName]"), true); });

                    if (shuminatorCount >= GetDoubleFromSettings("[KillProcess_shuminatorCount]"))
                    {
                        this.Invoke((MethodInvoker)delegate { logController.Add_textBox_Log("KillProcess_exec: " + GetStringFromSettings("[KillProcess_ProcessName]"), true, EventLogEntryType.Warning); });
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
            }
            catch (Exception ex)
            {
                LogController.WriteToRegistryLog("ShuminatorPlaySoundWarning: " + ex.ToString(), EventLogEntryType.Error, "NO_USERNAME");
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
        public static void FillArray(double[,] array, double _fillValue = 0)
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
            this.Invoke((MethodInvoker)delegate { logController.Add_textBox_Log("microphone_RecordingStopped!!!!!!!!! ", true, EventLogEntryType.Error); });
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

                timer1.Interval = 1000;// * Int32.Parse(textBox_Period.Text) * 1;
                timer1.Start();
                button_StartStop.Text = "Stop";

                if (USERNAME != "d")
                    DisableControls(this);

                notifyIcon1.Icon = Napominator.Properties.Resources.Icon1;
                logController.Add_textBox_Log("NAPOMINATOR startTimer() - start");
                if (checkBox_Polina.CheckState == CheckState.Checked)
                    if (GetDoubleFromSettings("[Enabled]") == 1)
                        Start_NoiseDetector();

                Hide();
            }
            else
            {
                logController.Add_textBox_Log("Stopped");
                timer1.Stop();
                button_StartStop.Text = "Start";
                notifyIcon1.Visible = true;
                logController.Add_textBox_Log("NAPOMINATOR startTimer() - stop");
                Show();
            }
            logController.Add_textBox_Log("startTimer()-exit from func");
        }




        string prev_curWinTitle = "";
        DateTime lastExec_Tick = DateTime.MinValue;
        DateTime lastReadSettingsFile = DateTime.MinValue;
        bool executing_Tick = false;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            string curWinTitle = GetActiveWindowTitle();
            curWinTitle = curWinTitle.ToLower();
            if (prev_curWinTitle != curWinTitle)
                logController.Add_textBox_Log("curWindowsTitle=" + curWinTitle, true);
            prev_curWinTitle = curWinTitle;

            if (DateTime.Now > lastReadSettingsFile.AddSeconds(59))
            {   // reread setting file every 59 seconds
                lastReadSettingsFile = DateTime.Now;
                ReadSettingFile();
            }

            // периодичность проверки 
            if (DateTime.Now > lastExec_Tick.AddSeconds(Int32.Parse(textBox_Period.Text)))
                lastExec_Tick = DateTime.Now;
            else
                return;

            if (executing_Tick == false)
                executing_Tick = true;
            else
                return;


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
            {//для Полины
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
                {//разрешенное время
                    if (GetDoubleFromSettings("[BlockChrome]") == 1)
                        if (curWinTitle.Contains("chrome") || curWinTitle.Contains("edge") || curWinTitle.Contains("firefox"))
                        {
                            bool foundany = curWinTitle == "" || CheckStringContainsInList(curWinTitle, GetStringFromSettings("[ExcludeFromBlock]"));
                            if (!foundany)
                                Show_Message_To_Polina("BlockChrome" + Parse_NotifyText(), "НАПОМИНАТОР! BlockChrome " + curWinTitle);
                        }

                    if (GetDoubleFromSettings("[BlockTotal]") == 1)
                        Show_Message_To_Polina("TotalBlock" + Parse_NotifyText(), "НАПОМИНАТОР! TotalBlock " + curWinTitle);
                }
                else
                {//не разрешенное время
                    bool foundany = curWinTitle == "" || CheckStringContainsInList(curWinTitle, GetStringFromSettings("[ExcludeFromBlock]"));
                    if (!foundany)
                        Show_Message_To_Polina("Allowed time from " + dt_from.ToShortTimeString() + " to " + dt_to.ToShortTimeString() + Parse_NotifyText(), "НАПОМИНАТОР! Allowed time " + curWinTitle);
                }
                //блок по списку
                if (CheckStringContainsInList(curWinTitle, GetStringFromSettings("[Blocklist]")))
                    Show_Message_To_Polina("BlockList", "НАПОМИНАТОР! BlockList " + curWinTitle);
            }
            else
            {// MAMA and PAPA
             //блок по списку
                if (checkBox_Mama.Checked == true)
                {
                    if (GetDoubleFromSettings("[DisableNotify]") == 0)
                        if (CheckStringContainsInList(curWinTitle, GetStringFromSettings("[Blocklist]")))
                            Show_Message_To_Polina("BlockList" + Parse_NotifyText(), "НАПОМИНАТОР! BlockList " + curWinTitle);

                    if (DateTime.Now.Hour == 04 && DateTime.Now.Minute == 44)
                    {
                        LockWorkStation();
                        logController.Add_textBox_Log("LockWorkStation by time " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "", true, EventLogEntryType.Warning);
                    }
                }
                if (GetDoubleFromSettings("[DisableNotify]") == 0)
                    if (checkBox_Papa.Checked == true || checkBox_Mama.Checked == true)
                        Show_Message_To_Polina(Parse_NotifyText(), "НАПОМИНАТОР!", false, true);
            }
            executing_Tick = false;
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
            if (_write_textBox_Log)
                logController.Add_textBox_Log("Block by " + _messageToShow, true);
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
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!allowFormClose)
            {
                logController.Add_textBox_Log("NAPOMINATOR Form1_FormClosing() - Dont allowed.");
                e.Cancel = true;
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.X))
            {
                logController.Add_textBox_Log("allowFormClose = true");
                allowFormClose = true;
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            USERNAME = username2USERNAME(System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1]);
            //TODO: debug
            //USERNAME = "d";
            //TODO: debug


            logController = LogController.GetInstance(textBox_Log, USERNAME);

            logController.Add_textBox_Log("Started. initial version created at 29.12.2022 11:50");
            logController.Add_textBox_Log("NAPOMINATOR MainForm()");
            logController.Add_textBox_Log($"Exec FileName : {System.IO.Path.GetFileName(Application.ExecutablePath)}");


            logController.Add_textBox_Log("NAPOMINATOR version:" + __VERSION);
            logController.Add_textBox_Log("ReadSettingFile() executed", false);

            //HACK: Если нужно запустить под другим пользователем, то менять тут.
            //USERNAME = "i";

            ReadSettingFile();

            if (USERNAME == "i")
                checkBox_Mama.Checked = true;
            if (USERNAME == "d")
                checkBox_Papa.Checked = true;
            if (USERNAME == "p")
                checkBox_Polina.Checked = true;

            string tmpUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];
            if (!tmpUser.Contains("d"))
                StartTimer();


        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Icon = Napominator.Properties.Resources.Icon1;
                Hide();
            }
        }



        int personalTimes_secondsToStop;
        void Persona_StartTimer()
        {
            if (btnPersonalTimerStartStop.Text == "START")
            {
                try
                {
                    var table = new DataTable();
                    personalTimes_secondsToStop = (int)table.Compute(textBox_PersonalPeriod.Text, null);
                }
                catch
                {
                    textBox_PersonalPeriod.Text = "5";
                    return;
                }
                if (personalTimes_secondsToStop <= 1)
                {
                    textBox_PersonalPeriod.Text = "5";
                    return;
                }

                timer_Personal.Interval = 1000;
                timer_Personal.Start();
                btnPersonalTimerStartStop.Text = "STOP";
                logController.Add_textBox_Log($"Personal timer started for {personalTimes_secondsToStop} sec.");
            }
            else
            {
                logController.Add_textBox_Log("Personal timer stopped.");
                timer_Personal.Stop();
                btnPersonalTimerStartStop.Text = "START";
            }
        }
        private void btnPersonalTimerStartStop_Click(object sender, EventArgs e)
        {
            Persona_StartTimer();
        }

        private void timer_Personal_Tick(object sender, EventArgs e)
        {
            if (personalTimes_secondsToStop > 0)
            {
                personalTimes_secondsToStop--;
                btnPersonalTimerStartStop.Text = personalTimes_secondsToStop.ToString();
                return;
            }
            Persona_StartTimer();
            Show_Message_To_Polina( $"Message: {textBox_PersonalPeriodText.Text}.\n Прошедшее время в секундах: {textBox_PersonalPeriod.Text}", $"Personal timer DONE.", false, true);

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}