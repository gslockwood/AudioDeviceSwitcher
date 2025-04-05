namespace AudioDeviceSwitcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            /*
            AudioDeviceManager audioDeviceManager = new();

            System.Diagnostics.Debug.WriteLine("Output devices");

            var playbackDevices = audioDeviceManager.GetPlaybackDevices();
            foreach( var device in playbackDevices )
                System.Diagnostics.Debug.WriteLine($"{device.FriendlyName} {device.State} {device.IsDefaultConsoleDevice}");

            AudioDeviceManager.SetDefaultPlaybackDevice(playbackDevices[0].Id);
            foreach( var device in audioDeviceManager.GetPlaybackDevices() )
                System.Diagnostics.Debug.WriteLine($"{device.FriendlyName} {device.State} {device.IsDefaultConsoleDevice}");

            System.Diagnostics.Debug.WriteLine("Input devices");

            var inputDevices = audioDeviceManager.ListInputDevices();
            foreach( var device in inputDevices )
                System.Diagnostics.Debug.WriteLine($"{device.FriendlyName} {device.State} {device.IsDefaultConsoleDevice}");

            audioDeviceManager.SetDefaulInputDevice(inputDevices[2].Id);
            */

        }
    }
}
