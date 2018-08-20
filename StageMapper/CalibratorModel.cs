using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Windows;

namespace StageMapper
{
    public class CalibratorModel : IDisposable
    {
        SerialPort arduino, printer;

        public Position CurrentPosition;

        public enum Buttons { Button1, Button2 }

        public bool IsReady
        {
            get
            {
                if (arduino == null || printer == null)
                    return false;
                return arduino.IsOpen && printer.IsOpen;
            }
        }

        public void printerWriteLine(string line)
        {
#if DEBUG
            Debugger.Debug.WriteDebug(line);
            return;
#endif
            if (!isConnected)
                return;
            Debugger.Debug.WriteDebug(line);
            printer.WriteLine(line);
        }


        public bool IsPushed1
        {
            get
            {
                if (!IsReady) return true;
                arduino.Write("1");
                var r = arduino.ReadLine();
                if (r == null) return true;

                bool b;

                if (r.Contains("1"))
                    b = false;
                else
                    b = true;
                Debugger.Debug.WriteDebug("IsPushed1: " + b);
                return b;
            }
        }

        public bool IsPushed2
        {
            get
            {
                if (!IsReady) return true;
                arduino.Write("2");
                var r = arduino.ReadLine();
                if (r == null) return true;

                bool b;

                if (r.Contains("1"))
                    b = false;
                else
                    b = true;
                Debugger.Debug.WriteDebug("IsPushed2: " + b);
                return b;
            }
        }

        public bool isConnected
        {
            get
            {
                if (printer == null || !printer.IsOpen)
                    return false;
                if (arduino == null || !arduino.IsOpen)
                    return false;
                return true;
            }
        }

        public CalibratorModel()
        {
            CurrentPosition = new Position(0, 0, 0);
            Debugger.Debug.WriteDebug("indul");
            var s = getValidPort("CH340");
            if (s == null)
                return;
            arduino = new SerialPort(s);
            arduino.Open();
            Debugger.Debug.WriteDebug("arduino ok");

            s = getValidPort("Arduino Mega");
            if (s == null)
                return;
            Debugger.Debug.WriteDebug("printer port: " + s);

            printer = new SerialPort(s, 250000);
            printer.DataReceived += Printer_DataReceived;
            printer.NewLine = "\n";
            printer.DtrEnable = true;
            printer.RtsEnable = true;
            printer.Open();
            System.Threading.Thread.Sleep(2000);

            //printer.BaudRate = 115200;
            //printer.DataBits = 8;
            //printer.StopBits = StopBits.One;
            //printer.Parity = Parity.None;
            //printer.WriteTimeout = 100;
            //printer.Handshake = Handshake.None;
            //printer.ReadBufferSize = 8192;
            //printer.ReadTimeout = 2000;
            //printer.RtsEnable = false;
            //printer.DtrEnable = false;

            Debugger.Debug.WriteDebug("printer ok");



            if (!printer.IsOpen)
                return;

            Debugger.Debug.WriteDebug("Parancs küldés");
            printerWriteLine("T0");
            printerWriteLine("M92 X78.74 Y78.74 Z533.33 E836.00");
            printerWriteLine("M203 X500.00 Y500.00 Z5.00 E25.00");
            printerWriteLine("M201 X9000 Y9000 Z100 E10000");
            printerWriteLine("M204 S4000.00 T3000.00");
            printerWriteLine("M205 S0.00 T0.00 B20000 X20.00 Z0.40 E5.00");
            printerWriteLine("M206 X0.00 Y0.00 Z0.00");
            printerWriteLine("M301 P22.20 I1.08 D114.00");
            printerWriteLine("G28 X0");
            printerWriteLine("G28 Y0");
            System.Threading.Thread.Sleep(2000);

        }
        
        

        private void Printer_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var msg = printer.ReadLine();
            Debugger.Debug.WriteDebug("Printer msg: " + msg);
        }


        public void GoTo(double x, double y)
        {
            int delay = (int)Math.Round((CurrentPosition.GetDelay(feedrate:5000,x: x, y: y)+1)*1000);
            printerWriteLine("G1 X" + x.ToString("F6", CultureInfo.InvariantCulture) + " Y" + y.ToString("F6", CultureInfo.InvariantCulture) + " F5000");
            Debugger.Debug.WriteDebug("delay msg: " + delay);
            CurrentPosition.X = x;
            CurrentPosition.Y = y;
            Thread.Sleep(delay);
        }

        public void GoTo(double z)
        {
            int delay = (int)Math.Round((CurrentPosition.GetDelay(feedrate: 300, z:z) + 1) * 1000);
            printerWriteLine("G1 Z" + z.ToString("F6", CultureInfo.InvariantCulture) + " F300");
            Debugger.Debug.WriteDebug("delay msg: " + delay);
            CurrentPosition.Z = z;
            Thread.Sleep(delay);
        }


        public double? getZMacro(double z, Buttons button)
        {
            return getZ(z, 0.1, 150, 40, button);
        }

        public double? getZMicro(double z, Buttons button)
        {
         
           return getZ(z, 0.0125, 100, 200, button);//0.0125
        }

      

        private double? getZ(double z, double step, int delay, int count, Buttons button)
        {
#if DEBUG
            return 1;
#endif

            for (int i = 0; i < count; i++)
            {
                var zk = (z - i * step);
                printerWriteLine("G1 Z" + zk.ToString("F6", CultureInfo.InvariantCulture) + " F300");
                System.Threading.Thread.Sleep(delay);
                switch (button)
                {
                    case Buttons.Button1:
                        if (IsPushed1)
                            return zk;
                        break;
                    case Buttons.Button2:
                        if (IsPushed2)
                            return zk;
                        break;
                }
            }
            return null;
        }

        internal void Init()
        {
            printerWriteLine("M115");
            printerWriteLine("G28 X0");
            printerWriteLine("G28 Y0");
            printerWriteLine("G28 Z0");
            printerWriteLine("G90");
            //printerWriteLine("G1 Z35 F1800");


        }




        private string getValidPort2(String name)
        {
            using (var searcher = new ManagementObjectSearcher
               ("SELECT * FROM WIN32_SerialPort"))
            {
                string[] portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in portnames
                             join p in ports on n equals p["DeviceID"].ToString()
                             select n + " - " + p["Caption"]).ToList();

                foreach (string s in tList)
                {
                    Console.WriteLine(s);
                }

                foreach (var port in ports)
                {
                    if (port["Caption"].ToString().Contains(name))
                        return port["DeviceID"].ToString();
                }

            }
            return null;
        }

        private string getValidPort(String name)
        {
            using (var searcher = new ManagementObjectSearcher
               ("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                string[] portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in portnames
                             join p in ports on n equals p["DeviceID"].ToString()
                             select n + " - " + p["Caption"]).ToList();

                foreach (string s in tList)
                {
                    Console.WriteLine(s);
                }

                foreach (var port in ports)
                {
                    if (port["Caption"].ToString().Contains(name))
                        return portnames.FirstOrDefault(s => port["Caption"].ToString().Contains(s));
                }

            }
            return null;
        }

        public void Dispose()
        {
            if (arduino != null)
                arduino.Dispose();
            if (printer != null)
                printer.Dispose();
        }
    }
}
