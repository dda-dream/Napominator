using Microsoft.Extensions.DependencyInjection;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.Grafana.Loki.HttpClients;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime;
using static Emgu.CV.VideoCapture;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Napominator;



public partial class MainForm : Form
{
    LogMediator logMediator;
    LogController logController;
    Sound sound;
    Functions functions;
    GlobalMouseHook globalMouseHook;

    const string __VERSION = "ver 27.03.2026";
    Boolean allowFormClose = false;

    static WaveIn? audioSource;
    double soundLevel;
    double continuousBig;
    double shuminatorCount = 0, tot_shuminatorCount = 0;
    static bool messageShown = false;

    TaskScheduler t1;
    SynchronizationContext t2;
    WindowsFormsSynchronizationContext t3;
    Task t4;

    public MainForm()
    {
        InitializeComponent();

        string USERNAME = Functions.username2USERNAME(System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1]);
        ArgumentException.ThrowIfNullOrEmpty(USERNAME);
        //TODO: debug
        //HACK: Если нужно запустить под другим пользователем, то менять тут.
        //USERNAME = "p";
        //TODO: debug

        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ip = host.AddressList.FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork);
        string ip_last_digit = ip.ToString().Split(".")[3];

        logController = LogController.Builder();
        logMediator = new LogMediator();
        logController.AddUSERNAME(USERNAME);
        logController.AddTextBoxLogger(logMediator);
        logController.AddEventViewerLogger();
        logController.AddFileLogger();
        logController.AddSerilogLoki(ip_last_digit);
        logMediator.subscriber += LogMediator_subscriber;

        functions = new Functions(logController, USERNAME, ip_last_digit);

        sound = new Sound();

        globalMouseHook = new GlobalMouseHook();
        globalMouseHook.OnMouseClick += functions.CaptureScreenshotByMouseClick;
        globalMouseHook.Start();

    }

    MMDevice? device = null;
    const int constCountNoMicrophoneDetected = 5;
    int count_NoMicrophoneDetected = constCountNoMicrophoneDetected;
    bool Start_NoiseDetector_executing = false;
    public void Start_NoiseDetector()
    {
        if (functions.GetStringFromSettings("[Shuminator_Enabled]") == "0")
            return;

        if (Start_NoiseDetector_executing == true)
            return;
        Start_NoiseDetector_executing = true;

        logController.Log("s: Start_NoiseDetector()");

        try
        {
            //device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
            var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            foreach (var devitem in devices)
            {
                logController.Log("Microphone name:" + devitem.FriendlyName + " mic level" + devitem.AudioEndpointVolume.MasterVolumeLevelScalar);
                if (device is null && devitem.FriendlyName.Contains(functions.GetStringFromSettings("[Shuminator_MicrophoneName]")))
                    device = devitem;
            }
            if (device is null)
                throw new Exception("No microphone detected.");

            logController.Log("Microphone selected. Name:" + device.FriendlyName + " mic level" + device.AudioEndpointVolume.MasterVolumeLevelScalar);

            audioSource = new WaveIn();
            audioSource.DataAvailable += new EventHandler<WaveInEventArgs>(Microphone_DataAvailable);
            audioSource.RecordingStopped += new EventHandler<NAudio.Wave.StoppedEventArgs>(Microphone_RecordingStopped);
            audioSource.WaveFormat = new WaveFormat(8000, 2);
            audioSource.StartRecording();
        }
        catch
        {
            audioSource = null;
            logController.Log("No microphone detected.");
            if (functions.GetDoubleFromSettings("[LockWorkStation_NoMicrophone]") == 1)
            {
                logController.Log("Подключи микрофон!");
                ShuminatorPlaySoundWarning("podkluchi_microfon.mp3", false, logController);
                Show_Message_To_Polina("ПОДКЛЮЧИ МИКРОФОН! \n СЧИТАЮ ДО 0-ля! " + count_NoMicrophoneDetected.ToString(), "ПОДКЛЮЧИ МИКРОФОН!", true, false, false, 5);
                count_NoMicrophoneDetected--;
                if (count_NoMicrophoneDetected == 0)
                {
                    count_NoMicrophoneDetected = constCountNoMicrophoneDetected;
                    functions.KillProcess(functions.GetStringFromSettings("[KillProcess_ProcessName]"), true);
                    Functions.LockWorkStation();
                }
            }
        }
        Start_NoiseDetector_executing = false;
        logController.Log("e: Start_NoiseDetector()");
    }


    bool microphone_DataAvailable_wait = false;
    DateTime microphone_DataAvailable_nextExec = DateTime.MinValue;
    void Microphone_DataAvailable(object? sender, WaveInEventArgs e)
    {
        if (device.AudioEndpointVolume.MasterVolumeLevelScalar < functions.GetDoubleFromSettings("[LockWorkStation_Microphone_MasterVolumeLevelScalar]"))
        {
            this.Invoke((MethodInvoker)delegate { logController.Log("SHUMINATOR LockWorkStation: MasterVolumeLevelScalar!", EventLogEntryType.Error); });
            Functions.LockWorkStation();
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
                logController.Log("SHUMINATOR soundlevel up: " + continuousBig.ToString("N6"), EventLogEntryType.Information);
                //notifyIcon1.BalloonTipText = "SHUMINATOR soundlevel up: " + continuousBig.ToString("N6");
                //notifyIcon1.ShowBalloonTip(1000);
            });
        }

        double maxNoiseLevel = functions.GetDoubleFromSettings("[ShuminatorPlaySoundWarning_MaxNoiseLevel]");
        string levelsStr = "Alert level: " + maxNoiseLevel.ToString("") + " cnt: " + shuminatorCount.ToString() + " total cnt: " + tot_shuminatorCount.ToString() + " maxDetected level: " + continuousBig.ToString("N6") + " current level: " + soundLevel.ToString("N6");
        tb_noiselevel.Invoke((MethodInvoker)delegate { tb_noiselevel.Text = levelsStr; tb_noiselevel.Update(); });

        if (continuousBig >= maxNoiseLevel)
        {
            logController.Log("Microphone name:" + device.FriendlyName + " level:" + device.AudioEndpointVolume.MasterVolumeLevelScalar);
            shuminatorCount++;
            tot_shuminatorCount++;
            messageShown = true;

            this.Invoke((MethodInvoker)delegate
            {
                logController.Log("SHUMINATOR FIRED at soundLevel: " + levelsStr, EventLogEntryType.Warning);
                logController.Log("Microphone name:" + device.FriendlyName + " mic level" + device.AudioEndpointVolume.MasterVolumeLevelScalar, EventLogEntryType.Warning);
            });

            if (/*Get photo from cam*/1 == 1)
            {
                this.Invoke((MethodInvoker)delegate { logController.Log("s: taking photo from camera. Sound levels:" + levelsStr, EventLogEntryType.Warning); });
                functions.TakeScreenshotFromWEBCameraViaEmguCV();
                this.Invoke((MethodInvoker)delegate { logController.Log("e: taking photo from camera. Sound levels:" + levelsStr, EventLogEntryType.Information); });
                //Get photo from cam
            }
            if (functions.GetDoubleFromSettings("[ShuminatorPlaySoundWarning]") == 1)
            {
                Functions.keybd_event(Functions.VK_MEDIA_PLAY_PAUSE, 0, Functions.KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);    // Play/Pause
                ShuminatorPlaySoundWarning("neshumi.mpeg", true, logController);
            }
            if (functions.GetDoubleFromSettings("[Show_Message_To_Polina]") == 1)
            {
                if (functions.USERNAME.Contains("d"))
                    Show_Message_To_Polina("НЕ ШУМИ!!! \n ДАЙ ПОСПАТЬ!!!", "НЕ ШУМИ!!! " + levelsStr, true, true, false, 15);
                else
                    Show_Message_To_Polina("НЕ ШУМИ!!! \n ДАЙ ПОСПАТЬ!!!", "НЕ ШУМИ!!! " + levelsStr, true, false, false, 15);
            }
            if (functions.GetDoubleFromSettings("[LockWorkStation]") == 1)
            {
                this.Invoke((MethodInvoker)delegate { logController.Log("LockWorkStation_shuminatorCount_check: ", EventLogEntryType.Information); });
                if (shuminatorCount >= functions.GetDoubleFromSettings("[LockWorkStation_shuminatorCount]"))
                {
                    this.Invoke((MethodInvoker)delegate { logController.Log("LockWorkStation_shuminatorCount_exec: ", EventLogEntryType.Warning); });
                    shuminatorCount = 0;
                    Functions.LockWorkStation();
                }
            }
            if (functions.GetDoubleFromSettings("[KillProcess]") == 1)
            {
                this.Invoke((MethodInvoker)delegate { logController.Log("KillProcess_check: " + functions.GetStringFromSettings("[KillProcess_ProcessName]"), EventLogEntryType.Information); });

                if (shuminatorCount >= functions.GetDoubleFromSettings("[KillProcess_shuminatorCount]"))
                {
                    this.Invoke((MethodInvoker)delegate { logController.Log("KillProcess_exec: " + functions.GetStringFromSettings("[KillProcess_ProcessName]"), EventLogEntryType.Warning); });
                    functions.KillProcess(functions.GetStringFromSettings("[KillProcess_ProcessName]"), true);
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
    static void ShuminatorPlaySoundWarning(string _warningSound, bool _playOtherSounds = true, LogController logController = null)
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
            if (logController != null)
                logController.Log("ShuminatorPlaySoundWarning: " + ex.ToString(), EventLogEntryType.Error);
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
        Functions.keybd_event(Functions.VK_MEDIA_PLAY_PAUSE, 0, Functions.KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);    // Play/Pause
    }
    void Microphone_RecordingStopped(object? sender, NAudio.Wave.StoppedEventArgs e)
    {
        this.Invoke((MethodInvoker)delegate { logController.Log("microphone_RecordingStopped!!!!!!!!! ", EventLogEntryType.Error); });
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

            timer1.Interval = 1000;
            timer1.Start();
            button_StartStop.Text = "Stop";

            if (functions.USERNAME != "d" && functions.USERNAME != "i")
                functions.DisableControls(this);

            notifyIcon1.Icon = Napominator.Properties.Resources.Icon1;
            logController.Log("NAPOMINATOR startTimer() - start");
            if (checkBox_Polina.CheckState == CheckState.Checked)
                if (functions.GetDoubleFromSettings("[Shuminator_Enabled]") == 1)
                    Start_NoiseDetector();

            Hide();
        }
        else
        {
            logController.Log("Stopped");
            timer1.Stop();
            button_StartStop.Text = "Start";
            notifyIcon1.Visible = true;
            logController.Log("NAPOMINATOR startTimer() - stop");
            Show();
        }
        logController.Log("startTimer()-exit from func");
    }


    string prev_curWinTitle = "";
    DateTime lastExec_Tick = DateTime.Now;
    DateTime lastReadSettingsFile = DateTime.MinValue;
    bool executing_Tick = false;
    private async void Timer1_Tick(object sender, EventArgs e)
    {
        int CHECK_PERIOD_SECONDS = 5;

        CHECK_PERIOD_SECONDS = (int)functions.GetDoubleFromSettings("[CHECK_PERIOD_SECONDS]");
        try
        {
            string curWinTitle = functions.GetActiveWindowTitle();
            curWinTitle = curWinTitle.ToLower();
            if (prev_curWinTitle != curWinTitle)
                logController.Log("curWindowsTitle=" + curWinTitle, EventLogEntryType.Information);

            prev_curWinTitle = curWinTitle;

            if (DateTime.Now > lastReadSettingsFile.AddSeconds(59))
            {   // reread setting file every 59 seconds
                lastReadSettingsFile = DateTime.Now;
                var lines = await functions.ReadSettingFile();
                functions.ParseSettingFile(lines);

                RefreshControlsFromSettings();
            }

            // периодичность проверки 
            if (DateTime.Now > lastExec_Tick.AddSeconds(CHECK_PERIOD_SECONDS))
                lastExec_Tick = DateTime.Now;
            else
                return;

            if (executing_Tick == false)
                executing_Tick = true;
            else
                return;


            DateTime dt_from, dt_to;
            DateTime dtFFile, dtTFile;
            dtFFile = functions.GetDateTimeFromSettings("[allowed time from]");
            dtTFile = functions.GetDateTimeFromSettings("[allowed time to]");

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
                if (audioSource == null
                    && functions.GetStringFromSettings("[Shuminator_Enabled]") == "1")
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
                    if (functions.GetDoubleFromSettings("[BlockChrome]") == 1)
                        if (curWinTitle.Contains("chrome") || curWinTitle.Contains("edge") || curWinTitle.Contains("firefox"))
                        {
                            bool foundany = curWinTitle == "" || functions.CheckStringContainsInList(curWinTitle, functions.GetStringFromSettings("[ExcludeFromBlock]"));
                            if (!foundany)
                                Show_Message_To_Polina("BlockChrome" + functions.Parse_NotifyText(), "НАПОМИНАТОР! BlockChrome " + curWinTitle);
                        }

                    if (functions.GetDoubleFromSettings("[BlockTotal]") == 1)
                        Show_Message_To_Polina("TotalBlock" + functions.Parse_NotifyText(), "НАПОМИНАТОР! TotalBlock " + curWinTitle);
                }
                else
                {//не разрешенное время
                    bool foundany = curWinTitle == "" || functions.CheckStringContainsInList(curWinTitle, functions.GetStringFromSettings("[ExcludeFromBlock]"));
                    if (!foundany)
                        Show_Message_To_Polina("Allowed time from " + dt_from.ToShortTimeString() + " to " + dt_to.ToShortTimeString() + functions.Parse_NotifyText(), "НАПОМИНАТОР! Allowed time " + curWinTitle);
                }
                //блок по списку
                if (functions.CheckStringContainsInList(curWinTitle, functions.GetStringFromSettings("[Blocklist]")))
                    Show_Message_To_Polina("BlockList", "НАПОМИНАТОР! BlockList " + curWinTitle);
            }
            else
            {// MAMA and PAPA
             //блок по списку
                if (checkBox_Mama.Checked == true)
                {
                    if (functions.GetDoubleFromSettings("[DisableNotify]") == 0)
                        if (functions.CheckStringContainsInList(curWinTitle, functions.GetStringFromSettings("[Blocklist]")))
                            Show_Message_To_Polina("BlockList" + functions.Parse_NotifyText(), "НАПОМИНАТОР! BlockList " + curWinTitle);

                    if (functions.GetDoubleFromSettings("[EnableLockAt0444]") == 1)
                        if (DateTime.Now.Hour == 04 && DateTime.Now.Minute == 44)
                        {
                            Functions.LockWorkStation();
                            logController.Log("LockWorkStation by time " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "", EventLogEntryType.Warning);
                        }

                }
                if (functions.GetDoubleFromSettings("[DisableNotify]") == 0)
                    if( ! string.IsNullOrEmpty(textBox_NotifyText.Text) )
                        if (checkBox_Papa.Checked == true || checkBox_Mama.Checked == true)
                            Show_Message_To_Polina(functions.Parse_NotifyText(), "НАПОМИНАТОР!", false, true);
            }
            executing_Tick = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Err");
        }
    }


    void RefreshControlsFromSettings()
    {
        textBox_NotifyText.Text = functions.Parse_NotifyText();
        dateTime_from.Text = functions.GetStringFromSettings("[allowed time from]");
        dateTime_to.Text = functions.GetStringFromSettings("[allowed time to]");
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
            logController.Log("Block by " + _messageToShow, EventLogEntryType.Information);
    }
    private void NotifyIcon1_DoubleClick(object sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
            Functions.ShowWindow(this.Handle, Functions.SW_RESTORE);

        this.Show();

        this.Activate();
        Functions.SetForegroundWindow(this.Handle);
    }
    private void CheckBox_Polina_CheckedChanged(object? sender, EventArgs e)
    {
        checkBox_Mama.CheckedChanged -= this.CheckBox_Mama_CheckedChanged;
        checkBox_Papa.CheckedChanged -= this.CheckBox_Papa_CheckedChanged;

        checkBox_Mama.CheckState = CheckState.Unchecked;
        checkBox_Papa.CheckState = CheckState.Unchecked;
        textBox_NotifyText.Text = "!!!";

        checkBox_Mama.CheckedChanged += this.CheckBox_Mama_CheckedChanged;
        checkBox_Papa.CheckedChanged += this.CheckBox_Papa_CheckedChanged;
    }
    private void CheckBox_Mama_CheckedChanged(object? sender, EventArgs e)
    {
        checkBox_Papa.CheckedChanged -= this.CheckBox_Papa_CheckedChanged;
        checkBox_Polina.CheckedChanged -= this.CheckBox_Polina_CheckedChanged;

        checkBox_Papa.CheckState = CheckState.Unchecked;
        checkBox_Polina.CheckState = CheckState.Unchecked;

        checkBox_Papa.CheckedChanged += this.CheckBox_Papa_CheckedChanged;
        checkBox_Polina.CheckedChanged += this.CheckBox_Polina_CheckedChanged;
    }
    private void CheckBox_Papa_CheckedChanged(object? sender, EventArgs e)
    {
        checkBox_Polina.CheckedChanged -= this.CheckBox_Polina_CheckedChanged;
        checkBox_Mama.CheckedChanged -= this.CheckBox_Mama_CheckedChanged;

        checkBox_Polina.CheckState = CheckState.Unchecked;
        checkBox_Mama.CheckState = CheckState.Unchecked;

        checkBox_Polina.CheckedChanged += this.CheckBox_Polina_CheckedChanged;
        checkBox_Mama.CheckedChanged += this.CheckBox_Mama_CheckedChanged;
    }
    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!allowFormClose)
        {
            logController.Log("NAPOMINATOR Form1_FormClosing() - Dont allowed.");
            e.Cancel = true;
        }
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.X))
        {
            logController.Log("allowFormClose = true");
            allowFormClose = true;
            this.Close();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void Form1_Load(object sender, EventArgs e)
    {
    }

    private void LogMediator_subscriber(object? sender, LogEventArgs e)
    {
        if (e.Message == "***CLEAR***LOG***CONTROL***")
            Clear_TextBox_Log();
        else
            Add_TextBox_Log(e.Message, e.level, e.USERNAME);
    }

    private void Clear_TextBox_Log()
    {
        textBox_Log.Text = "";
    }
    private void Add_TextBox_Log(string message, EventLogEntryType level, string USERNAME)
    {
        const int MAX_LINES = 100;
        if (textBox_Log.Lines.Length > MAX_LINES)
            textBox_Log.Text = "";

        string timeStr = DateTime.Now.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToLongTimeString() + ": ";
        string logEntry;

        if (String.IsNullOrEmpty(message))
            logEntry = Environment.NewLine;
        else
            logEntry = timeStr + message + Environment.NewLine;

        textBox_Log.AppendText(logEntry);
        textBox_Log.SelectionStart = textBox_Log.Text.Length;
        textBox_Log.SelectionLength = 0;
        textBox_Log.ScrollToCaret();
    }


    private async void Form1_Shown(object sender, EventArgs e)
    {
        logController.Log("Started. initial version created at 29.12.2022 11:50");
        logController.Log("NAPOMINATOR");
        logController.Log($"Exec FileName : {System.IO.Path.GetFileName(Application.ExecutablePath)}");
        logController.Log("NAPOMINATOR version:" + __VERSION);

        var lines = await functions.ReadSettingFile();
        functions.ParseSettingFile(lines);
        RefreshControlsFromSettings();


        if (functions.USERNAME == "i")
            checkBox_Mama.Checked = true;
        if (functions.USERNAME == "d")
            checkBox_Papa.Checked = true;
        if (functions.USERNAME == "p")
            checkBox_Polina.Checked = true;

        if (functions.USERNAME != "d")
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
    void PersonalTime_StartTimer()
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
            logController.Log($"Personal timer started for {personalTimes_secondsToStop} sec.");
        }
        else
        {
            logController.Log("Personal timer stopped.");
            timer_Personal.Stop();
            btnPersonalTimerStartStop.Text = "START";
            notifyIcon1.Text = ".Napominator.";
            this.Text = ".Napominator.";
        }
    }
    private void btnPersonalTimerStartStop_Click(object sender, EventArgs e)
    {
        PersonalTime_StartTimer();
    }

    private void timer_Personal_Tick(object sender, EventArgs e)
    {
        if (personalTimes_secondsToStop > 0)
        {
            personalTimes_secondsToStop--;
            btnPersonalTimerStartStop.Text = personalTimes_secondsToStop.ToString();
            string _ = $"Осталось: в секундах {personalTimes_secondsToStop.ToString()} или в минутах: {((int)(personalTimes_secondsToStop / 60)).ToString()}";
            notifyIcon1.Text = _;
            return;
        }
        PersonalTime_StartTimer();
        Show_Message_To_Polina($"Message: {textBox_PersonalPeriodText.Text}.\n Прошедшее время в секундах: {textBox_PersonalPeriod.Text}", $"Personal timer DONE.", false, true);

    }





    private async void btn_IPInfo_Click(object sender, EventArgs e)
    {
        int timeout;
        int.TryParse(rtb_httpTimeout.Text, out timeout);
        await PingTestProxy(cb_UsePing.Checked, timeout);
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        globalMouseHook.Dispose();
        logController.Dispose();

    }


    private async void IpInfo_timer_Tick(object sender, EventArgs e)
    {
        int seconds;
        if (int.TryParse(rtb_IpInfoSeconds.Text, out seconds) == false)
            seconds = 0;

        IpInfo_timer.Stop();
        int timeout;
        int.TryParse(rtb_httpTimeout.Text, out timeout);
        await PingTestProxy(cb_UsePing.Checked, timeout);

        if (seconds > 0)
            IpInfo_timer.Start();
    }

    private async Task PingTestProxy(bool usePing, int httpTimeout)
    {
        string proxy = functions.GetStringFromSettings("[Proxy_IP]");

        var ipInfo = Program._serviceProvider.GetRequiredService<IpInfo>();
        using (ipInfo)
        {
            if (!String.IsNullOrEmpty(proxy))
                if (rtbProxyUrl.Text == "")
                    rtbProxyUrl.Text = proxy;

            if (rtbProxyUrl.Text != "")
                ipInfo.Proxy = rtbProxyUrl.Text;

            var settings = Program._serviceProvider.GetRequiredService<IConfigService>();
            if (httpTimeout == 0)
                httpTimeout = settings.NetworkConfig.HttpClientTimeoutSeconds;

            ipInfo.Create(usePing, httpTimeout);
            if (rtbProxyUrl.Text == "")
                rtbProxyUrl.Text = ipInfo.Proxy;

            var (ipInfoDTO, pingResult) = await ipInfo.Process();

            string pingResultStr = "";
            if (usePing)
                if (pingResult.FirstOrDefault(k => k.Key == "Status").Value == IPStatus.Success.ToString())
                    pingResultStr = $" Ping - {pingResult.FirstOrDefault(k => k.Key == "RoundtripTime").Value} мс.";
                else
                    pingResultStr = $" Ping error. Status: {pingResult.FirstOrDefault(k => k.Key == "Status").Value}.";

            string http1_status = $"ip-api: {ipInfoDTO.http_response_status1}.";
            string http2_status = $"rutor: {ipInfoDTO.http_response_status2}.";
            string http3_status = $"youtube: {ipInfoDTO.http_response_status3}.";

            Add_TextBox_Log($"{ipInfoDTO.query} {ipInfoDTO.countryCode} {httpTimeout} {http1_status} {http2_status} {http3_status} {pingResultStr}", EventLogEntryType.Information, "");
        }
    }

    private void rtb_IpInfoSeconds_TextChanged(object sender, EventArgs e)
    {
        int seconds;
        if (int.TryParse(rtb_IpInfoSeconds.Text, out seconds) == false)
            seconds = 0;
         

        if (seconds <= 0)
        {
            IpInfo_timer.Stop();
            logController.Log("Stop IpInfo_timer.");
        } else {
            IpInfo_timer.Interval = 1000 * seconds;
            IpInfo_timer.Start();
            logController.Log("Start IpInfo_timer.");
        }

    }
}