using AudioDeviceManagerLibrary;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        AudioDeviceManager audioDeviceManager = new();

        var assembly = Assembly.GetExecutingAssembly();
        var productAttribute = assembly.GetCustomAttribute<AssemblyProductAttribute>();
        string productName = productAttribute?.Product ?? "Unknown Product";

        // Retrieve the AssemblyVersion
        string productVersion = assembly.GetName().Version?.ToString() ?? "Unknown Version";

        if( ( args.Length == 0 || args.Where(x => x.ToLower().Equals("/h")).Any() ) || args.Where(x => x.ToLower().Equals("/l")).Any() )
        {
            Console.WriteLine($"{productName} {productVersion}");

            Console.WriteLine("Playback (Output) devices");

            var playbackDevices = audioDeviceManager.ListPlaybackDevices();
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
            Console.WriteLine("Format:  AudioDeviceManager /input:<\"name\"> to set the default input device to the device that starts with \"name\" in the Input devices list shown above.");

            Console.WriteLine("Format:  AudioDeviceManager /output:<index> to set the default playback device to the device indexed in the Playback (Output) devices list shown above.");
            Console.WriteLine("Format:  AudioDeviceManager /output:<\"name\"> to set the default input device to the device that starts with \"name\" in the Playback (Output)  devices list shown above.");

            Console.WriteLine("Format:  AudioDeviceManager /input:<index> /output:<index>");
            Console.WriteLine("");

        }

        else
        {
            string? argOfInterest = args.FirstOrDefault(name => name.Contains("/input:", StringComparison.CurrentCultureIgnoreCase));
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
                else
                {
                    if( audioDeviceManager.SetDefaulInputDeviceByName(argOfInterest) )
                        Console.WriteLine("Playback Device changed");
                    else
                        Console.WriteLine("Playback Device not changed");
                }


            }

            argOfInterest = args.FirstOrDefault(name => name.Contains("/output:", StringComparison.CurrentCultureIgnoreCase));
            if( argOfInterest != null )
            {
                argOfInterest = argOfInterest.Split("/output:")[1];
                if( int.TryParse(argOfInterest, out int value) )
                {
                    if( audioDeviceManager.SetDefaultPlaybackDeviceByIndex(value) )
                        Console.WriteLine("Playback Device changed");
                    else
                        Console.WriteLine("Playback Device not changed");
                }
                else
                {
                    if( audioDeviceManager.SetDefaultPlaybackDeviceByName(argOfInterest) )
                        Console.WriteLine("Playback Device changed");
                    else
                        Console.WriteLine("Playback Device not changed");
                }

            }

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