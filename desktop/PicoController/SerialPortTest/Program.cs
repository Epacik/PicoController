// See https://aka.ms/new-console-template for more information
using CoreAudio;
using System.IO.Ports;

Console.WriteLine("Hello, World!");
//var port = new SerialPort()
//{
//    PortName               = "COM9",
//    BaudRate               = 115200,
//    Parity                 = Parity.None,
//    StopBits               = StopBits.One,
//    Handshake              = Handshake.None,
//    DataBits               = 8,
//    ReceivedBytesThreshold = 1,
//    DtrEnable              = true,
//};

//MMDeviceEnumerator audioEnumerator = new MMDeviceEnumerator();

//port.PinChanged += (s, e) => { Console.WriteLine(e); };
//port.ErrorReceived  += (s, e) => { Console.WriteLine(e.ToString()); };
//port.DataReceived += (s, e) =>
//{
//    var data = port.ReadLine()?.Trim() ?? "";
    
//    var device = audioEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
//    if (device?.AudioEndpointVolume is null)
//        return;

//    switch (data)
//    {
//        case "Toggle Mute":
//            device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
//            break;
//        case "Volume Up":
//            device.AudioEndpointVolume.VolumeStepUp();
//            break;
//        case "Volume Down":
//            device.AudioEndpointVolume.VolumeStepDown();
//            break;
//    }
//};
//port.Open();

Console.ReadKey();
