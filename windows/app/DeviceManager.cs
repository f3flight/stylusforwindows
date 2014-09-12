using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SPenClient
{
    static class DeviceManager
    {
        const int INVALID_HANDLE            = -1;
        const uint DICD_GENERATE_ID         = 0x00000001;
        const uint DIF_REMOVE               = 0x00000005;
        const uint DIF_REGISTERDEVICE       = 0x00000019;
        const uint SPDRP_HARDWAREID         = 0x00000001;
        const uint InstallFlags             = 0x00000001;
        const uint DI_REMOVEDEVICE_GLOBAL   = 0x00000001;

        public static SafeFileHandle file;

        // Guid should match "ClassGuid" from driver's inf file. Otherwise need to to use SetupDiGetINFClass function
        static Guid f3flightGuid = new Guid("a59a8c19-ab59-4161-8f58-09ecad135546");

        // ClassName should match "Class" from driver's inf file. Otherwise need to to use SetupDiGetINFClass function
        static string ClassName = "SPenVirtualDevice";

        // hwid should match "root\..." elements from driver's inf file.
        static string hwid = "root\\spenvhid";

        // This should be equal to the values from Device.h: HIDMINI_PID, HIDMINI_PID, HIDMINI_VERSION
        const int HIDMINI_VAL = 0x0F3F;

        //This should match vendor-defined usage page in HID_REPORT_DESCRIPTOR - if we use 2 devices.
        const UInt16 HIDMINI_USAGE_PAGE = 0xFF00; 

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern IntPtr SetupDiCreateDeviceInfoList(ref Guid ClassGuid, IntPtr hwndParent);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern Boolean SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        //------------- CloseHandle --------------------------
        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern Boolean CloseHandle(SafeFileHandle handle);
        //------------- CloseHandle --------------------------

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

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVICE_INTERFACE_DATA
        {
            public uint cbSize;
            public Guid InterfaceClassGuid;
            public uint Flags;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SP_CLASSINSTALL_HEADER
        {
            public uint cbSize;
            public uint InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SP_REMOVEDEVICE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public uint Scope;
            public uint HwProfile;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Boolean SetupDiGetDeviceInterfaceDetail(
           IntPtr hDevInfo,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           IntPtr NULL,
           UInt32 Zero,
           out UInt32 requiredSize,
           out SP_DEVINFO_DATA devInfoData
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Boolean SetupDiGetDeviceInterfaceDetail(
           IntPtr hDevInfo,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
           UInt32 deviceInterfaceDetailDataSize,
           out UInt32 requiredSize,
           out SP_DEVINFO_DATA devInfoData
        );

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Boolean SetupDiEnumDeviceInterfaces(
           IntPtr hDevInfo, IntPtr devInfo,
           ref Guid interfaceClassGuid, UInt32 memberIndex,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static extern Boolean SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            out SP_DEVINFO_DATA DeviceInfoData
        );

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
            uint PropertyBufferSize
        );

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern Boolean SetupDiSetClassInstallParams(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            ref SP_CLASSINSTALL_HEADER ClassInstallParams,
            uint ClassInstallParamsSize
        );

        //------------- HidD_GetHidGuid ----------------------
        [DllImport("hid.dll", EntryPoint = "HidD_GetHidGuid", SetLastError = true)]
        public static extern void HidD_GetHidGuid(out Guid hidGuid);
        //------------- HidD_GetHidGuid ----------------------

        //------------- SetupDiGetClassDevs ------------------
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, int Flags);
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(IntPtr NULL, IntPtr Enumerator, IntPtr hwndParent, int Flags);
        
        const int DIGCF_PRESENT = 0x00000002;
        const int DIGCF_INTERFACEDEVICE = 0x00000010;
        const int DIGCF_ALLCLASSES = 0x00000004;
        //------------- SetupDiGetClassDevs ------------------
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
            //System.Windows.Forms.MessageBox.Show(InfPath, "SPenClient debug - InfPath");
            Boolean bSuccess = false;
            IntPtr DeviceInfoSet = SetupDiCreateDeviceInfoList(ref f3flightGuid, IntPtr.Zero);
            SP_DEVINFO_DATA DeviceInfoData = new SP_DEVINFO_DATA();
            DeviceInfoData.cbSize = (uint)Marshal.SizeOf(DeviceInfoData);
            //if (DeviceInfoSet != ) {
            bSuccess = SetupDiCreateDeviceInfo(DeviceInfoSet, ClassName, ref f3flightGuid, IntPtr.Zero, IntPtr.Zero, DICD_GENERATE_ID, ref DeviceInfoData);
            //System.Windows.Forms.MessageBox.Show("SetupDiCreateDeviceInfo result is " + bSuccess, "SPenClient debug");
            bSuccess = SetupDiSetDeviceRegistryProperty(DeviceInfoSet, ref DeviceInfoData, SPDRP_HARDWAREID, hwid, (uint)hwid.Length);
            //System.Windows.Forms.MessageBox.Show("SetupDiSetDeviceRegistryProperty result is " + bSuccess, "SPenClient debug");

            //uint reqSize = 0;
            //uint propRegDataType = 0;
            //uint bufferSize = 50;
            //byte[] propBuffer = new byte[50];
            //StringBuilder pBuf = new StringBuilder(50);
            //bSuccess = SetupDiGetDeviceRegistryProperty(DeviceInfoSet, ref DeviceInfoData, SPDRP_HARDWAREID, out propRegDataType, propBuffer, bufferSize, out reqSize);
            bSuccess = SetupDiCallClassInstaller(DIF_REGISTERDEVICE, DeviceInfoSet, ref DeviceInfoData);
            //System.Windows.Forms.MessageBox.Show("SetupDiCallClassInstaller result is " + bSuccess, "SPenClient debug");

            //bSuccess = SetupDiCallClassInstaller(DIF_REMOVE, DeviceInfoSet, ref DeviceInfoData);
            bSuccess = SetupDiDestroyDeviceInfoList(DeviceInfoSet);
            bool bRebootRequired = false;
            bSuccess = UpdateDriverForPlugAndPlayDevices(IntPtr.Zero, hwid, InfPath, InstallFlags, ref bRebootRequired);
            //System.Windows.Forms.MessageBox.Show("UpdateDriverForPlugAndPlayDevices result is " + bSuccess, "SPenClient debug");

            return true;
        }

        public static bool Found()
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
                SP_DEVINFO_DATA devInfoData = new SP_DEVINFO_DATA();
                devInfoData.cbSize = (uint)Marshal.SizeOf(devInfoData);
                result = SetupDiGetDeviceInterfaceDetail(hardwareDeviceInfo, ref deviceInterfaceData, IntPtr.Zero, bufferSize, out requiredSize, out devInfoData);
                error = Marshal.GetLastWin32Error();
                didd.DevicePath = new string(char.MinValue, 256);
                uint nBytes = (uint)didd.DevicePath.Length;
                result = SetupDiGetDeviceInterfaceDetail(hardwareDeviceInfo, ref deviceInterfaceData, ref didd, nBytes, out requiredSize, out devInfoData);
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
                                        SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                }

                i++;
            }
            SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
            file = null;
            return false;
        }

        public static bool removeDevice()
        {
            bool result = false;
            int error = 0;
            result = CloseHandle(file);
            error = Marshal.GetLastWin32Error();
            SP_REMOVEDEVICE_PARAMS rmdParams = new SP_REMOVEDEVICE_PARAMS();
            SP_CLASSINSTALL_HEADER classInstallHeader = new SP_CLASSINSTALL_HEADER();
            rmdParams.ClassInstallHeader.cbSize = (uint)Marshal.SizeOf(classInstallHeader);
            rmdParams.ClassInstallHeader.InstallFunction = DIF_REMOVE;
            rmdParams.Scope = DI_REMOVEDEVICE_GLOBAL;
            rmdParams.HwProfile = 0;
            Guid hidGuid;
            HidD_GetHidGuid(out hidGuid);
            IntPtr hardwareDeviceInfo = SetupDiGetClassDevs(ref f3flightGuid, IntPtr.Zero, IntPtr.Zero, 0);
            SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
            deviceInterfaceData.cbSize = (uint)Marshal.SizeOf(deviceInterfaceData);
            UInt32 i = 0;
            SP_DEVINFO_DATA devInfoData = new SP_DEVINFO_DATA();
            devInfoData.cbSize = (uint)Marshal.SizeOf(devInfoData);
            while (SetupDiEnumDeviceInfo(hardwareDeviceInfo, i, out devInfoData))
            {
            //    SP_DEVICE_INTERFACE_DETAIL_DATA didd = new SP_DEVICE_INTERFACE_DETAIL_DATA();
            //    if (IntPtr.Size == 8) // for 64 bit operating systems
            //        didd.cbSize = 8;
            //    else
            //        didd.cbSize = 4 + (uint)Marshal.SystemDefaultCharSize;
            //    UInt32 bufferSize = 0;
            //    UInt32 requiredSize = 0;
            //    SP_DEVINFO_DATA devInfoData = new SP_DEVINFO_DATA();
            //    devInfoData.cbSize = (uint)Marshal.SizeOf(devInfoData);
            //    result = SetupDiGetDeviceInterfaceDetail(hardwareDeviceInfo, ref deviceInterfaceData, IntPtr.Zero, bufferSize, out requiredSize, out devInfoData);
            //    error = Marshal.GetLastWin32Error();
            //    didd.DevicePath = new string(char.MinValue, 256);
            //    uint nBytes = (uint)didd.DevicePath.Length;
            //    result = SetupDiGetDeviceInterfaceDetail(hardwareDeviceInfo, ref deviceInterfaceData, ref didd, nBytes, out requiredSize, out devInfoData);
            //    if (didd.DevicePath.Contains(ClassName.ToLower()))
            //    {
            //        result = SetupDiSetClassInstallParams(hardwareDeviceInfo, ref devInfoData, ref rmdParams.ClassInstallHeader, (uint)Marshal.SizeOf(rmdParams));
            //        error = Marshal.GetLastWin32Error();
            //        result = SetupDiCallClassInstaller(DIF_REMOVE, hardwareDeviceInfo, ref devInfoData);
            //        error = Marshal.GetLastWin32Error();
            //    }
                i++;
            }
            devInfoData.DevInst = 4276;
            result = SetupDiSetClassInstallParams(hardwareDeviceInfo, ref devInfoData, ref rmdParams.ClassInstallHeader, (uint)Marshal.SizeOf(rmdParams));
            error = Marshal.GetLastWin32Error();
            result = SetupDiCallClassInstaller(DIF_REMOVE, hardwareDeviceInfo, ref devInfoData);
            error = Marshal.GetLastWin32Error();
            SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
            return true;
        }
    }
}
