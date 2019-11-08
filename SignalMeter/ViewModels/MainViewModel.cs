using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace SignalMeter.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        static double start = 0;
        static bool proces = false;


        public ComPort _mainPort;
        public ComPort MainPort { get => _mainPort; set { _mainPort = value; OnPropertyChanged(); } }


        #region Port_ComboBox
        public string[] AvailablePorts { get => ComPort.GetPorts(); }
        public string PortNumber { get => _mainPort.PortName; set { _mainPort.PortName = value; OnPropertyChanged(); } }
        #endregion

        #region LogList
        public byte[] ComData { get => _mainPort.NewData; }
        private ObservableCollection<string> _listBoxContent = new ObservableCollection<string>();
        public ObservableCollection<string> ListBoxContent { get => _listBoxContent; set { _listBoxContent = value; } }
        #endregion

        #region Graph_Prop
        private PlotModel _myModel;
        public PlotModel MyModel { get => _myModel; set { _myModel = value; OnPropertyChanged(); } }

        private ObservableCollection<DataPoint> _points;
        public ObservableCollection<DataPoint> Points { get => _points; set { _points = value; OnPropertyChanged(); } }
        #endregion

        public MainViewModel()
        {
            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
            StartStopButtonCommand = new DelegateCommand(StartStopButtonCommandUpdate);
            AutoScrollButtonCommand = new DelegateCommand(AutoScrollButtonCommandUpdate);

            MainPort = ComPort.initPort();
            MainPort.PropertyChanged += ModelPropertyChanged;

            #region Graph
            MyModel = new PlotModel { Title = "Voltage graph" };
            Points = new ObservableCollection<DataPoint>();
           
            LineSeries lineserie = new LineSeries()
            {
                ItemsSource = Points,
                DataFieldX = "Time",
                DataFieldY = "mV",
                StrokeThickness = 2,
                MarkerSize = 0,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Blue,
                MarkerType = MarkerType.None
            };

            DateTimeAxis xAxis = new DateTimeAxis
            {
                Title = "Time",
                Position = AxisPosition.Bottom,
                StringFormat = "HH:mm:ss",
                IntervalLength = 60,
                MinorIntervalType = DateTimeIntervalType.Milliseconds,
                IntervalType = DateTimeIntervalType.Milliseconds,
                AbsoluteMinimum = 0,
            };
            MyModel.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis
            {
                Title = "mV",
                Position = AxisPosition.Left,
                StringFormat = "0.00",
                AbsoluteMinimum = 0,
                AbsoluteMaximum = 25000,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };

            MyModel.Axes.Add(yAxis);
            MyModel.Series.Add(lineserie);
            MyModel.ZoomAllAxes(15);
            #endregion

            ListBoxContent = new ObservableCollection<string>();
        }

        #region Auto_Scroll_Button
        public ICommand AutoScrollButtonCommand { get; private set; }
        private void AutoScrollButtonCommandUpdate(object obj) => MyModel.ResetAllAxes();
        #endregion

        #region Start_Stop_Button
        private string _startStopbuttonContent = "New metering";
        public string StartStopButtonContent { get => _startStopbuttonContent; set { _startStopbuttonContent = value; OnPropertyChanged(); } }
        public ICommand StartStopButtonCommand { get; private set; }
        private void StartStopButtonCommandUpdate(object obj)
        {
            if (proces) { proces = false; StartStopButtonContent = "New metering"; }
            else
            {
                Points.Clear();
                proces = true;
                MyModel.InvalidatePlot(true);
                StartStopButtonContent = "Stop";
                StartMetering();
            }
        }
        #endregion

        private void StartMetering() //send request to device after some time 
        {
            Task Worker = Task.Factory.StartNew(() =>
            {
                int delay = 1000/3; //  three times per second
                byte[] request = { 0xAB, 0x18, 0x00, 0x00, 0xE8 };
                start = DateTimeAxis.ToDouble(DateTime.Now);

                //double time = 0;//------------
                //Random rand = new Random();//----------
                while (proces)
                {
                    //Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now) - start, rand.Next(5000,25000)*rand.NextDouble())); //-------------
                    //MyModel.InvalidatePlot(true);//---
                    //time += delay;//-------------

                    MainPort.Write(request);
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (_listBoxContent.Count > 10) _listBoxContent.RemoveAt(0);
                        ListBoxContent.Add(String.Join("", $"{DateTime.Now.ToString("HH:mm:ss")} PC->Device: " + BitConverter.ToString(request)));
                    });
                    Task.Delay(delay).Wait();
                }
            });
        }
        private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NewData")
            {
                App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                {
                    byte[] graph;
                    if (ComData == null) return;
                    graph = ComData;

                    if (_listBoxContent.Count > 10) _listBoxContent.RemoveAt(0);
                    if (graph.Length == 10)
                    {
                        if (graph[0] == 0xAB && graph[1] == 0x18 && graph[2] == 0x00 && graph[3] == 0x05 &&
                            graph[9] == (byte)(0x00 - graph[1] - graph[2] - graph[3] - graph[4] - graph[5] - graph[6] - graph[7] - graph[8]))
                        {
                            double data = 2.5 / 16777215 * 1000* (graph[5] | graph[6] << 8 | graph[7] << 16 | graph[8] << 24);
                            ListBoxContent.Add(String.Join("", $"{DateTime.Now.ToString("HH:mm:ss")} Device->PC: " + BitConverter.ToString(ComData)));
                            if (Points.Count >= 2700) Points.RemoveAt(0); //2700 
                            Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now) - start, data));
                            MyModel.InvalidatePlot(true);
                            return;
                        }
                    }
                    ListBoxContent.Add(String.Join("", $"{DateTime.Now.ToString("HH:mm:ss")} Device->PC: " + BitConverter.ToString(ComData) + " - 'WRONG RESPONSE'"));
                });
            }
        }

        #region Other
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) //действия при закрытии приложения
        {
            if (MessageBoxResult.No == MessageBox.Show("Вы действительно хотите закрыть программу?", "Закрытие программы", MessageBoxButton.YesNo, MessageBoxImage.Warning))
            {
                e.Cancel = true;
                return;
            }
            proces = false;
            MainPort.Close();
            Application.Current.Shutdown();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
