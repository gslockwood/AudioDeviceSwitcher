using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;

namespace AudioDeviceManagerLibrary
{
    /// <summary>
    /// Represents an audio playback device.
    /// </summary>
    public class AudioDevice
    {
        /// <summary>
        /// The user-friendly name of the device.
        /// </summary>
        public string? FriendlyName { get; set; }

        /// <summary>
        /// The unique system identifier for the device.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// The current state of the device.
        /// </summary>
        public DeviceState State { get; set; }

        /// <summary>
        /// Indicates if this is the current default playback device for console/communication.
        /// </summary>
        public bool IsDefaultConsoleDevice { get; set; }

        /// <summary>
        /// Indicates if this is the current default playback device for multimedia.
        /// </summary>
        public bool IsDefaultMultimediaDevice { get; set; }

        public override string ToString()
        {
            string defaultMarker = "";
            if( IsDefaultConsoleDevice && IsDefaultMultimediaDevice )
                defaultMarker = " (Default)";
            else if( IsDefaultConsoleDevice )
                defaultMarker = " (Default Communication)";
            else if( IsDefaultMultimediaDevice )
                defaultMarker = " (Default Multimedia)";

            return $"{FriendlyName}{defaultMarker} [{State}]";
        }
    }

    /// <summary>
    /// Manages listing and setting default audio playback devices.
    /// </summary>
    public class AudioDeviceManager
    {
        private readonly MMDeviceEnumerator _deviceEnumerator;

        public AudioDeviceManager()
        {
            _deviceEnumerator = new MMDeviceEnumerator();
        }

        /// <summary>
        /// Gets a list of all active audio playback devices.
        /// </summary>
        /// <returns>A list of AudioDevice objects.</returns>
        public List<AudioDevice> GetPlaybackDevices()
        {
            var devices = new List<AudioDevice>();
            MMDevice? defaultConsoleDevice = null;
            MMDevice? defaultMultimediaDevice = null;

            try
            {
                // Get default devices first to mark them later
                defaultConsoleDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                defaultMultimediaDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }
            catch( COMException ex ) when( (uint)ex.HResult == 0x80070490 ) // ELEMENT_NOT_FOUND
            {
                // Handle cases where no default device is set for a role (less common)
                Console.WriteLine("Warning: Could not find a default device for one or more roles.");
            }
            catch( Exception ex )
            {
                Console.WriteLine($"Error getting default devices: {ex.Message}");
                // Continue trying to enumerate devices even if default fails
            }


            try
            {
                var deviceCollection = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                foreach( var device in deviceCollection )
                {
                    using( device ) // Ensure COM object is released
                    {
                        devices.Add(new AudioDevice
                        {
                            FriendlyName = device.FriendlyName,
                            Id = device.ID,
                            State = device.State,
                            IsDefaultConsoleDevice = defaultConsoleDevice != null && device.ID == defaultConsoleDevice.ID,
                            IsDefaultMultimediaDevice = defaultMultimediaDevice != null && device.ID == defaultMultimediaDevice.ID
                        });
                    }
                }
            }
            catch( Exception ex )
            {
                Console.WriteLine($"Error enumerating devices: {ex.Message}");
                // Depending on requirements, you might want to throw or return empty list
            }
            finally
            {
                // Clean up default device COM objects if they were retrieved
                defaultConsoleDevice?.Dispose();
                defaultMultimediaDevice?.Dispose();
                //if( defaultConsoleDevice != null ) Marshal.ReleaseComObject(defaultConsoleDevice);
                //if( defaultMultimediaDevice != null ) Marshal.ReleaseComObject(defaultMultimediaDevice);
            }

            return devices;
            //
        }

        /// <summary>
        /// Sets the default audio playback device for Console (Communication) and Multimedia roles.
        /// </summary>
        /// <param name="deviceId">The unique system identifier of the device to set as default.</param>
        /// <returns>True if successful, False otherwise.</returns>
        public bool SetDefaultPlaybackDevice(string deviceId)
        {
            if( string.IsNullOrWhiteSpace(deviceId) )
            {
                Console.WriteLine("Error: Device ID cannot be empty.");
                return false;
            }

            IPolicyConfigVista? policyConfig = null;
            try
            {
                // Use the PolicyConfigClient COM object (defined below)
                policyConfig = new PolicyConfigClient() as IPolicyConfigVista;
                if( policyConfig != null )
                {
                    // Set for Console/Communication role
                    int hr = policyConfig.SetDefaultEndpoint(deviceId, ERole.eConsole);
                    if( hr != 0 )
                    {
                        Console.WriteLine($"Error setting default console device: HRESULT={hr:X}");
                        // Optionally throw an exception here or return false
                        // Marshal.ThrowExceptionForHR(hr); // Throws if you want exception behavior
                    }

                    // Set for Multimedia role
                    hr = policyConfig.SetDefaultEndpoint(deviceId, ERole.eMultimedia);
                    if( hr != 0 )
                    {
                        Console.WriteLine($"Error setting default multimedia device: HRESULT={hr:X}");
                        // Optionally throw an exception here or return false
                        // Marshal.ThrowExceptionForHR(hr);
                    }

                    // Set for general communications role (often overlaps with eConsole)
                    hr = policyConfig.SetDefaultEndpoint(deviceId, ERole.eCommunications);
                    if( hr != 0 )
                    {
                        Console.WriteLine($"Error setting default communications device: HRESULT={hr:X}");
                        // Optionally throw an exception here or return false
                        // Marshal.ThrowExceptionForHR(hr);
                    }

                    Console.WriteLine($"Successfully attempted to set device ID {deviceId} as default.");
                    return true; // Indicate attempt was made (might need further check if needed)
                }
                else
                {
                    Console.WriteLine("Error: Could not instantiate PolicyConfigClient.");
                    return false;
                }
            }
            catch( Exception ex )
            {
                Console.WriteLine($"Error setting default device: {ex.Message}");
                return false;
            }
            finally
            {
                if( policyConfig != null && Marshal.IsComObject(policyConfig) )
                {
                    Marshal.ReleaseComObject(policyConfig);
                }
            }
        }

        public bool SetDefaultPlaybackDeviceByIndex(int index)
        {
            List<AudioDevice> outputDevices = GetPlaybackDevices();
            AudioDevice device = outputDevices[index];
            if( device != null && device.Id != null )
                return this.SetDefaultPlaybackDevice(device.Id);

            return false;

        }




        //public struct InputAudioDevice
        //{
        //    public string FriendlyName { get; set; }
        //    public string ID { get; set; }
        //    public bool IsDefault { get; set; } // Is it the default for Role.Console?
        //    public bool IsDefaultCommunications { get; set; } // Is it the default for Role.Communications?
        //    public DeviceState State { get; set; } // e.g., Active, Disabled, Unplugged

        //    public override string ToString()
        //    {
        //        string defaultInfo = IsDefault ? "[Default]" : "";
        //        string defaultCommInfo = IsDefaultCommunications ? "[Default Communications]" : "";
        //        return $"{FriendlyName} ({State}) {defaultInfo}{defaultCommInfo} (ID: {ID})";
        //    }
        //}


        public List<AudioDevice> ListInputDevices(DeviceState deviceState = DeviceState.Active)
        {
            var devices = new List<AudioDevice>();

            try
            {
                // Get the default devices for different roles (important for context)
                string defaultDeviceId = "";
                string defaultCommDeviceId = "";

                // Role.Console is typically used for general audio applications (games, media players)
                if( _deviceEnumerator.HasDefaultAudioEndpoint(DataFlow.Capture, Role.Console) )
                {
                    defaultDeviceId = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console)?.ID;
                }

                // Role.Communications is used for communication apps (VoIP, Teams, Discord)
                if( _deviceEnumerator.HasDefaultAudioEndpoint(DataFlow.Capture, Role.Communications) )
                {
                    defaultCommDeviceId = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications)?.ID;
                }

                // Enumerate devices for the 'Capture' data flow (input devices)
                MMDeviceCollection captureDevices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, deviceState);

                foreach( MMDevice device in captureDevices )
                {
                    devices.Add(new AudioDevice
                    {
                        // FriendlyName is the user-friendly name shown in Sound settings
                        FriendlyName = device.FriendlyName,
                        // ID is the unique identifier for the device
                        Id = device.ID,
                        // Check if this device is the default for the specified roles
                        IsDefaultConsoleDevice = !string.IsNullOrEmpty(defaultDeviceId) && device.ID == defaultDeviceId,
                        IsDefaultMultimediaDevice = !string.IsNullOrEmpty(defaultCommDeviceId) && device.ID == defaultCommDeviceId,
                        // State indicates if the device is Active, Disabled, etc.
                        State = device.State
                    });

                    // Dispose of the device object to release resources
                    device.Dispose();
                }
                //captureDevices.Dispose(); // Dispose collection
            }
            catch( Exception ex )
            {
                // Handle potential exceptions (e.g., COM errors)
                Console.WriteLine($"Error listing audio devices: {ex.Message}");
                // Depending on the application, you might want to throw, log, or return empty list
            }

            return devices;
        }


        public bool SetDefaulInputDeviceByIndex(int index)
        {
            var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            MMDevice device = devices[index];
            if( device != null )
                return SetDefaulInputDevice(device);

            return false;
        }

        private bool SetDefaulInputDevice(MMDevice device)
        {
            if( device != null )
            {
                try
                {
                    // Use the PolicyConfigClient COM object (defined below)
                    if( new PolicyConfigClient() is IPolicyConfigVista policyConfig )
                    {
                        policyConfig.SetDefaultEndpoint(device.ID, ERole.eConsole);
                        Marshal.ReleaseComObject(policyConfig);
                        return true;
                    }
                }
                catch { }
                finally
                { }

            }

            return false;
        }
        public bool SetDefaulInputDevice(string id)
        {
            var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            MMDevice? device = devices.FirstOrDefault(d => d.ID == id);
            if( device != null )
                return SetDefaulInputDevice(device);

            return false;

            //if( device != null )
            //{
            //    try
            //    {
            //        // Use the PolicyConfigClient COM object (defined below)
            //        IPolicyConfigVista? policyConfig = new PolicyConfigClient() as IPolicyConfigVista;
            //        if( policyConfig != null )
            //        {
            //            policyConfig.SetDefaultEndpoint(device.ID, ERole.eConsole);
            //            Marshal.ReleaseComObject(policyConfig);
            //            return true;
            //        }
            //    }
            //    catch
            //    {
            //    }
            //    finally
            //    { }

            //}

            //return false;
        }


        // --- COM Interop for setting default device ---
        // These interfaces and classes are needed to call the undocumented Windows API

        [ComImport]
        [Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        //[GeneratedComInterface]
        internal interface IPolicyConfigVista
        {
            [PreserveSig]
            int GetMixFormat(string pszDeviceName, IntPtr ppFormat);

            [PreserveSig]
            int GetDeviceFormat(string pszDeviceName, bool bDefault, IntPtr ppFormat);

            [PreserveSig]
            int ResetDeviceFormat(string pszDeviceName);

            [PreserveSig]
            int SetDeviceFormat(string pszDeviceName, bool bDefault, IntPtr pFormat);

            [PreserveSig]
            int GetProcessingPeriod(string pszDeviceName, bool bDefault, IntPtr pmftDefaultPeriod, IntPtr pmftMinimumPeriod);

            [PreserveSig]
            int SetProcessingPeriod(string pszDeviceName, IntPtr pmftPeriod);

            [PreserveSig]
            int GetShareMode(string pszDeviceName, IntPtr pShareMode);

            [PreserveSig]
            int SetShareMode(string pszDeviceName, IntPtr shareMode);

            [PreserveSig]
            int GetPropertyValue(string pszDeviceName, IntPtr key, IntPtr pv);

            [PreserveSig]
            int SetPropertyValue(string pszDeviceName, IntPtr key, IntPtr pv);

            // Use ERole enum values for role parameter
            [PreserveSig]
            int SetDefaultEndpoint(string pszDeviceName, ERole role);

            [PreserveSig]
            int SetEndpointVisibility(string pszDeviceName, bool bVisible);
        }

        [ComImport]
        [Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")] // CLSID_PolicyConfigClient
        internal class PolicyConfigClient
        {
        }

        // ERole enum defined in NAudio.CoreAudioApi, but redefined here
        // for clarity if not directly using NAudio's enum in SetDefaultEndpoint call signature.
        // Ensure these values match the standard ERole enum.
        internal enum ERole
        {
            eConsole = 0,
            eMultimedia = 1,
            eCommunications = 2,
            ERole_enum_count = 3 // Typically not used directly
        }
    }
}