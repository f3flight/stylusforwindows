using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace StylusForWindowsClient
{
    class HIDWriter
    {

        //------------- Common -------------------------------
        const int INVALID_HANDLE = -1;
        

        [StructLayout(LayoutKind.Sequential, Pack=1, Size=11)] 
        public struct SPEN_REPORT
        {
	        public byte ReportID;
            public byte Switches;
            public UInt16 X;
            public UInt16 Y;
            public UInt16 Pressure;
            public byte XTilt;
            public byte YTilt;
            public byte Twist;
            //public byte Reserved;
        };
        public const byte SwitchTip     = 1;     // These const bytes should be used with logical OR to fill Switches byte; Bit-field imitation.
        public const byte SwitchBarrel  = 2;
        public const byte SwitchInvert  = 4;
        public const byte SwitchEraser  = 8;
        public const byte SwitchInRange = 16;
        public const byte SwitchFingerDown = 32; // not used in driver, used by app only
        public const byte SwitchFingerUp = 64; // not used in driver, used by app only
        public const UInt16 PressureMax = 32767;
        //------------- Common -------------------------------


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
        public static extern Boolean SetupDiEnumDeviceInterfaces(
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
        public static extern Boolean SetupDiGetDeviceInterfaceDetail(
           IntPtr hDevInfo,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           IntPtr NULL,
           UInt32 Zero,
           out UInt32 requiredSize,
           IntPtr deviceInfoData
        );

        //------------- SetupDiGetDeviceInterfaceDetail ------


        


 
        
        
        //------------- WriteFile ----------------------------
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern Boolean WriteFile(SafeFileHandle handle, IntPtr buffer, UInt32 numBytesToWrite, out UInt32 numBytesWritten, IntPtr NULL);
        //------------- WriteFile ----------------------------


        //------------- SetupDiDestroyDeviceInfoList ---------
        [DllImport("setupapi.dll", SetLastError = true)]
        static extern Boolean SetupDiDestroyDeviceInfoList(IntPtr hardwareDeviceInfo);
        //------------- SetupDiDestroyDeviceInfoList ---------


      


        //---
        //[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //static extern Boolean SetupDiEnumDeviceInfo(IntPtr hDevInfo, UInt32 index, ref SP_DEVINFO_DATA DeviceInfoData);
        //---

        public SPEN_REPORT spenReport = new SPEN_REPORT();

        public HIDWriter()
        {          
        }

        public bool Write()
        {
            bool bSuccess = false;
                spenReport.ReportID = 1;
                //spenReport.Switches = SwitchInRange;
                //spenReport.X = 20000;
                //spenReport.Y = 1000;

                int reportSize = Marshal.SizeOf(spenReport);
                //byte[] buffer = new byte[reportSize];
                //buffer[0] = 1;
                //buffer[1] = 32;
                //buffer[2] = 32;
                //buffer[3] = 78;
                //buffer[4] = 232;
                //buffer[5] = 3;
                //Marshal.Copy(spenReport, 0, buffer, reportSize);
                IntPtr buffer = Marshal.AllocHGlobal(reportSize);         
                //GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                //Marshal.StructureToPtr(spenReport, handle.AddrOfPinnedObject(), false);
                Marshal.StructureToPtr(spenReport, buffer, false);
                //handle.Free();
                //SPEN_REPORT buffer = new SPEN_REPORT();
                //buffer.ReportID = spenReport.ReportID;
                //buffer.Switches = spenReport.Switches;
                //buffer.X = spenReport.X;
                //buffer.Y = spenReport.Y;
                UInt32 bytesWritten = 0;
                UInt32 bufferSize = (uint)Marshal.SizeOf(spenReport);
                bSuccess = WriteFile(DeviceManager.file, buffer, bufferSize, out bytesWritten, IntPtr.Zero);
                int error = Marshal.GetLastWin32Error();
            return bSuccess;
        }
    }
}
