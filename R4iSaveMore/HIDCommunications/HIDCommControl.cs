//-----------------------------------------------------------------------------
//
//  usbGenericHidCommunications.cs
//
//  USB Generic HID Communications 3_0_0_0
//
//  A class for communicating with Generic HID USB devices
//  Copyright (C) 2011 Simon Inns
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Web:    http://www.waitingforfriday.com
//  Email:  simon.inns@gmail.com
//
//-----------------------------------------------------------------------------

using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
// The following namespace allows debugging output (when compiled in debug mode)
using System.Diagnostics;

namespace HIDCommunications
{
    /// <summary>
    /// USB Generic HID Communication class
    /// </summary>
    /// <remarks>This class provides communication and device handling events
    /// for USB devices which implement the generic HID protocol. It is
    /// designed to be inherited into a class which deals with a specific
    /// device firmware and is therefore defined as an abstract class.
    /// 
    /// This class definition contains the highest level parts of the
    /// class which is defined over a number of files</remarks>
    public class HIDCommControl : Control
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>This constructor method creates an object for HID communication and attempts to find
        /// (and bind to) the USB device indicated by the passed VID (Vendor ID) and PID (Product ID) which
        /// should both be passed as unsigned integers.</remarks>
        public HIDCommControl(int vid, int pid)
        {
            Debug.WriteLine("HIDCommControl:HIDCommControl() -> Class constructor called");

            // Set the deviceAttached flag to false
            _deviceInformation.deviceAttached = false;

            // Store the target device's VID and PID in the device attributes
            _deviceInformation.targetVid = (UInt16)vid;
            _deviceInformation.targetPid = (UInt16)pid;

            // Register for device notifications
            registerForDeviceNotifications(Handle);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        /// <remarks>This method closes any open connections to the USB device and clears up any resources
        /// that have been consumed over the lifetime of the object.</remarks>
        ~HIDCommControl()
        {
            Debug.WriteLine("HIDCommControl:HIDCommControl() -> Class destructor called");
            // Detach the USB device (performs required clean up operations)
            DetachUsbDevice();
        }

        /// <summary>
        /// deviceAttributesStructure is used to represent the details of the target USB device as the methods
        /// discover them so they can be reused by other methods when communicating with the device
        /// </summary>
        private struct deviceInformationStructure
        {
            public UInt16 targetVid;                // Our target device's VID
            public UInt16 targetPid;                // Our target device's PID
            public bool deviceAttached;             // Device attachment state flag
            public HID.HIDD_ATTRIBUTES attributes;      // HID Attributes
            public HID.HIDP_CAPS capabilities;          // HID Capabilities
            public SafeFileHandle readHandle;       // Read handle from the device
            public SafeFileHandle writeHandle;      // Write handle to the device
            public SafeFileHandle hidHandle;        // Handle used for communicating via hid.dll
            public String devicePathName;           // The device's path name
            public IntPtr deviceNotificationHandle; // The device's notification handle
        }

        /// <summary>
        /// deviceAttributes contains the discovered attributes of the target USB device
        /// </summary>
        private deviceInformationStructure _deviceInformation;

        /// <summary>
        /// Detach the USB device
        /// </summary>
        /// <remarks>This method detaches the USB device and frees the read and write handles
        /// to the device.</remarks>
        private void DetachUsbDevice()
        {
            Debug.WriteLine("HIDCommControl:detachUsbDevice() -> Method called");

            // Is a device currently attached?
            if (IsDeviceAttached)
            {
                Debug.WriteLine("HIDCommControl:detachUsbDevice() -> Detaching device and closing file handles");
                // Set the device status to detached;
                IsDeviceAttached = false;

                // Close the readHandle, writeHandle and hidHandle
                if (!_deviceInformation.hidHandle.IsInvalid) _deviceInformation.hidHandle.Close();
                if (!_deviceInformation.readHandle.IsInvalid) _deviceInformation.readHandle.Close();
                if (!_deviceInformation.writeHandle.IsInvalid) _deviceInformation.writeHandle.Close();

                // Throw an event
                onUsbEvent(EventArgs.Empty);
            }
            else Debug.WriteLine("HIDCommControl:detachUsbDevice() -> No device attached");
        }

        /// <summary>
        /// Is device attached?
        /// </summary>
        /// <remarks>This method is used to set (private) or test (public) the device attachment status</remarks>
        public bool IsDeviceAttached { get { return _deviceInformation.deviceAttached; } private set { _deviceInformation.deviceAttached = value; } }

        #region outputToDeviceRegion

        /// <summary>
        /// writeRawReportToDevice - Writes a report to the device using interrupt transfer.
        /// Note: this method performs no checking on the buffer.  The first byte must 
        /// always be zero (or the write will fail!) and the second byte should be the
        /// command number for the USB device firmware.
        /// </summary>
        protected bool writeRawReportToDevice(Byte[] outputReportBuffer)
        {
            bool success = false;
            int numberOfBytesWritten = 0;

            // Make sure a device is attached
            if (!IsDeviceAttached)
            {
                Debug.WriteLine("HIDCommControl:writeReportToDevice(): -> No device attached!");
                return success;
            }

            try
            {
                // Set an output report via interrupt to the device
                success = Kernel32.WriteFile(
                _deviceInformation.writeHandle,
                outputReportBuffer,
                outputReportBuffer.Length,
                ref numberOfBytesWritten,
                IntPtr.Zero);

                if (success) Debug.WriteLine("HIDCommControl:writeReportToDevice(): -> Write report succeeded");
                else Debug.WriteLine("HIDCommControl:writeReportToDevice(): -> Write report failed!");

                return success;
            }
            catch (Exception)
            {
                // An error - send out some debug and return failure
                Debug.WriteLine("HIDCommControl:writeReportToDevice(): -> EXCEPTION: When attempting to send an output report");
                return false;
            }
        }
        #endregion

        #region inputFromDeviceRegion
        /// <summary>
        /// readRawReportFromDevice - Reads a raw report from the device with timeout handling
        /// Note: This method performs no checking on the buffer.  The first byte returned is
        /// usually zero, the second byte is the command that the USB firmware is replying to.
        /// The other 63 bytes are (possibly) data.
        /// 
        /// The maximum report size will be determind by the length of the inputReportBuffer.
        /// </summary>
        private bool readRawReportFromDevice(ref Byte[] inputReportBuffer, ref int numberOfBytesRead)
        {
            IntPtr eventObject = IntPtr.Zero;
            NativeOverlapped hidOverlapped = new NativeOverlapped();
            IntPtr nonManagedBuffer = IntPtr.Zero;
            IntPtr nonManagedOverlapped = IntPtr.Zero;
            Int32 result = 0;
            bool success;

            // Make sure a device is attached
            if (!IsDeviceAttached)
            {
                Debug.WriteLine("HIDCommControl:readReportFromDevice(): -> No device attached!");
                return false;
            }

            try
            {
                // Prepare an event object for the overlapped ReadFile
                eventObject = Kernel32.CreateEvent(IntPtr.Zero, false, false, "");

                hidOverlapped.OffsetLow = 0;
                hidOverlapped.OffsetHigh = 0;
                hidOverlapped.EventHandle = eventObject;

                // Allocate memory for the unmanaged input buffer and overlap structure.
                nonManagedBuffer = Marshal.AllocHGlobal(inputReportBuffer.Length);
                nonManagedOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf(hidOverlapped));
                Marshal.StructureToPtr(hidOverlapped, nonManagedOverlapped, false);

                // Read the input report buffer
                Debug.WriteLine("HIDCommControl:readReportFromDevice(): -> Attempting to ReadFile");
                success = Kernel32.ReadFile(
                    _deviceInformation.readHandle,
                    nonManagedBuffer,
                    inputReportBuffer.Length,
                    ref numberOfBytesRead,
                    nonManagedOverlapped);

                if (!success)
                {
                    // We are now waiting for the FileRead to complete
                    Debug.WriteLine("HIDCommControl:readReportFromDevice(): -> ReadFile started, waiting for completion...");

                    // Wait a maximum of 3 seconds for the FileRead to complete
                    result = Kernel32.WaitForSingleObject(eventObject, 3000);

                    switch (result)
                    {
                        // Has the ReadFile completed successfully?
                        case (System.Int32)Kernel32.WAIT_OBJECT_0:
                            success = true;

                            // Get the number of bytes transferred
                            Kernel32.GetOverlappedResult(_deviceInformation.readHandle, nonManagedOverlapped, ref numberOfBytesRead, false);

                            Debug.WriteLine(string.Format("HIDCommControl:readReportFromDevice(): -> ReadFile successful (overlapped) {0} bytes read", numberOfBytesRead));
                            break;

                        // Did the FileRead operation timeout?
                        case Kernel32.WAIT_TIMEOUT:
                            success = false;

                            // Cancel the ReadFile operation
                            Kernel32.CancelIo(_deviceInformation.readHandle);
                            if (!_deviceInformation.hidHandle.IsInvalid) _deviceInformation.hidHandle.Close();
                            if (!_deviceInformation.readHandle.IsInvalid) _deviceInformation.readHandle.Close();
                            if (!_deviceInformation.writeHandle.IsInvalid) _deviceInformation.writeHandle.Close();

                            // Detach the USB device to try to get us back in a known state
                            DetachUsbDevice();

                            Debug.WriteLine("HIDCommControl:readReportFromDevice(): -> ReadFile timedout! USB device detached");
                            break;

                        // Some other unspecified error has occurred?
                        default:
                            success = false;

                            // Cancel the ReadFile operation

                            // Detach the USB device to try to get us back in a known state
                            DetachUsbDevice();

                            Debug.WriteLine("HIDCommControl:readReportFromDevice(): -> ReadFile unspecified error! USB device detached");
                            break;
                    }
                }
                if (success)
                {
                    // Report receieved correctly, copy the unmanaged input buffer over to the managed buffer
                    Marshal.Copy(nonManagedBuffer, inputReportBuffer, 0, numberOfBytesRead);
                    Debug.WriteLine(string.Format("HIDCommControl:readReportFromDevice(): -> ReadFile successful {0} bytes read", numberOfBytesRead));
                }
            }
            catch (Exception)
            {
                // An error - send out some debug and return failure
                Debug.WriteLine("HIDCommControl:readReportFromDevice(): -> EXCEPTION: When attempting to receive an input report");
                return false;
            }

            // Release non-managed objects before returning
            Marshal.FreeHGlobal(nonManagedBuffer);
            Marshal.FreeHGlobal(nonManagedOverlapped);

            // Close the file handle to release the object
            Kernel32.CloseHandle(eventObject);

            return success;
        }

        /// <summary>
        /// readSingleReportFromDevice - Reads a single report packet from the USB device.
        /// The size of the passed inputReportBuffer must be correct for the device, so
        /// this method checks the passed buffer's size against the input report size
        /// discovered by the device enumeration.
        /// </summary>
        /// <param name="inputReportBuffer"></param>
        /// <returns></returns>
        protected bool readSingleReportFromDevice(ref Byte[] inputReportBuffer)
        {
            bool success;
            int numberOfBytesRead = 0;

            // The size of our inputReportBuffer must be at least the same size as the input report.
            if (inputReportBuffer.Length != (int)_deviceInformation.capabilities.inputReportByteLength)
            {
                // inputReportBuffer is not the right length!
                Debug.WriteLine(
                    "HIDCommControl:readSingleReportFromDevice(): -> ERROR: The referenced inputReportBuffer size is incorrect for the input report size!");
                return false;
            }

            // The readRawReportFromDevice method will fill the passed readBuffer or return false
            success = readRawReportFromDevice(ref inputReportBuffer, ref numberOfBytesRead);

            return success;
        }

        /// <summary>
        /// readMultipleReportsFromDevice - Attempts to retrieve multiple reports from the device in 
        /// a single read.  This action can be block the form execution if you request too much data.
        /// If you need more data from the device and want to avoid any blocking you will have to make
        /// multiple commands to the device and deal with the multiple requests and responses in your
        /// application.
        /// </summary>
        /// <param name="inputReportBuffer"></param>
        /// <param name="numberOfReports"></param>
        /// <returns></returns>
        protected bool readMultipleReportsFromDevice(ref Byte[] inputReportBuffer, int numberOfReports)
        {
            bool success = false;
            int numberOfBytesRead = 0;
            long pointerToBuffer = 0;

            // Define a temporary buffer for assembling partial data reads into the completed inputReportBuffer
            Byte[] temporaryBuffer = new Byte[inputReportBuffer.Length];

            // Range check the number of reports
            if (numberOfReports == 0)
            {
                Debug.WriteLine(
                    "HIDCommControl:readMultipleReportsFromDevice(): -> ERROR: You cannot request 0 reports!");
                return false;
            }

            if (numberOfReports > 128)
            {
                Debug.WriteLine(
                    "HIDCommControl:readMultipleReportsFromDevice(): -> ERROR: Reference application testing does not verify the code for more than 128 reports");
                return false;
            }

            // The size of our inputReportBuffer must be at least the same size as the input report multiplied by the number of reports requested.
            if (inputReportBuffer.Length != ((int)_deviceInformation.capabilities.inputReportByteLength * numberOfReports))
            {
                // inputReportBuffer is not the right length!
                Debug.WriteLine(
                    "HIDCommControl:readMultipleReportsFromDevice(): -> ERROR: The referenced inputReportBuffer size is incorrect for the number of input reports requested!");
                return false;
            }

            // The readRawReportFromDevice method will fill the passed read buffer or return false
            while (pointerToBuffer != ((int)_deviceInformation.capabilities.inputReportByteLength * numberOfReports))
            {
                Debug.WriteLine(
                    "HIDCommControl:readMultipleReportsFromDevice(): -> Reading from device...");
                success = readRawReportFromDevice(ref temporaryBuffer, ref numberOfBytesRead);

                // Was the read successful?
                if (!success)
                {
                    return false;
                }

                // Copy the received data into the referenced input buffer
                Array.Copy(temporaryBuffer, 0, inputReportBuffer, pointerToBuffer, (long)numberOfBytesRead);
                pointerToBuffer += (long)numberOfBytesRead;
            }

            return success;
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Create a delegate for the USB event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void usbEventsHandler(object sender, EventArgs e);

        /// <summary>
        /// Define the event
        /// </summary>
        public event usbEventsHandler usbEvent;

        /// <summary>
        /// The usb event thrower
        /// </summary>
        /// <param name="e"></param>
        protected virtual void onUsbEvent(EventArgs e)
        {
            if (usbEvent != null)
            {
                Debug.WriteLine("usbGenericHidCommunications:onUsbEvent() -> Throwing a USB event to a listener");
                usbEvent(this, e);
            }
            else Debug.WriteLine("usbGenericHidCommunications:onUsbEvent() -> Attempted to throw a USB event, but no one was listening");
        }

        /// <summary>
        /// isNotificationForTargetDevice - Compares the target devices pathname against the
        /// pathname of the device which caused the event message
        /// </summary>
        private Boolean isNotificationForTargetDevice(Message m)
        {
            Int32 stringSize;

            try
            {
                var devBroadcastDeviceInterface = new User32.DEV_BROADCAST_DEVICEINTERFACE_1();
                var devBroadcastHeader = new User32.DEV_BROADCAST_HDR();

                Marshal.PtrToStructure(m.LParam, devBroadcastHeader);

                // Is the notification event concerning a device interface?
                if ((devBroadcastHeader.dbch_devicetype == User32.DBT_DEVTYP_DEVICEINTERFACE))
                {
                    // Get the device path name of the affected device
                    stringSize = System.Convert.ToInt32((devBroadcastHeader.dbch_size - 32) / 2);
                    devBroadcastDeviceInterface.dbcc_name = new Char[stringSize + 1];
                    Marshal.PtrToStructure(m.LParam, devBroadcastDeviceInterface);
                    var deviceNameString = new String(devBroadcastDeviceInterface.dbcc_name, 0, stringSize);

                    // Compare the device name with our target device's pathname (strings are moved to lower case
                    // using en-US to ensure case insensitivity accross different regions)
                    if ((String.Compare(deviceNameString.ToLower(new System.Globalization.CultureInfo("en-US")),
                        _deviceInformation.devicePathName.ToLower(new System.Globalization.CultureInfo("en-US")), true) == 0)) return true;
                    else return false;
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("HIDCommControl:isNotificationForTargetDevice() -> EXCEPTION: An unknown exception has occured!");
                return false;
            }
            return false;
        }

        /// <summary>
        /// registerForDeviceNotification - registers the window (identified by the windowHandle) for 
        /// device notification messages from Windows
        /// </summary>
        public Boolean registerForDeviceNotifications(IntPtr windowHandle)
        {
            Debug.WriteLine("HIDCommControl:registerForDeviceNotifications() -> Method called");

            // A DEV_BROADCAST_DEVICEINTERFACE header holds information about the request.
            var devBroadcastDeviceInterface = new User32.DEV_BROADCAST_DEVICEINTERFACE();
            var devBroadcastDeviceInterfaceBuffer = IntPtr.Zero;
            var size = 0;

            // Get the required GUID
            System.Guid systemHidGuid = new Guid();
            HID.HidD_GetHidGuid(ref systemHidGuid);

            try
            {
                // Set the parameters in the DEV_BROADCAST_DEVICEINTERFACE structure.
                size = Marshal.SizeOf(devBroadcastDeviceInterface);
                devBroadcastDeviceInterface.dbcc_size = size;
                devBroadcastDeviceInterface.dbcc_devicetype = User32.DBT_DEVTYP_DEVICEINTERFACE;
                devBroadcastDeviceInterface.dbcc_reserved = 0;
                devBroadcastDeviceInterface.dbcc_classguid = systemHidGuid;

                devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);

                // Register for notifications and store the returned handle
                _deviceInformation.deviceNotificationHandle = User32.RegisterDeviceNotification(windowHandle, devBroadcastDeviceInterfaceBuffer, User32.DEVICE_NOTIFY_WINDOW_HANDLE);
                Marshal.PtrToStructure(devBroadcastDeviceInterfaceBuffer, devBroadcastDeviceInterface);

                if ((_deviceInformation.deviceNotificationHandle.ToInt32() == IntPtr.Zero.ToInt32()))
                {
                    Debug.WriteLine("HIDCommControl:registerForDeviceNotifications() -> Notification registration failed");
                    return false;
                }
                else
                {
                    Debug.WriteLine("HIDCommControl:registerForDeviceNotifications() -> Notification registration succeded");
                    return true;
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("HIDCommControl:registerForDeviceNotifications() -> EXCEPTION: An unknown exception has occured!");
            }
            finally
            {
                // Free the memory allocated previously by AllocHGlobal.
                if (devBroadcastDeviceInterfaceBuffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(devBroadcastDeviceInterfaceBuffer);
            }
            return false;
        }

        /// <summary>
        /// handleDeviceNotificationMessages - this method examines any windows devices messages that are
        /// received to check if they are relevant to our target USB device.  If so the method takes the 
        /// correct action dependent on the message type.
        /// </summary>
        /// <param name="m"></param>
        public void handleDeviceNotificationMessages(Message m)
        {
            //Debug.WriteLine("HIDCommControl:handleDeviceNotificationMessages() -> Method called");

            // Make sure this is a device notification
            if (m.Msg != User32.WM_DEVICECHANGE) return;

            Debug.WriteLine("HIDCommControl:handleDeviceNotificationMessages() -> Device notification received");

            try
            {
                switch (m.WParam.ToInt32())
                {
                    // Device attached
                    case User32.DBT_DEVICEARRIVAL:
                        Debug.WriteLine("HIDCommControl:handleDeviceNotificationMessages() -> New device attached");
                        // If our target device is not currently attached, this could be our device, so we attempt to find it.
                        if (!IsDeviceAttached)
                        {
                            findTargetDevice();
                            onUsbEvent(EventArgs.Empty); // Generate an event
                        }
                        break;

                    // Device removed
                    case User32.DBT_DEVICEREMOVECOMPLETE:
                        Debug.WriteLine("HIDCommControl:handleDeviceNotificationMessages() -> A device has been removed");

                        // Was this our target device?  
                        if (isNotificationForTargetDevice(m))
                        {
                            // If so detach the USB device.
                            Debug.WriteLine("HIDCommControl:handleDeviceNotificationMessages() -> The target USB device has been removed - detaching...");
                            DetachUsbDevice();
                            onUsbEvent(EventArgs.Empty); // Generate an event
                        }
                        break;

                    // Other message
                    default:
                        Debug.WriteLine("HIDCommControl:handleDeviceNotificationMessages() -> Unknown notification message");
                        break;
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("HIDCommControl:handleDeviceNotificationMessages() -> EXCEPTION: An unknown exception has occured!");
            }
        }

        /// <summary>
        /// WndProc - This method overrides the WinProc in order to pass notification messages
        /// to the base WndProc
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            handleDeviceNotificationMessages(m);

            // Pass the notification message to the base WndProc
            base.WndProc(ref m);
        }

        #endregion

        #region Discovery

        /// <summary>
        /// Find all devices with the HID GUID attached to the system
        /// </summary>
        /// <remarks>This method searches for all devices that have the correct HID GUID and
        /// returns an array of matching device paths</remarks>
        private bool findHidDevices(ref String[] listOfDevicePathNames, ref int numberOfDevicesFound)
        {
            Debug.WriteLine("HIDCommControl:findHidDevices() -> Method called");
            // Detach the device if it's currently attached
            if (IsDeviceAttached) DetachUsbDevice();

            // Initialise the internal variables required for performing the search
            Int32 bufferSize = 0;
            IntPtr detailDataBuffer = IntPtr.Zero;
            Boolean deviceFound;
            IntPtr deviceInfoSet = new System.IntPtr();
            Boolean lastDevice = false;
            Int32 listIndex = 0;
            SetupApi.SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SetupApi.SP_DEVICE_INTERFACE_DATA();
            Boolean success;

            // Get the required GUID
            System.Guid systemHidGuid = new Guid();
            HID.HidD_GetHidGuid(ref systemHidGuid);
            Debug.WriteLine(string.Format("HIDCommControl:findHidDevices() -> Fetched GUID for HID devices ({0})", systemHidGuid.ToString()));

            try
            {
                // Here we populate a list of plugged-in devices matching our class GUID (DIGCF_PRESENT specifies that the list
                // should only contain devices which are plugged in)
                Debug.WriteLine("HIDCommControl:findHidDevices() -> Using SetupDiGetClassDevs to get all devices with the correct GUID");
                deviceInfoSet = SetupApi.SetupDiGetClassDevs(ref systemHidGuid, IntPtr.Zero, IntPtr.Zero, SetupApi.DIGCF_PRESENT | SetupApi.DIGCF_DEVICEINTERFACE);

                // Reset the deviceFound flag and the memberIndex counter
                deviceFound = false;
                listIndex = 0;

                deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);

                // Look through the retrieved list of class GUIDs looking for a match on our interface GUID
                do
                {
                    //Debug.WriteLine("HIDCommControl:findHidDevices() -> Enumerating devices");
                    success = SetupApi.SetupDiEnumDeviceInterfaces
                    (deviceInfoSet,
                    IntPtr.Zero,
                    ref systemHidGuid,
                    listIndex,
                    ref deviceInterfaceData);

                    if (!success)
                    {
                        //Debug.WriteLine("HIDCommControl:findHidDevices() -> No more devices left - giving up");
                        lastDevice = true;
                    }
                    else
                    {
                        // The target device has been found, now we need to retrieve the device path so we can open
                        // the read and write handles required for USB communication

                        // First call is just to get the required buffer size for the real request
                        success = SetupApi.SetupDiGetDeviceInterfaceDetail
                        (deviceInfoSet,
                        ref deviceInterfaceData,
                        IntPtr.Zero,
                        0,
                        ref bufferSize,
                        IntPtr.Zero);

                        // Allocate some memory for the buffer
                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);
                        Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

                        // Second call gets the detailed data buffer
                        //Debug.WriteLine("HIDCommControl:findHidDevices() -> Getting details of the device");
                        success = SetupApi.SetupDiGetDeviceInterfaceDetail
                            (deviceInfoSet,
                            ref deviceInterfaceData,
                            detailDataBuffer,
                            bufferSize,
                            ref bufferSize,
                            IntPtr.Zero);

                        // Skip over cbsize (4 bytes) to get the address of the devicePathName.
                        IntPtr pDevicePathName = new IntPtr(detailDataBuffer.ToInt32() + 4);

                        // Get the String containing the devicePathName.
                        listOfDevicePathNames[listIndex] = Marshal.PtrToStringAuto(pDevicePathName);

                        //Debug.WriteLine(string.Format("HIDCommControl:findHidDevices() -> Found matching device (memberIndex {0})", memberIndex));
                        deviceFound = true;
                    }
                    listIndex = listIndex + 1;
                }
                while (!((lastDevice == true)));
            }
            catch (Exception)
            {
                // Something went badly wrong... output some debug and return false to indicated device discovery failure
                Debug.WriteLine("HIDCommControl:findHidDevices() -> EXCEPTION: Something went south whilst trying to get devices with matching GUIDs - giving up!");
                return false;
            }
            finally
            {
                // Clean up the unmanaged memory allocations
                if (detailDataBuffer != IntPtr.Zero)
                {
                    // Free the memory allocated previously by AllocHGlobal.
                    Marshal.FreeHGlobal(detailDataBuffer);
                }

                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupApi.SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }

            if (deviceFound)
            {
                Debug.WriteLine(string.Format("HIDCommControl:findHidDevices() -> Found {0} devices with matching GUID", listIndex - 1));
                numberOfDevicesFound = listIndex - 2;
            }
            else Debug.WriteLine("HIDCommControl:findHidDevices() -> No matching devices found");

            return deviceFound;
        }

        /// <summary>
        /// Find the target device based on the VID and PID
        /// </summary>
        /// <remarks>This method used the 'findHidDevices' to fetch a list of attached HID devices
        /// then it searches through the list looking for a HID device with the correct VID and
        /// PID</remarks>
        public void findTargetDevice()
        {
            Debug.WriteLine("HIDCommControl:findTargetDevice() -> Method called");

            bool deviceFoundByGuid = false;
            String[] listOfDevicePathNames = new String[128]; // 128 is the maximum number of USB devices allowed on a single host
            int listIndex = 0;
            bool success = false;
            bool isDeviceDetected;
            int numberOfDevicesFound = 0;

            try
            {
                // Reset the device detection flag
                isDeviceDetected = false;

                // Get all the devices with the correct HID GUID
                deviceFoundByGuid = findHidDevices(ref listOfDevicePathNames, ref numberOfDevicesFound);

                if (deviceFoundByGuid)
                {
                    Debug.WriteLine("HIDCommControl:findTargetDevice() -> Devices with matching GUID found...");
                    listIndex = 0;

                    do
                    {
                        Debug.WriteLine(string.Format("HIDCommControl:findTargetDevice() -> Performing CreateFile to listIndex {0}", listIndex));
                        _deviceInformation.hidHandle = Kernel32.CreateFile(listOfDevicePathNames[listIndex], 0, Kernel32.FILE_SHARE_READ | Kernel32.FILE_SHARE_WRITE, IntPtr.Zero, Kernel32.OPEN_EXISTING, 0, 0);

                        if (!_deviceInformation.hidHandle.IsInvalid)
                        {
                            _deviceInformation.attributes.size = Marshal.SizeOf(_deviceInformation.attributes);
                            success = HID.HidD_GetAttributes(_deviceInformation.hidHandle, ref _deviceInformation.attributes);

                            if (success)
                            {
                                Debug.WriteLine(string.Format("HIDCommControl:findTargetDevice() -> Found device with VID {0}, PID {1} and Version number {2}",
                                    Convert.ToString(_deviceInformation.attributes.vendorId, 16),
                                    Convert.ToString(_deviceInformation.attributes.productId, 16),
                                    Convert.ToString(_deviceInformation.attributes.versionNumber, 16)));

                                //  Do the VID and PID of the device match our target device?
                                if ((_deviceInformation.attributes.vendorId == _deviceInformation.targetVid) &&
                                    (_deviceInformation.attributes.productId == _deviceInformation.targetPid))
                                {
                                    // Matching device found
                                    Debug.WriteLine("HIDCommControl:findTargetDevice() -> Device with matching VID and PID found!");
                                    isDeviceDetected = true;

                                    // Store the device's pathname in the device information
                                    _deviceInformation.devicePathName = listOfDevicePathNames[listIndex];
                                }
                                else
                                {
                                    // Wrong device, close the handle
                                    Debug.WriteLine("HIDCommControl:findTargetDevice() -> Device didn't match... Continuing...");
                                    isDeviceDetected = false;
                                    _deviceInformation.hidHandle.Close();
                                }
                            }
                            else
                            {
                                //  Something went rapidly south...  give up!
                                Debug.WriteLine("HIDCommControl:findTargetDevice() -> Something bad happened - couldn't fill the HIDD_ATTRIBUTES, giving up!");
                                isDeviceDetected = false;
                                _deviceInformation.hidHandle.Close();
                            }
                        }

                        //  Move to the next device, or quit if there are no more devices to examine
                        listIndex++;
                    }
                    while (!((isDeviceDetected || (listIndex == numberOfDevicesFound + 1))));
                }

                // If we found a matching device then we need discover more details about the attached device
                // and then open read and write handles to the device to allow communication
                if (isDeviceDetected)
                {
                    // Query the HID device's capabilities (primarily we are only really interested in the 
                    // input and output report byte lengths as this allows us to validate information sent
                    // to and from the device does not exceed the devices capabilities.
                    //
                    // We could determine the 'type' of HID device here too, but since this class is only
                    // for generic HID communication we don't care...
                    queryDeviceCapabilities();

                    if (success)
                    {
                        // Open the readHandle to the device
                        Debug.WriteLine("HIDCommControl:findTargetDevice() -> Opening a readHandle to the device");
                        _deviceInformation.readHandle = Kernel32.CreateFile(
                            _deviceInformation.devicePathName,
                            Kernel32.GENERIC_READ,
                            Kernel32.FILE_SHARE_READ | Kernel32.FILE_SHARE_WRITE,
                            IntPtr.Zero, Kernel32.OPEN_EXISTING,
                            Kernel32.FILE_FLAG_OVERLAPPED,
                            0);

                        // Did we open the readHandle successfully?
                        if (_deviceInformation.readHandle.IsInvalid)
                        {
                            Debug.WriteLine("HIDCommControl:findTargetDevice() -> Unable to open a readHandle to the device!");
                        }
                        else
                        {
                            Debug.WriteLine("HIDCommControl:findTargetDevice() -> Opening a writeHandle to the device");
                            _deviceInformation.writeHandle = Kernel32.CreateFile(
                                _deviceInformation.devicePathName,
                                Kernel32.GENERIC_WRITE,
                                Kernel32.FILE_SHARE_READ | Kernel32.FILE_SHARE_WRITE,
                                IntPtr.Zero,
                                Kernel32.OPEN_EXISTING,
                                0,
                                0);

                            // Did we open the writeHandle successfully?
                            if (_deviceInformation.writeHandle.IsInvalid)
                            {
                                Debug.WriteLine("HIDCommControl:findTargetDevice() -> Unable to open a writeHandle to the device!");

                                // Attempt to close the writeHandle
                                _deviceInformation.writeHandle.Close();
                            }
                            else
                            {
                                // Device is now discovered and ready for use, update the status
                                IsDeviceAttached = true;
                                onUsbEvent(EventArgs.Empty); // Throw an event
                            }
                        }
                    }
                }
                else
                {
                    //  The device wasn't detected.
                    Debug.WriteLine("HIDCommControl:findTargetDevice() -> No matching device found!");
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("HIDCommControl:findTargetDevice() -> EXCEPTION: Unknown - device not found");
                isDeviceDetected = false;
            }
        }

        /// <summary>
        /// queryDeviceCapabilities - Used the hid.dll function to query the capabilities
        /// of the target device.
        /// </summary>
        private void queryDeviceCapabilities()
        {
            IntPtr preparsedData = new System.IntPtr();
            Int32 result = 0;
            Boolean success = false;

            try
            {
                // Get the preparsed data from the HID driver
                success = HID.HidD_GetPreparsedData(_deviceInformation.hidHandle, ref preparsedData);

                // Get the HID device's capabilities
                result = HID.HidP_GetCaps(preparsedData, ref _deviceInformation.capabilities);
                if ((result != 0))
                {
                    Debug.WriteLine("HIDCommControl:queryDeviceCapabilities() -> Device query results:");
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Usage: {0}",
                        Convert.ToString(_deviceInformation.capabilities.usage, 16)));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Usage Page: {0}",
                        Convert.ToString(_deviceInformation.capabilities.usagePage, 16)));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Input Report Byte Length: {0}",
                        _deviceInformation.capabilities.inputReportByteLength));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Output Report Byte Length: {0}",
                        _deviceInformation.capabilities.outputReportByteLength));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Feature Report Byte Length: {0}",
                        _deviceInformation.capabilities.featureReportByteLength));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Number of Link Collection Nodes: {0}",
                        _deviceInformation.capabilities.numberLinkCollectionNodes));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Number of Input Button Caps: {0}",
                        _deviceInformation.capabilities.numberInputButtonCaps));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Number of Input Value Caps: {0}",
                        _deviceInformation.capabilities.numberInputValueCaps));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Number of Input Data Indices: {0}",
                        _deviceInformation.capabilities.numberInputDataIndices));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Number of Output Button Caps: {0}",
                        _deviceInformation.capabilities.numberOutputButtonCaps));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Number of Output Value Caps: {0}",
                        _deviceInformation.capabilities.numberOutputValueCaps));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Number of Output Data Indices: {0}",
                        _deviceInformation.capabilities.numberOutputDataIndices));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Number of Feature Button Caps: {0}",
                        _deviceInformation.capabilities.numberFeatureButtonCaps));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Number of Feature Value Caps: {0}",
                        _deviceInformation.capabilities.numberFeatureValueCaps));
                    Debug.WriteLine(string.Format("HIDCommControl:queryDeviceCapabilities() ->     Number of Feature Data Indices: {0}",
                        _deviceInformation.capabilities.numberFeatureDataIndices));
                }
            }
            catch (Exception)
            {
                // Something went badly wrong... this shouldn't happen, so we throw an exception
                Debug.WriteLine("HIDCommControl:queryDeviceCapabilities() -> EXECEPTION: An unrecoverable error has occurred!");
                throw;
            }
            finally
            {
                // Free up the memory before finishing
                if (preparsedData != IntPtr.Zero)
                {
                    success = HID.HidD_FreePreparsedData(preparsedData);
                }
            }
        }

        #endregion
    }
}
