using Microsoft.Extensions.DependencyInjection;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace Napominator;



public partial class MainForm : Form
{
    LogMediator logMediator;
    LogController logController;
    Sound sound;
    Functions functions;
    GlobalMouseHook globalMouseHook;
    IAppSettings settings;

    const string __VERSION = "ver 27.03.2026";
    Boolean allowFormClose = false;

    static bool messageShown = false;

    TaskScheduler t1;
    SynchronizationContext t2;
    WindowsFormsSynchronizationContext t3;
    Task t4;

    public MainForm()
    {
        InitializeComponent();
        settings = Program._serviceProvider.GetRequiredService<IAppSettings>();
        if (settings == null)
            settings = new AppSettings();

        string USERNAME = Functions.username2USERNAME(System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1]);
        ArgumentException.ThrowIfNullOrEmpty(USERNAME);
        //TODO: debug
        //HACK: Если нужно запустить под другим пользователем, то менять тут.
        //USERNAME = "p";
        //TODO: debug
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        IPAddress[] ips = host.AddressList;
        foreach( var i in ips)
        {
            var s = i.ToString();
        }

        IPAddress ip = ips.FirstOrDefault(ip => ip.ToString().StartsWith("192.168.2."));
        if (ip == null)
            ip = ips.FirstOrDefault(ip => ip.ToString().StartsWith("192.168.3."));
        if (ip == null)
            ip = ips.FirstOrDefault(ip => ip.ToString().StartsWith("192.168.5."));

        string ip_last_digit = "00";
        if (ip == null)
        {
            if (host.HostName.Contains("BMAX"))
                ip_last_digit = "44";
        }
        else
        {
            ip_last_digit = ip.ToString().Split(".")[3];
        }

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

        Add_TextBox_Log($"USERNAME: {USERNAME}", EventLogEntryType.Information, USERNAME);

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


    public async Task ShowForUnreadMessagesInChat()
    {
        var messages = await functions.CheckUnreadChatMessages();
        if (!string.IsNullOrEmpty(messages))
        {
            var unreadCounts = JsonSerializer.Deserialize<Dictionary<string, int>>(messages);
            if (unreadCounts != null)
            {
                unreadCounts.TryGetValue(settings.ChatConnectionConfig.CheckForUser, out int msgCount);

                if (msgCount > 0)
                {
                    Show_Message_To_Polina($"У вас есть {msgCount} непрочитанные сообщения в чате!",
                                           $"У вас есть {msgCount} непрочитанные сообщения в чате!",
                                            false, true, false,
                                            4 * 60/*4 минуты мигать и 1 минута до следующей проверки*/
                                            , 1);
                }
            }
        }
    }


    string prev_curWinTitle = "";
    DateTime lastExec_Tick = DateTime.Now;
    DateTime lastReadSettingsFile = DateTime.MinValue;
    DateTime lastCheckUnreadChatMessages = DateTime.MinValue;
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
                CancellationTokenSource cts = new CancellationTokenSource();
                var lines = await functions.ReadSettingFile(cts.Token);
                functions.ParseSettingFile(lines);

                RefreshControlsFromSettings();
            }

            if (DateTime.Now > lastCheckUnreadChatMessages.AddMinutes(5))
            {
                lastCheckUnreadChatMessages = DateTime.Now;

                await ShowForUnreadMessagesInChat();
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
                    if (!string.IsNullOrEmpty(textBox_NotifyText.Text))
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


    void Show_Message_To_Polina(string _messageToShow, string _formCaption, Boolean _showDesktop = true,
        Boolean _dontCloseWindow = false, bool _write_textBox_Log = true,
        int periodToCloseSeconds = 5, int timerPeriodSeconds = 0)
    {
        Message_To_Polina Message_To_Polina = new Message_To_Polina();
        Message_To_Polina.Set_TimerPeriod(timerPeriodSeconds);
        Message_To_Polina.Set_NotifyText(_messageToShow);
        Message_To_Polina.Set_counter(periodToCloseSeconds);
        Message_To_Polina.Set_FormCaption(_formCaption);
        Message_To_Polina.Set_ShowDesktop(_showDesktop);
        Message_To_Polina.Set_AllowCloseWindow(_dontCloseWindow);
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
        if (IsDisposed || Disposing || textBox_Log.IsDisposed || textBox_Log.Disposing)
            return;

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

        if (functions.USERNAME == "i")
            checkBox_Mama.Checked = true;
        if (functions.USERNAME == "d")
            checkBox_Papa.Checked = true;
        if (functions.USERNAME == "p")
            checkBox_Polina.Checked = true;

        if (functions.USERNAME != "d")
            StartTimer();

        InitMainThings();
    }


    CancellationTokenSource ctsReadRabbitMQ;
    async void InitMainThings()
    {
        logController.Log("InitMainThings()");
        ctsReadRabbitMQ = new CancellationTokenSource();
        ctsReadRabbitMQ.Token.Register(RabbitMQCancelled);
        var lines = await functions.ReadSettingFile(ctsReadRabbitMQ.Token);
        functions.ParseSettingFile(lines);
        RefreshControlsFromSettings();

        await ShowForUnreadMessagesInChat();
    }

    public void RabbitMQCancelled()
    {
        logController.Log("!!");
        
    }


    public Action<int> a()
    {
        return (i) =>
        {

        };
    }


    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (functions.USERNAME != "d")
        {
            if (!allowFormClose)
            {
                logController.Log("NAPOMINATOR Form1_FormClosing() - Dont allowed.");
                e.Cancel = true;
            }
        }
        ctsReadRabbitMQ.Cancel();
    }


    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        globalMouseHook.Dispose();
        logController.Dispose();

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
                personalTimes_secondsToStop = (int)table.Compute(ctrlPersonalPeriod.Text, null);
            }
            catch
            {
                ctrlPersonalPeriod.Text = "5";
                return;
            }
            if (personalTimes_secondsToStop <= 1)
            {
                ctrlPersonalPeriod.Text = "5";
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
        Show_Message_To_Polina($"Message: {textBox_PersonalPeriodText.Text}.\n Прошедшее время в секундах: {ctrlPersonalPeriod.Text}", $"Personal timer DONE.", false, true);

    }





    private async void btn_IPInfo_Click(object sender, EventArgs e)
    {
        Clear_TextBox_Log();
        Add_TextBox_Log($"--------------Start IpInfo-----------------", EventLogEntryType.Information, "");

        /*
        var messages = await functions.CheckUnreadChatMessages();
        var unreadCounts = JsonSerializer.Deserialize<Dictionary<string, int>>(messages);

        if (unreadCounts != null && unreadCounts.ContainsKey("dddMobile"))
        {
            Show_Message_To_Polina("У вас есть непрочитанные сообщения в чате!",
                                   "У вас есть непрочитанные сообщения в чате!",
                    false, true, false, 5 );
        } 
        */

        int.TryParse(ctrlHttpTimeout.Text, out int timeout);
        await PingTestProxy(cb_UsePing.Checked, cb_ShowContentLength.Checked, timeout);
        Add_TextBox_Log($"--------------End IpInfo-----------------", EventLogEntryType.Information, "");

    }



    private async void IpInfo_timer_Tick(object sender, EventArgs e)
    {
        if (int.TryParse(ctrlIpInfoSeconds.Text, out var seconds) == false)
        {
            seconds = 0;
            return;
        }

        IpInfo_timer.Stop();
        int.TryParse(ctrlHttpTimeout.Text, out var timeout);
        await PingTestProxy(cb_UsePing.Checked, cb_ShowContentLength.Checked, timeout);

        if (cb_IpInfoTimerEnabled.Checked == true)
            IpInfo_timer.Start();
    }

    private async Task PingTestProxy(bool usePing, bool showContentLength, int httpTimeout)
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

            var settings = Program._serviceProvider.GetRequiredService<IAppSettings>();
            if (httpTimeout == 0)
                httpTimeout = settings.NetworkConfig.HttpClientTimeoutSeconds;

            ipInfo.Create(usePing, showContentLength, httpTimeout);
            if (rtbProxyUrl.Text == "")
                rtbProxyUrl.Text = ipInfo.Proxy;

            var ipInfoDTO = await ipInfo.Process();

            string pingResultStr = "";
            if (usePing)
                if (ipInfoDTO.Ping_Status == IPStatus.Success.ToString())
                    pingResultStr = $" Ping - {ipInfoDTO.Ping_RoundtripTime} мс.";
                else
                    pingResultStr = $" Ping error. Status: {ipInfoDTO.Ping_Status}.";

            //Clear_TextBox_Log();
            Add_TextBox_Log($"-------------------------------", EventLogEntryType.Information, "");
            Add_TextBox_Log($"Proxy-{ipInfo.Proxy} IP-{ipInfoDTO.query} Country-{ipInfoDTO.countryCode} Timeout-{httpTimeout} Ping result-{pingResultStr}", EventLogEntryType.Information, "");
            Add_TextBox_Log($"-------------------------------", EventLogEntryType.Information, "");

            var sortedDictionary = ipInfoDTO.http_response_status.OrderBy(pair => pair.Key);

            foreach (var i in sortedDictionary)
            {
                Add_TextBox_Log($"{i.Key} - {i.Value} - {ipInfoDTO.http_response[i.Key].Length} bytes", EventLogEntryType.Information, "");
            }

            /*
            string response_len_1 = "", response_len_2 = "", response_len_3 = "";
            if (showContentLength)
            {
                response_len_1 = $"({ipInfoDTO.http_response_1?.Length} b)";
                response_len_2 = $"({ipInfoDTO.http_response_2?.Length} b)";
                response_len_3 = $"({ipInfoDTO.http_response_3?.Length} b)";
            }
            string http1_status = $"ip-api: {ipInfoDTO.http_response_status1}.{response_len_1}";
            string http2_status = $"rutor: {ipInfoDTO.http_response_status2}.{response_len_2}";
            string http3_status = $"youtube: {ipInfoDTO.http_response_status3}.{response_len_3}";
            */

            //Add_TextBox_Log($"{ipInfoDTO.query} {ipInfoDTO.countryCode} {httpTimeout} {http1_status} {http2_status} {http3_status} {pingResultStr}", EventLogEntryType.Information, "");
        }
    }

    private void rtb_IpInfoSeconds_TextChanged(object sender, EventArgs e)
    {

    }

    private void cb_IpInfoTimerEnabled_CheckedChanged(object sender, EventArgs e)
    {
        int seconds;
        if (int.TryParse(ctrlIpInfoSeconds.Text, out seconds) == false)
            seconds = 0;

        if (cb_IpInfoTimerEnabled.Checked == true)
        {
            if (seconds <= 0)
            {
                IpInfo_timer.Stop();
                logController.Log("Stop IpInfo_timer.");
            }
            else
            {
                IpInfo_timer.Interval = 1000 * seconds;
                IpInfo_timer.Start();
                logController.Log("Start IpInfo_timer.");
            }
        }
        else
        {
            IpInfo_timer.Stop();
            logController.Log("Stop IpInfo_timer.");
        }
    }

    private void btnPlus1min_Click(object sender, EventArgs e)
    {
        AddRemove_ctrlPersonalPeriod(1);
    }

    private void btnMinus1min_Click(object sender, EventArgs e)
    {
        AddRemove_ctrlPersonalPeriod(-1);
    }

    private void btnPlus5min_Click(object sender, EventArgs e)
    {
        AddRemove_ctrlPersonalPeriod(5);
    }

    private void btnMinus5min_Click(object sender, EventArgs e)
    {
        AddRemove_ctrlPersonalPeriod(-5);
    }

    void AddRemove_ctrlPersonalPeriod(int minutes)
    {

        int.TryParse(ctrlPersonalPeriod.Text.Replace("*60", ""), out int curValue);
        if (curValue > 0)
            curValue += minutes;
        if(curValue > 0)
            ctrlPersonalPeriod.Text = curValue.ToString()+"*60";
    }
}