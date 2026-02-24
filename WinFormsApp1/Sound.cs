namespace Napominator;

class Sound
{
    /*
    
    LogController logController;


    static WaveIn? audioSource;
    double soundLevel;
    double continuousBig;
    double shuminatorCount = 0, tot_shuminatorCount = 0;
    static bool messageShown = false;

    MMDevice? device = null;
    const int constCountNoMicrophoneDetected = 5;
    int count_NoMicrophoneDetected = constCountNoMicrophoneDetected;
    bool Start_NoiseDetector_executing = false;


    public void parmLogController(LogController logController)
    {
        this.logController = logController;
    }

    public void Start_NoiseDetector()
    {
        if (GetStringFromSettings("[Shuminator_Enabled]") == "0")
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
                if (device is null && devitem.FriendlyName.Contains(GetStringFromSettings("[Shuminator_MicrophoneName]")))
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
            if (GetDoubleFromSettings("[LockWorkStation_NoMicrophone]") == 1)
            {
                logController.Log("Подключи микрофон!");
                ShuminatorPlaySoundWarning("podkluchi_microfon.mp3", false, logController);
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
        logController.Log("e: Start_NoiseDetector()");
    }


    */
}
