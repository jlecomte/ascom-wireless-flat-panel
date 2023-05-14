/*
 * WirelessFlatPanel.cs
 * Copyright (C) 2023 - Present, Julien Lecomte - All Rights Reserved
 * Licensed under the MIT License. See the accompanying LICENSE file for terms.
 */

using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace ASCOM.DarkSkyGeek
{
    //
    // Your driver's DeviceID is ASCOM.DarkSkyGeek.WirelessFlatPanel
    //
    // The Guid attribute sets the CLSID for ASCOM.DarkSkyGeek.WirelessFlatPanel
    // The ClassInterface/None attribute prevents an empty interface called
    // _DarkSkyGeek from being created and used as the [default] interface
    //

    /// <summary>
    /// DarkSkyGeek’s ASCOM WirelessFlatPanel Driver.
    /// </summary>
    [Guid("b97088ae-7680-4b44-95cb-f08e4b8972e9")]
    [ClassInterface(ClassInterfaceType.None)]
    public class WirelessFlatPanel : ICoverCalibratorV1
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.DarkSkyGeek.WirelessFlatPanel";

        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string deviceName = "DarkSkyGeek’s Wireless Flat Panel";

        // Constants used for Profile persistence
        private const string traceStateProfileName = "Trace Level";
        private const string traceStateDefault = "false";

        private const string bleDeviceAddressProfileName = "BLE Device Address";
        private const ulong bleDeviceAddressDefault = 0;

        // Variables to hold the current device configuration
        internal ulong bleDeviceAddress = bleDeviceAddressDefault;

        // Constants shared with the Arduino firmware...
        // Some of these are static because they are used in the setup dialog...
        public static Guid BLE_SERVICE_UUID = new Guid("0d389e0f-25dc-4070-9135-400b81e543ce");
        public static Guid BLE_CHARACTERISTIC_UUID = new Guid("2a0f87c9-7270-4c3e-aaa3-647961dfffa3");

        private const int MIN_BRIGHTNESS = 0;
        private const int MAX_BRIGHTNESS = 1023;

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private bool connectedState;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal TraceLogger tl;

        /// <summary>
        /// Variable to hold the physical BLE device we are communicating with.
        /// </summary>
        private BluetoothLEDevice bleDevice;

        /// <summary>
        /// Variable to hold the physical BLE device characteristic we are interacting with.
        /// </summary>
        private GattCharacteristic bleCharacteristic;

        /// <summary>
        /// Variable to hold the current brightness of the device.
        /// </summary>
        private UInt16 brightness = MIN_BRIGHTNESS;

        /// <summary>
        /// Initializes a new instance of the <see cref="DarkSkyGeek"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public WirelessFlatPanel()
        {
            tl = new TraceLogger("", "DarkSkyGeek");
            tl.LogMessage("WirelessFlatPanel", "Starting initialization");
            ReadProfile();
            connectedState = false;
            tl.LogMessage("WirelessFlatPanel", "Completed initialization");
        }

        //
        // PUBLIC COM INTERFACE ICoverCalibratorV1 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected)
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm(this))
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        /// <summary>Returns the list of custom action names supported by this driver.</summary>
        /// <value>An ArrayList of strings (SafeArray collection) containing the names of supported actions.</value>
        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }

        /// <summary>Invokes the specified device-specific custom action.</summary>
        /// <param name="ActionName">A well known name agreed by interested parties that represents the action to be carried out.</param>
        /// <param name="ActionParameters">List of required parameters or an <see cref="String.Empty">Empty String</see> if none are required.</param>
        /// <returns>A string response. The meaning of returned strings is set by the driver author.
        /// <para>Suppose filter wheels start to appear with automatic wheel changers; new actions could be <c>QueryWheels</c> and <c>SelectWheel</c>. The former returning a formatted list
        /// of wheel names and the second taking a wheel name and making the change, returning appropriate values to indicate success or failure.</para>
        /// </returns>
        public string Action(string actionName, string actionParameters)
        {
            LogMessage("", "Action {0}, parameters {1} not implemented", actionName, actionParameters);
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and does not wait for a response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a boolean response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        /// <returns>
        /// Returns the interpreted boolean response received from the device.
        /// </returns>
        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a string response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        /// <returns>
        /// Returns the string response received from the device.
        /// </returns>
        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        /// <summary>
        /// Dispose the late-bound interface, if needed. Will release it via COM
        /// if it is a COM object, else if native .NET will just dereference it
        /// for GC.
        /// </summary>
        public void Dispose()
        {
            Connected = false;
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
        }

        /// <summary>
        /// Set True to connect to the device hardware. Set False to disconnect from the device hardware.
        /// You can also read the property to check whether it is connected. This reports the current hardware state.
        /// </summary>
        /// <value><c>true</c> if connected to the hardware; otherwise, <c>false</c>.</value>
        public bool Connected
        {
            get
            {
                LogMessage("Connected", "Get {0}", IsConnected);
                return IsConnected;
            }
            set
            {
                tl.LogMessage("Connected", "Set {0}", value);
                if (value == IsConnected)
                    return;

                if (value)
                {
                    if (bleDeviceAddress == bleDeviceAddressDefault)
                    {
                        throw new ASCOM.DriverException("You have not yet paired a device");
                    }

                    LogMessage("Connected Set", "Connecting...");
                    Task t = ConnectToDevice();
                    t.Wait(15000); // Wait up to 15 seconds...
                    if (bleDevice != null && bleCharacteristic != null)
                    {
                        connectedState = true;
                    }
                    else
                    {
                        bleCharacteristic = null;
                        bleDevice?.Dispose();
                        bleDevice = null;
                        throw new ASCOM.DriverException("Failed to connect");
                    }
                }
                else
                {
                    connectedState = false;

                    LogMessage("Connected Set", "Disconnecting...");

                    bleCharacteristic?.Service?.Session?.Dispose();
                    bleCharacteristic?.Service?.Dispose();

                    bleCharacteristic = null;

                    bleDevice?.Dispose();
                    bleDevice = null;
                }
            }
        }

        /// <summary>
        /// Returns a description of the device, such as manufacturer and modelnumber. Any ASCII characters may be used.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get
            {
                tl.LogMessage("Description Get", deviceName);
                return deviceName;
            }
        }

        /// <summary>
        /// Descriptive and version information about this ASCOM driver.
        /// </summary>
        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverInfo = deviceName + " Version " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        /// <summary>
        /// A string containing only the major and minor version of the driver formatted as 'm.n'.
        /// </summary>
        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        /// <summary>
        /// The interface version number that this device supports. 
        /// </summary>
        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "1");
                return Convert.ToInt16("1");
            }
        }

        /// <summary>
        /// The short name of the driver, for display purposes.
        /// </summary>
        public string Name
        {
            get
            {
                tl.LogMessage("Name Get", deviceName);
                return deviceName;
            }
        }

        #endregion

        #region ICoverCalibrator Implementation

        /// <summary>
        /// Returns the state of the device cover, if present, otherwise returns "NotPresent"
        /// </summary>
        public CoverStatus CoverState
        {
            get
            {
                return CoverStatus.NotPresent;
            }
        }

        /// <summary>
        /// Initiates cover opening if a cover is present
        /// </summary>
        public void OpenCover()
        {
            tl.LogMessage("OpenCover", "Not implemented");
            throw new MethodNotImplementedException("OpenCover");
        }

        /// <summary>
        /// Initiates cover closing if a cover is present
        /// </summary>
        public void CloseCover()
        {
            tl.LogMessage("CloseCover", "Not implemented");
            throw new MethodNotImplementedException("CloseCover");
        }

        /// <summary>
        /// Stops any cover movement that may be in progress if a cover is present and cover movement can be interrupted.
        /// </summary>
        public void HaltCover()
        {
            tl.LogMessage("HaltCover", "Not implemented");
            throw new MethodNotImplementedException("HaltCover");
        }

        /// <summary>
        /// Returns the state of the calibration device, if present, otherwise returns CalibratorStatus.NotPresent
        /// </summary>
        public CalibratorStatus CalibratorState
        {
            get
            {
                return CalibratorStatus.Ready;
            }
        }

        /// <summary>
        /// Returns the current calibrator brightness in the range 0 (completely off) to <see cref="MaxBrightness"/> (fully on)
        /// </summary>
        public int Brightness
        {
            get
            {
                return brightness;
            }
        }

        /// <summary>
        /// The Brightness value that makes the calibrator deliver its maximum illumination.
        /// </summary>
        public int MaxBrightness
        {
            get
            {
                return MAX_BRIGHTNESS;
            }
        }

        /// <summary>
        /// Turns the calibrator on at the specified brightness if the device has calibration capability
        /// </summary>
        /// <param name="Brightness"></param>
        public void CalibratorOn(int Brightness)
        {
            if (Brightness < 0 || Brightness > MAX_BRIGHTNESS)
            {
                throw new ASCOM.InvalidValueException("Invalid brightness value", Brightness.ToString(), "[0, " + MAX_BRIGHTNESS.ToString() + "]");
            }

            UInt16 value = (UInt16) Brightness;

            CheckConnected("CalibratorOn");
            tl.LogMessage("CalibratorOn", "Sending request to device...");

            Task<bool> t = UpdateDeviceState(value);
            t.Wait();

            if (t.Result)
            {
                brightness = value;
                tl.LogMessage("CalibratorOff", "Device has been updated!");
            }
            else
            {
                throw new ASCOM.DriverException("Device state could not be successfully updated!");
            }
        }

        /// <summary>
        /// Turns the calibrator off if the device has calibration capability
        /// </summary>
        public void CalibratorOff()
        {
            CheckConnected("CalibratorOff");
            tl.LogMessage("CalibratorOff", "Sending request to device...");

            Task<bool> t = UpdateDeviceState(MIN_BRIGHTNESS);
            t.Wait();

            if (t.Result)
            {
                brightness = MIN_BRIGHTNESS;
                tl.LogMessage("CalibratorOff", "Device has been updated!");
            }
            else
            {
                throw new ASCOM.DriverException("Device state could not be successfully updated!");
            }
        }

        #endregion

        #region Private properties and methods

        // Here are some useful properties and methods that can be used as required
        // to help with driver development...

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "CoverCalibrator";
                if (bRegister)
                {
                    P.Register(driverID, deviceName);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        /// <summary>
        /// Attempts to connect to the BLE device, and sets the `bleDevice` and
        /// `bleCharacteristic` class instance members in case of success.
        /// </summary>
        private Task ConnectToDevice()
        {
            var tcs = new TaskCompletionSource<bool>();

            // You cannot connect to a BLE device without doing an enumeration.
            // That is why in version 1.0, the connection was not working reliably.
            // Starting with version 1.1, we use the BluetoothLEAdvertisementWatcher
            // to ensure that our device is powered up and advertising before attempting
            // a connection. The DeviceWatcher would also work, but it is much slower!

            var watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            watcher.Received += async (w, args) =>
            {
                if (args.BluetoothAddress == bleDeviceAddress)
                {
                    watcher.Stop();

                    Debug.WriteLine("Discovered the wireless flat panel device. Attempting to connect...");

                    bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
                    if (bleDevice == null)
                    {
                        tcs.SetException(new ASCOM.DriverException("Could not connect to device."));
                    }

                    GattDeviceServicesResult servicesResult = await bleDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                    if (servicesResult.Status == GattCommunicationStatus.Success)
                    {
                        var services = servicesResult.Services;
                        foreach (var service in services)
                        {
                            if (service.Uuid.Equals(BLE_SERVICE_UUID))
                            {
                                var characteristicsResult = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                                if (characteristicsResult.Status == GattCommunicationStatus.Success)
                                {
                                    var characteristics = characteristicsResult.Characteristics;
                                    foreach (var characteristic in characteristics)
                                    {
                                        if (characteristic.Uuid.Equals(BLE_CHARACTERISTIC_UUID))
                                        {
                                            bleCharacteristic = characteristic;
                                            Debug.WriteLine("Wireless flat panel device connected!");
                                            tcs.SetResult(true);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            watcher.Start();

            return tcs.Task;
        }

        /// <summary>
        /// Returns the current value of the BLE characteristic,
        /// or -1 if the characteristic could not be read successfully...
        /// </summary>
        private async Task<int> QueryDeviceState()
        {
            tl.LogMessage("QueryDeviceState", "Reading BLE characteristic value...");
            GattReadResult result = await bleCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (result.Status == GattCommunicationStatus.Success && result.Value.Length == 2)
            {
                return DataReader.FromBuffer(result.Value).ReadUInt16();
            }

            return -1;
        }

        /// <summary>
        /// Writes the specified value to the BLE characteristic.
        /// </summary>
        private async Task<bool> UpdateDeviceState(UInt16 brightness)
        {
            tl.LogMessage("UpdateDeviceState", "Writing BLE characteristic value...");
            var writer = new DataWriter();
            writer.WriteUInt16(brightness);
            GattCommunicationStatus result = await bleCharacteristic.WriteValueAsync(writer.DetachBuffer());
            return result == GattCommunicationStatus.Success;
        }

        /// <summary>
        /// Returns true if there is a valid connection to the device.
        /// </summary>
        private bool IsConnected
        {
            get
            {
                return connectedState;

                /*
                
                // I tried the following, which made sense to me, but it caused N.I.N.A.
                // to crash upon connecting to the device. I did not investigate further.

                if (!connectedState)
                {
                    return false;
                }

                Task<int> t = QueryDeviceState();
                t.Wait();
                return t.Result != -1;

                */
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "CoverCalibrator";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                bleDeviceAddress = ulong.Parse(driverProfile.GetValue(driverID, bleDeviceAddressProfileName, string.Empty, bleDeviceAddressDefault.ToString()));
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "CoverCalibrator";
                driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());
                driverProfile.WriteValue(driverID, bleDeviceAddressProfileName, bleDeviceAddress.ToString());
            }
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }

        #endregion
    }
}
