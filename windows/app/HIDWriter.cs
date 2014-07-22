using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SPenClient
{
    class HIDWriter
    {

        //------------- Common -------------------------------
        const int INVALID_HANDLE = -1;
        const int HIDMINI_VAL = 0x0F3F; // This should be equal to the values from Device.h: HIDMINI_PID, HIDMINI_PID, HIDMINI_VERSION
        [StructLayout(LayoutKind.Sequential, Pack=1, Size=11)] 
        public struct SPEN_REPORT
        {
	        public byte ReportID;
            public byte Switches;
            public Int16 X;
            public Int16 Y;
            public byte Pressure;
            public byte XTilt;
            public byte YTilt;
            public byte Twist;
            //public byte Reserved;
        };
        const byte SwitchTip = 1;     // These const bytes should be used with logical OR to fill Switches byte; Bit-field imitation.
        const byte SwitchBarrel = 2;
        const byte SwitchInvert = 4;
        const byte SwitchEraser = 8;
        const byte SwitchInRange = 32;
        //------------- Common -------------------------------


        //------------- HidD_GetHidGuid ----------------------
        [DllImport("hid.dll", EntryPoint = "HidD_GetHidGuid", SetLastError = true)]
        static extern void HidD_GetHidGuid(out Guid hidGuid);
        //------------- HidD_GetHidGuid ----------------------


        //------------- SetupDiGetClassDevs ------------------
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, int Flags);
        const int DIGCF_PRESENT = 0x00000002;
        const int DIGCF_INTERFACEDEVICE = 0x00000010;
        //------------- SetupDiGetClassDevs ------------------


        //------------- SetupDiEnumDeviceInterfaces ----------
        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public uint cbSize;
            public Guid InterfaceClassGuid;
            public uint Flags;
            public IntPtr Reserved;
        }
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Boolean SetupDiEnumDeviceInterfaces(
           IntPtr hDevInfo, IntPtr devInfo,
           ref Guid interfaceClassGuid, UInt32 memberIndex,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
        );
        //------------- SetupDiEnumDeviceInterfaces ----------


        //------------- SetupDiGetDeviceInterfaceDetail ------
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Boolean SetupDiGetDeviceInterfaceDetail(
           IntPtr hDevInfo,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
           UInt32 deviceInterfaceDetailDataSize,
           out UInt32 requiredSize,
           IntPtr deviceInfoData
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Boolean SetupDiGetDeviceInterfaceDetail(
           IntPtr hDevInfo,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           IntPtr NULL,
           UInt32 Zero,
           out UInt32 requiredSize,
           IntPtr deviceInfoData
        );
        //------------- SetupDiGetDeviceInterfaceDetail ------


        //------------- CreateFile ---------------------------
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern SafeFileHandle CreateFile(
             [MarshalAs(UnmanagedType.LPTStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);
        //------------- CreateFile ---------------------------


        //------------- HidD_GetAttributes -------------------
        [StructLayout(LayoutKind.Sequential)]
        public struct HIDD_ATTRIBUTES
        {
            public Int32 Size;
            public Int16 VendorID;
            public Int16 ProductID;
            public Int16 VersionNumber;
        }
        [DllImport("hid.dll", SetLastError = true)]
        static extern Boolean HidD_GetAttributes(SafeFileHandle HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);
        //------------- HidD_GetAttributes -------------------


        //------------- WriteFile ----------------------------
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern Boolean WriteFile(SafeFileHandle handle, ref SPEN_REPORT spenReport,
          UInt32 numBytesToWrite, out UInt32 numBytesWritten, IntPtr NULL);
        //------------- WriteFile ----------------------------


        //------------- SetupDiDestroyDeviceInfoList ---------
        [DllImport("setupapi.dll", SetLastError = true)]
        static extern Boolean SetupDiDestroyDeviceInfoList(IntPtr hardwareDeviceInfo);
        //------------- SetupDiDestroyDeviceInfoList ---------


        private SafeFileHandle file;
        public bool found = false;
        public SPEN_REPORT spenReport = new SPEN_REPORT();

        public HIDWriter()
        {
            Guid hidGuid;
            HidD_GetHidGuid(out hidGuid);
            IntPtr hardwareDeviceInfo = SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_INTERFACEDEVICE);
            SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
            deviceInterfaceData.cbSize = (uint)Marshal.SizeOf(deviceInterfaceData);
            UInt32 i = 0;
            while (SetupDiEnumDeviceInterfaces(hardwareDeviceInfo, IntPtr.Zero, ref hidGuid, i, ref deviceInterfaceData))
            {
                SP_DEVICE_INTERFACE_DETAIL_DATA didd = new SP_DEVICE_INTERFACE_DETAIL_DATA();
                if (IntPtr.Size == 8) // for 64 bit operating systems
                    didd.cbSize = 8; 
                else
                    didd.cbSize = 4 + (uint)Marshal.SystemDefaultCharSize;
                UInt32 bufferSize = 0;
                UInt32 requiredSize = 0;
                bool result;
                int error;
                result = SetupDiGetDeviceInterfaceDetail(hardwareDeviceInfo, ref deviceInterfaceData, IntPtr.Zero, bufferSize, out requiredSize, IntPtr.Zero);
                error = Marshal.GetLastWin32Error();
                didd.DevicePath = new string(char.MinValue, 256);
                uint nBytes = (uint)didd.DevicePath.Length;
                result = SetupDiGetDeviceInterfaceDetail(hardwareDeviceInfo, ref deviceInterfaceData, ref didd, nBytes, out requiredSize, IntPtr.Zero);
                if (result)
                {
                    file = CreateFile(didd.DevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                    HIDD_ATTRIBUTES ha = new HIDD_ATTRIBUTES();
                    result = HidD_GetAttributes(file, ref ha);
                    if (result)
                    {
                        if (ha.VendorID == HIDMINI_VAL & ha.ProductID == HIDMINI_VAL & ha.VersionNumber == HIDMINI_VAL)
                        {
                            found = true;
                            Write();
                            break;
                        }
                    }

                }

                i++;
            }
            SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
        }

        public bool Write()
        {
            bool bSuccess = false;
            if (found)
            {
                spenReport.ReportID = 1;
                spenReport.Switches = SwitchInRange;
                spenReport.X = 20000;
                spenReport.Y = 1000;

                SPEN_REPORT buffer = new SPEN_REPORT();
                buffer.ReportID = spenReport.ReportID;
                buffer.Switches = spenReport.Switches;
                buffer.X = spenReport.X;
                buffer.Y = spenReport.Y;
                UInt32 bytesWritten = 0;
                UInt32 bufferSize = (uint)Marshal.SizeOf(buffer);
                bSuccess = WriteFile(file, ref buffer, bufferSize, out bytesWritten, IntPtr.Zero);
                int error = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }
    }
}
