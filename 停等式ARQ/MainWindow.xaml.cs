using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using 停等式ARQ.Converter;

namespace 停等式ARQ
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
     

        private Arq arqSender;
        public Arq ArqSender
        {
            get { return arqSender; }
            set {
                arqSender = value;
               }
        }
        private Arq arqReciever;
        public Arq ArqReciever
        {
            get { return arqReciever; }
            set { arqReciever = value;
                }
        } 
        private Pipe Pipe  = new Pipe();
        DispatcherTimer timer;
        //public VM VM { get; set; } = new VM();
        public MainWindow()
        {
            InitializeComponent();

            ArqSender = new Arq(Pipe, null, VM.UpdateRecieveQueue,VM.UpdateLog);
            ArqReciever = new Arq(null, Pipe,VM.UpdateRecieveQueue, VM.UpdateLog);

            timer = new DispatcherTimer(DispatcherPriority.Send);
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(150);
            timer.Stop();

            this.Loaded += MainWindow_Loaded;
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in tbSend.Text)
            {
                ArqSender.SendQueue.Enqueue(item);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Pipe.RecieveNumber = -1;
        }
        private void btnJam_Checked(object sender, RoutedEventArgs e)
        {
            if (btnJam.IsChecked== true)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timer.Interval = TimeSpan.FromMilliseconds(e.NewValue + 50);
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var scroll=GetFirstElement<ScrollViewer>(tbLog);
            tbScrollViewer = scroll;
        }
        ScrollViewer tbScrollViewer = null;
        public T GetFirstElement<T>(DependencyObject father)where T: FrameworkElement
        {
            Queue<FrameworkElement> queue = new Queue<FrameworkElement>();
            queue.Enqueue(father as FrameworkElement);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                if(item is T)
                {
                    return (T)item;
                }
                else
                {
                    var count = VisualTreeHelper.GetChildrenCount(father);
                    for (int i = 0; i < count; i++)
                    {
                        var child = VisualTreeHelper.GetChild(item, i) as FrameworkElement;
                        queue.Enqueue(child);
                    }
                }
            }
            return null;
        }
        private void tbLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbScrollViewer.ScrollToVerticalOffset(tbScrollViewer.ScrollableHeight);


        }
    }
    public class VM:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string _sendQueue;
        public string SendQueue
        {
            get { return _sendQueue; }
            set { _sendQueue = value;
                OnPropertyChanged();
            } }

        private string _recieveQueue;
        public string RecieveQueue
        {
            get { return _recieveQueue; }
            set { _recieveQueue = value;
                OnPropertyChanged();
            }
        }

        public void UpdateRecieveQueue(Queue<char> queue)
        {
            Queue<char> source = queue;
            if (source == null)
                return;
            string goalValue = "";
            foreach (var item in source.ToArray())
            {
                goalValue += item;
            }
            RecieveQueue = goalValue;
        }

        private string  _log;

        public string  Log
        {
            get { return _log; }
            set { _log = value;OnPropertyChanged(); }
        }
        private StringBuilder _sb = new StringBuilder();
        public void UpdateLog(string log)
        {
            _sb.AppendLine(log);
            Log = _sb.ToString();
        }
    }
    public class Pipe
    {
        public int SendNumber { get; set; }
        public int RecieveNumber { get; set; }
        public char? Content { get; set; } = null;
    }
}
