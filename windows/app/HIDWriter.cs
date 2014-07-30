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
        const UInt16 HIDMINI_USAGE_PAGE = 0xFF00; //This should match vendor-defined usage page in HID_REPORT_DESCRIPTOR - if we use 2 devices.

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
        public const UInt16 PressureMax = 32767;
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


        //------------- HidD_GetPreparsedData ---------------- // for debug only
        [DllImport("hid.dll", SetLastError = true)]
        static extern Boolean HidD_GetPreparsedData(SafeFileHandle HidDeviceObject, ref IntPtr preParsedData);
        //------------- HidD_GetPreparsedData ---------------- // for debug only


        //------------- HidP_GetCaps -------------------------
        [StructLayout(LayoutKind.Sequential)]
        public struct HIDP_CAPS
        {
            public UInt16 Usage;
            public UInt16 UsagePage;
            public UInt16 InputReportByteLength;
            public UInt16 OutputReportByteLength;
            public UInt16 FeatureReportByteLength;
            public UInt16 Reserved;

            public UInt16 NumberLinkCollectionNodes;

            public UInt16 NumberInputButtonCaps;
            public UInt16 NumberInputValueCaps;
            public UInt16 NumberInputDataIndices;

            public UInt16 NumberOutputButtonCaps;
            public UInt16 NumberOutputValueCaps;
            public UInt16 NumberOutputDataIndices;

            public UInt16 NumberFeatureButtonCaps;
            public UInt16 NumberFeatureValueCaps;
            public UInt16 NumberFeatureDataIndices;
        }
        [DllImport("hid.dll", SetLastError = true)]
        static extern Boolean HidP_GetCaps(IntPtr preParsedData, ref HIDP_CAPS hidCaps);
        //------------- HidP_GetCaps -------------------------
        
        
        //------------- WriteFile ----------------------------
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern Boolean WriteFile(SafeFileHandle handle, IntPtr buffer, UInt32 numBytesToWrite, out UInt32 numBytesWritten, IntPtr NULL);
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
        }

        public void Find()
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
                            IntPtr preparsedDataPointer = new System.IntPtr();
                            if (HidD_GetPreparsedData(file, ref preparsedDataPointer))
                            {
                                HIDP_CAPS hidCaps = new HIDP_CAPS();
                                if (HidP_GetCaps(preparsedDataPointer, ref hidCaps))
                                {
                                    if (hidCaps.UsagePage == HIDMINI_USAGE_PAGE)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                            }                           
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
                bSuccess = WriteFile(file, buffer, bufferSize, out bytesWritten, IntPtr.Zero);
                int error = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }
    }
}
