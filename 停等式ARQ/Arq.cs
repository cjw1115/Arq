using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace 停等式ARQ
{
    public class Arq:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyChanged=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyChanged));
        }

        public int Sn { get; set; }
        public int RnFromReviever { get; set; }
        public int Rn { get; set; }
        public int SnFromSender { get; set; }

        public int TimeOut { get; set; } = 200;
        public bool isTimeOut { get; set; } = false;

        private Queue<char> _sendQueue = new Queue<char>();
        public Queue<char> SendQueue
        {
            get { return _sendQueue; }
            set { _sendQueue = value;OnPropertyChanged(); }
        }
        public char? CurrentSend { get; set; }

        private Queue<char> _recieveQueue = new Queue<char>();
        public Queue<char> RecieveQueue
        {
            get { return _recieveQueue; }
            set
            {
                _recieveQueue = value;
                OnPropertyChanged();
            }
        }
        public System.Timers.Timer SendTimer { get; set;}

        public Pipe _sendPipe;
        public Pipe _recievePipe;
        public Action<Queue<char>> Update;
        public Action<string> Log;
        public Arq(Pipe sendPipe, Pipe recievePipe,Action<Queue<char>> update,Action<string> log)
        {
            _sendPipe = sendPipe;
            _recievePipe = recievePipe;
            Update = update;
            Log = log;

            SendTimer = new System.Timers.Timer(TimeOut);
            SendTimer.AutoReset = false;
            SendTimer.Elapsed += SendTimer_Elapsed;


            if (_sendPipe != null)
            {
                ThreadPool.QueueUserWorkItem(SendHandler);
            }
            if (_recievePipe != null)
            {
                ThreadPool.QueueUserWorkItem(ReceiveHandler);
            }
            
        }

        public void SendHandler(object o)
        {
            while (true)
            {
                Thread.Sleep(50);
                if (SendQueue.Count > 0)
                {
                    var next = SendQueue.Dequeue();
                    CurrentSend = next;
                    SendTimer.Start();
                    while(!(Send(Sn, next)))
                    {
                        if (isTimeOut)
                        {
                            SendTimer.Start();
                            isTimeOut = false;
                        }
                    }
                }
                
            }
        }
        public bool Send(int sn, char? content)
        {
            //
            //发送逻辑
            _sendPipe.SendNumber = sn;
            _sendPipe.Content = content.Value;

            Log($"Send:Sn:{sn}    Content:{content}");

            Thread.Sleep(50);
            RnFromReviever = _sendPipe.RecieveNumber;//获取管道上传输的Number
            //
            while (RnFromReviever <= Sn&& !isTimeOut)
            {
                Thread.Sleep(50);
                RnFromReviever = _sendPipe.RecieveNumber;//获取管道上传输的Number
            }
            if (isTimeOut)
            {
                return false;
            }
            Sn++;
            CurrentSend = null;
            SendTimer.Stop();
            Log($"ACK:Rn:{RnFromReviever}    Content:{content}{Environment.NewLine}");
            return true;
        }
        private void SendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            isTimeOut = true;
            Log($"TimeOut:Sn:{Sn}   Content:{CurrentSend}");
        }

        public void ReceiveHandler(object o)
        {
            while (true)
            {

                Thread.Sleep(50);

                int sn = _recievePipe.SendNumber;
                char? content = _recievePipe.Content;
                Receive(sn, content);
            }
        }
        public bool Receive(int sn, char? content)
        {
            if (Rn == sn&&content!=null)
            {
                Rn++;
                _recievePipe.RecieveNumber = Rn;
                _recievePipe.Content = null;
                RecieveQueue.Enqueue(content.Value);

                Update(RecieveQueue);
                Log($"Recieve:Sn:{sn}    Content:{content}  Rn:{Rn}");
                return true;
            }
            else if(Rn>sn && content != null)
            {
                _recievePipe.RecieveNumber = Rn;
                return true;
            }
            return false;
        }
    }
}