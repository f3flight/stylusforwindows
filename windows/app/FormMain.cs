using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace SPenClient
{

    public partial class FormMain : Form
    {
        public class PenData
        {
            public PenData(FormMain form)
            {
                this.form = form;
            }

            public byte switches;
            public const byte SwitchFinger = 32;
            public float x;
            public float y;
            public float pressure;
            public int action;
            public string type;
            public uint index;
            public string up;

            public float _x;
            public float _y;
            public float _pressure;
            public int _action;
            public string _type;
            public int _index;
            public string _up;

            //private string[] dta;

            public bool isNew =true;
            private FormMain form;

            //public void SetData()
            //{
            //    if (isNew)
            //    {
            //        isNew = false;
            //        SetBackup();
            //    }

            //    if (!type.Equals(_type))
            //    {
            //        this._index = this.index;
            //    }

            //    if (this.index >= this._index)
            //    {

            //        //if (!type.Equals(_type))
            //        //{
            //        //    if (type.Equals("hover") && _type.Equals("pen"))
            //        //    {
            //        //        mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
            //        //    }

            //        //    if (type.Equals("pen") && _type.Equals("hover"))
            //        //    {
            //        //        mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            //        //    }
            //        //}


            //        if (pressure <= 0 && Math.Min(x, y) <= 0 & type.Equals("hover") || type.Equals("finger") && up.Equals("up"))
            //        {
            //            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
            //            isNew = true;
            //            RestoreBackup();
            //            return;
            //        }

            //        hwr.spenReport.X = (UInt16)(x * 20);
            //        hwr.spenReport.Y = (UInt16)(y * 20);
            //        hwr.spenReport.Pressure = (pressure <= 1) ? (UInt16)(pressure * HIDWriter.PressureMax) : HIDWriter.PressureMax;
            //        if (type.Equals("pen")){
            //            hwr.spenReport.Switches = HIDWriter.SwitchInRange | HIDWriter.SwitchTip;
            //            //hwr.spenReport.Switches = 255; //debug - all switches ON! may cause problems.
            //            hwr.Write();
            //        }                     
            //        else if (type.Equals("hover"))
            //        {
            //            hwr.spenReport.Switches = HIDWriter.SwitchInRange;
            //            hwr.Write();
            //        }
            //        else
            //        {
            //            if (hwr.spenReport.Switches != 0)
            //            {
            //                hwr.spenReport.Switches = 0;
            //                hwr.Write();
            //            }
            //            Cursor.Position = new System.Drawing.Point(this.GetX, this.GetY);
                    
            //        }                       

            //        //if (form.WindowState == FormWindowState.Normal)
            //        //{
            //        //    form.labelx.Text = x.ToString();
            //        //    form.labely.Text = y.ToString();
            //        //    form.labelp.Text = pressure.ToString();
            //        //    form.labela.Text = action.ToString();
            //        //    form.labelt.Text = type.ToString();
            //        //    form.labeli.Text = index.ToString();
            //        //    form.labelu.Text = up.ToString();
            //        //}

            //        SetBackup();
            //    }
            //}

            //public void LoadData(object raw)
            //{
            //    string sep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            //    string str = ((string)raw).Replace(",", sep).Replace(".", sep);
            //    this.dta = (str).Split(new char[] { '|' });

            //    this.x = float.Parse(this.dta[1]);
            //    this.y = float.Parse(this.dta[2]);
            //    this.pressure = float.Parse(this.dta[3]);
            //    this.action = int.Parse(this.dta[4]);
            //    this.type = this.dta[5];
            //    this.index = int.Parse(this.dta[6]);
            //    this.up = this.dta[7];
            //}

            public void LoadByteData(byte[] receivedData)
            {
                this.switches = receivedData[0];
                this.x = BitConverter.ToSingle(receivedData, 1);
                this.y = BitConverter.ToSingle(receivedData, 5);
                this.pressure = BitConverter.ToSingle(receivedData, 9);
                this.index = BitConverter.ToUInt32(receivedData, 13);
            }

            //public void SetBackup()
            //{
            //    this._x = this.x;
            //    this._y = this.y;
            //    this._pressure = this.pressure;
            //    this._action = this.action;
            //    this._type = this.type;
            //    this._index = this.index;
            //    this._up = this.up;
            //}

            //public void RestoreBackup()
            //{
            //    this.x = this._x;
            //    this.y = this._y;
            //    this.pressure = this._pressure;
            //    this.action = this._action;
            //    this.type = this._type;
            //    this.index = this._index;
            //    this.up = this._up;
            //}

            public int GetX { 
                get {
                    return Cursor.Position.X + (int)(x - _x);
                } 
            }

            public int GetY { 
                get {
                    return Cursor.Position.Y + (int)(y - _y);
                } 
            }
        }

        private PenData pen;
        BackgroundWorker bw;
        System.IO.MemoryStream ms = new System.IO.MemoryStream(13);

        static HIDWriter hwr;

        public FormMain()
        {
            InitializeComponent();
            checkProcessArchMatch();
            checkOSVersion();
            hwr = new HIDWriter();
            hwr.Find();
            if (!hwr.found)
            {
                installCert();
                DeviceManager.installDevice();
                hwr.Find();
                if (!hwr.found)
                {
                    MessageBox.Show("Could not find S-Pen Virtual Device. Did you agree to install the driver?", "SPenClient error - no device");
                    Environment.Exit(1);
                }
            }
            pen = new PenData(this);
            init(12333);
        }

        private void init(int port)
        {
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerAsync(port);
        }

        private void installCert()
        {
            Stream CertStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"SPenClient.f3flight-code-signing.cer");
            MemoryStream ms = new MemoryStream();
            CertStream.CopyTo(ms);
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            X509Certificate2 f3flightCert = new X509Certificate2(ms.ToArray());
            store.Add(f3flightCert);
            store.Close();
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState.GetType() == typeof(string))
            {
                //this.pen.LoadData(e.UserState);
                //this.pen.SetData();
            }     
            else
            {
                this.pen.LoadByteData((byte[])e.UserState);
                hwr.spenReport.Index = this.pen.index;
                hwr.spenReport.X = (UInt16)(this.pen.x * 20);
                hwr.spenReport.Y = (UInt16)(this.pen.y * 20);
                hwr.spenReport.Pressure = (this.pen.pressure <= 1) ? (UInt16)(this.pen.pressure * HIDWriter.PressureMax) : HIDWriter.PressureMax;
                if ((this.pen.switches & HIDWriter.SwitchInRange) == HIDWriter.SwitchInRange)
                {
                    hwr.spenReport.Switches = this.pen.switches;
                    hwr.Write();
                }
                else
                {
                    Cursor.Position = new System.Drawing.Point((int)this.pen.x, (int)this.pen.y);
                }
            }
                
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            UdpClient udpClient = new UdpClient((int)e.Argument);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, (int)e.Argument);
            BackgroundWorker worker = sender as BackgroundWorker;
            do
            {
                byte[] receiveBytes = udpClient.Receive(ref groupEP);
                if (receiveBytes[0] == '|')
                {
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    worker.ReportProgress(0, returnData);
                }
                else
                {
                    worker.ReportProgress(0, receiveBytes);
                }
            } while (!worker.CancellationPending);
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        private void buttonChangePort_Click(object sender, EventArgs e)
        {
            bw.CancelAsync();
            bw.Dispose();
            init(int.Parse(textBox1.Text));
        }

        private void checkProcessArchMatch()
        {
            if (Environment.Is64BitOperatingSystem & !Environment.Is64BitProcess)
            {
                MessageBox.Show("Your system is x64, please run x64-version of the app!", "SPenClient error - wrong architecture");
                Environment.Exit(1);
            }
        }

        private void checkOSVersion()
        {
            if (Environment.OSVersion.Version.Major != 6)
            {
                MessageBox.Show("Your OS is not supported, sorry.", "SPenClient error - untested OS");
            }
            else if (Environment.OSVersion.Version.Minor != 1 && Environment.OSVersion.Version.Minor != 3)
            {
                MessageBox.Show("Your OS is not Windows 7 or Windows 8.1, sorry.", "SPenClient error - untested OS");
            }
        }

        private void buttonUninstall_Click(object sender, EventArgs e)
        {
            bw.CancelAsync();
            bw.Dispose();
            System.Threading.Thread.Sleep(1000);
            //Environment.Exit(0);
        }
    }
}