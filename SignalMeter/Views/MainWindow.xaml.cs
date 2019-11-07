using SignalMeter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SignalMeter
{
    public partial class MainWindow : Window
    {
        //static ComPort mainPort;
        public MainWindow()
        {
            InitializeComponent();


            //Closing += MainWindow_Closing;
            //    mainPort = new ComPort();

            //    #region COM_INIT and COM_EVENTS
            //    mainPort.NewDataRecived     += rdata => AddDataInListBox(rdata, true);
            //    mainPort.NewDataTransfered  += tdata => AddDataInListBox(tdata, false);
            //    //mainPort.NewDataRecived += rdata => Manager(rdata);
            //    ComPortBox.Items.Add(Settings.Com);
            //    ComPortBox.SelectedIndex = 0;
            //    mainPort.InitComPort(Settings.Com, Settings.BaudRate);
            //    #endregion
            //}

            //private void AddDataInListBox(byte[] data, bool rxOrTx)
            //{
            //    //if (!LogListBox.CheckAccess())
            //    //{
            //    //    LogListBox.Dispatcher.Invoke(new Action<byte[], bool>(AddDataInListBox), data, rxOrTx);
            //    //}
            //    //else
            //    //{
            //        string bytes = string.Join(" ", data.Select(i => i.ToString("X2")));
            //        if (rxOrTx) LogListBox.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")} DEVICE->PC: " + bytes);
            //        else LogListBox.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")} PC->DEVICE: " + bytes);

            //    //}
            //}

            //#region ComPortBox_Events
            //private void ComPortBox_Opened(object sender, EventArgs e)
            //{
            //    ComPortBox.Items.Clear();
            //    ComPortBox.SelectedItem = mainPort.CurrentPort;

            //    foreach (string s in mainPort.GetPorts())
            //    {
            //        ComPortBox.Items.Add($"{s}");
            //    }
            //}

            //private void ComPortBox_Closed(object sender, EventArgs e)
            //{
            //    if (ComPortBox.SelectedItem == null) return;
            //    Settings.Com = ComPortBox.SelectedItem.ToString();
            //    mainPort.InitComPort(Settings.Com, Settings.BaudRate);
            //}
            //#endregion

            //private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) //действия при закрытии приложения
            //{
            //    if (MessageBoxResult.No == MessageBox.Show("Вы действительно хотите закрыть программу?", "Закрытие клиента", MessageBoxButton.YesNo, MessageBoxImage.Warning))
            //    {
            //        e.Cancel = true;
            //        return;
            //    }
            //    mainPort.Close();
            //    App.Current.Shutdown();
        }
    }
}
