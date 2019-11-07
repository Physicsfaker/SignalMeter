using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SignalMeter.Models
{
    class Signal : INotifyPropertyChanged
    {
        private byte[] data;
        private string hexString;
        private int dataValue;


        public Signal(byte[] bytes)
        {
            data = bytes;
            hexString = ($"{DateTime.Now.ToString("HH:mm:ss")} Device->PC: " + data.Select(i => i.ToString("X2")));
            dataValue = 2;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
