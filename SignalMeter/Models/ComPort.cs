using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SignalMeter
{
    public class ComPort : INotifyPropertyChanged
    {
        private SerialPort currentSerial { get; set; } //хранит ком порт
        private string portName = "COM3";
        private int BaudRate = 115200;
        public string PortName { get { return portName; } set { portName = value; InitComPort(); } }
       
        private byte[] _newData;
        public byte[] NewData { get => _newData; private set { _newData = value; OnPropertyChanged(); } }

        public ComPort()
        {
            currentSerial = new SerialPort();
            currentSerial.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        private void InitComPort() //задает номер порта и скорость
        {
            var serial = currentSerial.PortName;

            if (currentSerial.IsOpen) currentSerial.Close();

            currentSerial.PortName = PortName;
            currentSerial.BaudRate = BaudRate;
            currentSerial.Parity = Parity.None;
            currentSerial.DataBits = 8;
            currentSerial.StopBits = StopBits.One;
            currentSerial.Handshake = Handshake.None;
            currentSerial.RtsEnable = true;
            currentSerial.ReadTimeout = 500;
            currentSerial.WriteTimeout = 500;

            try { currentSerial.Open(); }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"Доступ к порту '{CurrentPort}' закрыт.");
                currentSerial.PortName = serial;
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show($"Порт '{CurrentPort}' не существует.");
            }
        }

        public static ComPort initPort()
        {
            var result = new ComPort();
            result.InitComPort();
            return result;
        }

        private async void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            if (!IsOpen) return;
            await Task.Run(() =>
            {
                byte[] bytes = new byte[currentSerial.BytesToRead];
                while (currentSerial.BytesToRead > 0) currentSerial.Read(bytes, 0, currentSerial.BytesToRead);
                NewData = bytes;
            });
        }

        public string CurrentPort => currentSerial.PortName.ToString(); //получить имя текущего порта
        static public string[] GetPorts() => SerialPort.GetPortNames(); // получить список всех доступных портов
        public bool IsOpen => currentSerial.IsOpen;


        public void Close() => currentSerial.Close();
        public void Write(byte[] bytes)
        {
            if (!IsOpen) return;
            try { currentSerial.Write(bytes, 0, bytes.Length); }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"Доступ к порту '{CurrentPort}' закрыт.");
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show($"Порт '{CurrentPort}' не существует.");
            }
            //NewDataTransfered(bytes);g
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
