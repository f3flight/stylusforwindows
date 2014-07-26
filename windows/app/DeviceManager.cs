using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SPenClient
{
    static class DeviceManager
    {
        const int INVALID_HANDLE = -1;
        const UInt16 DICD_GENERATE_ID = 1;
        static Guid f3flightGuid = new Guid("3F3F3F3F-3F3F-3F3F-3F3F-3F3F3F3F3F3F");

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern IntPtr SetupDiCreateDeviceInfoList(ref Guid ClassGuid, IntPtr hwndParent);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern Boolean SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern Boolean SetupDiCreateDeviceInfo(
            IntPtr DeviceInfoSet,
            char[] ClassName,
            ref Guid ClassGUID,
            IntPtr DeviceDescription,
            IntPtr hwndParent,
            UInt16 CreationFlags,
            IntPtr DeviceInfoData
        );

        public static bool installDevice()
        {
            IntPtr DeviceInfoSet = SetupDiCreateDeviceInfoList(ref f3flightGuid, IntPtr.Zero);
            SP_DEVINFO_DATA DeviceInfoData = new SP_DEVINFO_DATA();
            DeviceInfoData.cbSize = (uint)Marshal.SizeOf(DeviceInfoData);
            //if (DeviceInfoSet != ) {
            SetupDiDestroyDeviceInfoList(DeviceInfoSet);
            return true;
        }

        public static bool removeDevice()
        {
            return true;
        }
    }
}
