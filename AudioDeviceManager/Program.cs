using AudioDeviceManagerLibrary;

internal class Program
{
    private static void Main(string[] args)
    {
        //Console.WriteLine("Hello, World!");

        AudioDeviceManager audioDeviceManager = new();

        if( ( args.Length == 0 || args.Where(x => x.ToLower().Equals("/h")).Any() ) || args.Where(x => x.ToLower().Equals("/l")).Any() )
        {
            Console.WriteLine("Playback (Output) devices");

            var playbackDevices = audioDeviceManager.GetPlaybackDevices();
            for( int i = 0; i < playbackDevices.Count; i++ )
            {
                var device = playbackDevices[i];
                Console.WriteLine($"{i}. {device.FriendlyName} {device.State} {device.IsDefaultConsoleDevice}");
            }

            Console.WriteLine("\nInput devices");
            var inputDevices = audioDeviceManager.ListInputDevices();
            for( int i = 0; i < inputDevices.Count; i++ )
            {
                var device = inputDevices[i];
                Console.WriteLine($"{i}. {device.FriendlyName} {device.State} {device.IsDefaultConsoleDevice}");
            }

            Console.WriteLine("");
            Console.WriteLine("Format:  AudioDeviceManager /input:<index> to set the default input device to the device indexed in the Input devices list shown above.");
            Console.WriteLine("Format:  AudioDeviceManager /output:<index> to set the default playback device to the device indexed in the Playback (Output) devices list shown above.");
            Console.WriteLine("Format:  AudioDeviceManager /input:<index> /output:<index>");
            Console.WriteLine("");

        }

        else
        {
            string? argOfInterest = args.FirstOrDefault(name => name.ToLower().Contains("/input:"));
            if( argOfInterest != null )
            {
                argOfInterest = argOfInterest.Split("/input:")[1];
                if( int.TryParse(argOfInterest, out int value) )
                {
                    if( audioDeviceManager.SetDefaulInputDeviceByIndex(value) )
                        Console.WriteLine("Input Device changed");
                    else
                        Console.WriteLine("Input Device  not changed");
                }

            }
        again:
            argOfInterest = args.FirstOrDefault(name => name.ToLower().Contains("/output:"));
            if( argOfInterest != null )
            {
                argOfInterest = argOfInterest.Split("/output:")[1];
                if( int.TryParse(argOfInterest, out int value) )
                {
                    if( audioDeviceManager.SetDefaultPlaybackDeviceByIndex(value) )
                        Console.WriteLine("Playback Device changed");
                    else
                        Console.WriteLine("PlaybackD evice not changed");
                }

            }

            //goto again;

            /*
             var filteredArgs = args.Where(name => name != "/h").ToList();
            foreach( string arg in filteredArgs ){}
            */
        }

        /*
        System.Diagnostics.Debug.WriteLine("Output devices");

        var playbackDevices = audioDeviceManager.GetPlaybackDevices();
        foreach( var device in playbackDevices )
            System.Diagnostics.Debug.WriteLine($"{device.FriendlyName} {device.State} {device.IsDefaultConsoleDevice}");

        audioDeviceManager.SetDefaultPlaybackDevice(playbackDevices.Skip(2).First().Id);
        foreach( var device in audioDeviceManager.GetPlaybackDevices() )
            System.Diagnostics.Debug.WriteLine($"{device.FriendlyName} {device.State} {device.IsDefaultConsoleDevice}");

        System.Diagnostics.Debug.WriteLine("Input devices");

        var inputDevices = audioDeviceManager.ListInputDevices();
        foreach( var device in inputDevices )
            System.Diagnostics.Debug.WriteLine($"{device.FriendlyName} {device.State} {device.IsDefaultConsoleDevice}");

        audioDeviceManager.SetDefaulInputDevice(inputDevices.First().Id);
        */
    }
}