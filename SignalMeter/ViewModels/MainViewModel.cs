using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
//using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace SignalMeter.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        double start = 0;
        bool proces = false;

        private int _clicks;
        public int Clicks { get => _clicks; set { _clicks = value; OnPropertyChanged(); } }



        public ComPort _mainPort;
        public ComPort MainPort { get => _mainPort; set { _mainPort = value; OnPropertyChanged(); } }
        public string[] AvailablePorts { get => ComPort.GetPorts(); }
        public string PortNumber { get => _mainPort.PortName; set { _mainPort.PortName = value; OnPropertyChanged(); } }


        private string _buttonContent = "New metering";
        public string ButtonContent { get => _buttonContent; set { _buttonContent = value; OnPropertyChanged(); } }

        public byte[] ComData { get => _mainPort.NewData; }

        private ObservableCollection<string> _listBoxContent = new ObservableCollection<string>();
        public ObservableCollection<string> ListBoxContent { get => _listBoxContent; set { _listBoxContent = value; } }


        private PlotModel _myModel;
        public PlotModel MyModel { get => _myModel; set { _myModel = value; OnPropertyChanged(); } }

        private ObservableCollection<DataPoint> _points;
        public ObservableCollection<DataPoint> Points { get => _points; set { _points = value; OnPropertyChanged(); } }


        public MainViewModel()
        {
            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
            ButtonCommand = new DelegateCommand(ButtonCommandUpdate);

            MainPort = ComPort.initPort();
            MainPort.PropertyChanged += ModelPropertyChanged;

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
                IntervalType = DateTimeIntervalType.Milliseconds
            };
            MyModel.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis
            {
                Title = "mV",
                Position = AxisPosition.Left,
                MinorStep = 2,
                //Maximum = maxValue + maxValue / 10, // to have the sd/upper level lines show with some margin
                //Minimum = minValue - minValue / 6,
                Angle = 90,
                AxisTickToLabelDistance = 3,
                MaximumPadding = 50,
                MinimumPadding = 50,
            };
            MyModel.Axes.Add(yAxis);
            MyModel.Series.Add(lineserie);

            ListBoxContent = new ObservableCollection<string>();

        }

        private void StartMetering()
        {
            Task Worker = Task.Factory.StartNew(() =>
            {
                int delay = 333;
                byte[] request = { 0xAB, 0x18, 0x00, 0x00, 0xE8 };
                start = DateTimeAxis.ToDouble(DateTime.Now);
                
                double time = 0;//------------
                Random rand = new Random();//----------
                while (proces)
                {
                    Clicks++;
                    Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now) - start, rand.Next(0, 15))); //-------------
                    MyModel.InvalidatePlot(true);//---
                    time += delay;//-------------

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

        public ICommand ButtonCommand { get; private set; }
        private void ButtonCommandUpdate(object obj)
        {
            if (proces) { proces = false; ButtonContent = "New metering"; }
            else
            {
                Points.Clear();
                proces = true;
                MyModel.InvalidatePlot(true);
                ButtonContent = "Stop";
                StartMetering();
            }
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
                            double data = 2.5 / 16777215 * (graph[5] | graph[6] << 8 | graph[7] << 16 | graph[8] << 24);
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


    }
}
