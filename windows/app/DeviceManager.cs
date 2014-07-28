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
        const uint DICD_GENERATE_ID   = 0x00000001;
        const uint DIF_REMOVE         = 0x00000005;
        const uint DIF_REGISTERDEVICE = 0x00000019;
        const uint SPDRP_HARDWAREID   = 0x00000001;
        const uint InstallFlags       = 0x00000001;

        // Guid should match "ClassGuid" from driver's inf file. Otherwise need to to use SetupDiGetINFClass function
        static Guid f3flightGuid = new Guid("a59a8c19-ab59-4161-8f58-09ecad135546");

        // ClassName should match "Class" from driver's inf file. Otherwise need to to use SetupDiGetINFClass function
        static string ClassName = "SPenVirtualDevice";

        // hwid should match "root\..." elements from driver's inf file.
        static string hwid = "root\\spenvhid";

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

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern Boolean SetupDiCreateDeviceInfo(
            IntPtr DeviceInfoSet,
            string ClassName,
            ref Guid ClassGUID,
            IntPtr DeviceDescription,
            IntPtr hwndParent,
            uint CreationFlags,
            ref SP_DEVINFO_DATA DeviceInfoData
        );

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern Boolean SetupDiSetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property,
            string PropertyBuffer,
            ushort PropertyBufferSize
        );

        //[DllImport("setupapi.dll", SetLastError = true)]
        //static extern Boolean SetupDiGetDeviceRegistryProperty(
        //    IntPtr DeviceInfoSet,
        //    ref SP_DEVINFO_DATA DeviceInfoData,
        //    uint Property,
        //    out uint PropertyRegDataType,
        //    byte[] PropertyBuffer,
        //    uint PropertyBufferSize,
        //    out uint RequiredSize
        //);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern Boolean SetupDiCallClassInstaller(uint InstallFunction, IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("newdev.dll", SetLastError = true)]
        static extern Boolean UpdateDriverForPlugAndPlayDevices(
            IntPtr hwndParent,
            string HardwareId,
            string FullInfPath,
            uint InstallFlags,
            ref bool bRebootRequired
        );

        public static bool installDevice()
        {

            //Version tv = new Version()
            //OperatingSystem test = new OperatingSystem(PlatformID.Win32NT, Ver)
            //if (Environment.OSVersion == OperatingSystem.Version.)
            string InfPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath;
            InfPath = System.IO.Path.GetDirectoryName(InfPath);
            string arch = (Environment.Is64BitOperatingSystem) ? "x64" : "x86";
            string version = (Environment.OSVersion.Version.ToString().Contains("6.1.")) ? "Win7" : "Win8.1";
            InfPath = InfPath + "\\driver\\" + arch + "\\" + version + "\\spenvhid.Inf";
            Boolean bSuccess = false;
            IntPtr DeviceInfoSet = SetupDiCreateDeviceInfoList(ref f3flightGuid, IntPtr.Zero);
            SP_DEVINFO_DATA DeviceInfoData = new SP_DEVINFO_DATA();
            DeviceInfoData.cbSize = (uint)Marshal.SizeOf(DeviceInfoData);
            //if (DeviceInfoSet != ) {
            bSuccess = SetupDiCreateDeviceInfo(DeviceInfoSet, ClassName, ref f3flightGuid, IntPtr.Zero, IntPtr.Zero, DICD_GENERATE_ID, ref DeviceInfoData);
            bSuccess = SetupDiSetDeviceRegistryProperty(DeviceInfoSet, ref DeviceInfoData, SPDRP_HARDWAREID, hwid, (ushort)hwid.Length);
            //uint reqSize = 0;
            //uint propRegDataType = 0;
            //uint bufferSize = 50;
            //byte[] propBuffer = new byte[50];
            //StringBuilder pBuf = new StringBuilder(50);
            //bSuccess = SetupDiGetDeviceRegistryProperty(DeviceInfoSet, ref DeviceInfoData, SPDRP_HARDWAREID, out propRegDataType, propBuffer, bufferSize, out reqSize);
            bSuccess = SetupDiCallClassInstaller(DIF_REGISTERDEVICE, DeviceInfoSet, ref DeviceInfoData);
            //bSuccess = SetupDiCallClassInstaller(DIF_REMOVE, DeviceInfoSet, ref DeviceInfoData);
            bSuccess = SetupDiDestroyDeviceInfoList(DeviceInfoSet);
            bool bRebootRequired = false;
            bSuccess = UpdateDriverForPlugAndPlayDevices(IntPtr.Zero, hwid, InfPath, InstallFlags, ref bRebootRequired);
            return true;
        }

        public static bool removeDevice()
        {
            return true;
        }
    }
}
