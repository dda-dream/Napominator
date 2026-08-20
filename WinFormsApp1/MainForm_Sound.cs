using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;

namespace Napominator;


public partial class MainForm : Form
{
    static WaveIn? audioSource;
    double soundLevel;
    double continuousBig;
    double shuminatorCount = 0, tot_shuminatorCount = 0;



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
        } catch
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
            } else
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
        } catch (Exception ex)
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

}
