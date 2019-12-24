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
using System.Windows.Shapes;
using System.IO.Ports;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;
using System.Timers;
using System.Windows.Threading;

namespace WristSymbol
{
    /// <summary>
    /// ExpTraining.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ExpMain : Window
    {
        private BackgroundWorker worker;
        TextWriter tw;
        int trial;
        int trialEnd;
        String playedLetters;
        int confidenceLevel = -1;   // 1: Low, 2: Middle, 3: High

        String[] letterSet = { "a", "c", "f", "j", "l", "r", "t", "v",
                               "a", "c", "f", "j", "l", "r", "t", "v",
                               "a", "c", "f", "j", "l", "r", "t", "v",
                               "a", "c", "f", "j", "l", "r", "t", "v",
                               "a", "c", "f", "j", "l", "r", "t", "v"};
        String[] playSet = { "a0","a1","a2","a3","a4","a5","a6","a7","a8","a9","a10","a11","a12","a13","a14","a15","a16","a17",
                             "b0","b1","b2","b3","b4","b5","b6","b7","b8","b9","b10","b11","b12","b13","b14","b15","b16","b17",
                             "a0","a1","a2","a3","a4","a5","a6","a7","a8","a9","a10","a11","a12","a13","a14","a15","a16","a17",
                             "b0","b1","b2","b3","b4","b5","b6","b7","b8","b9","b10","b11","b12","b13","b14","b15","b16","b17",
                             "a0","a1","a2","a3","a4","a5","a6","a7","a8","a9","a10","a11","a12","a13","a14","a15","a16","a17",
                             "b0","b1","b2","b3","b4","b5","b6","b7","b8","b9","b10","b11","b12","b13","b14","b15","b16","b17",};
        enum pattern { top_left, top, top_right, right, bottom_right, bottom, bottom_left, left };
        bool patternAnswering;
        bool confAnswered;

        System.IO.Ports.SerialPort serialPort1 = new SerialPort();
        String logID;

        int baseModality;   // 0 : LRA, 1 : ERM
        int duration;   // ms
        
        FixedSizedQueueInt recentResults = new FixedSizedQueueInt(24);
        double recentAccuracy = 0;
        long startTimestamp;
        long playstamp;
        long playendstamp;
        long enterstamp;

        String height_up_str = "100";
        String height_down_str = "121";

        bool keyboardEvent = true;

        /*
        System.Timers.Timer timer;
        private double secondsToWait;   // ms
        private DateTime startTime;
        */

        public void setExpMain(SerialPort port, String s1)
        {
            serialPort1 = port;
            logID = s1;
            baseModality = 0;
            duration = 500;
            
            tw = new StreamWriter(logID + "_exp" + ".csv", true);
            tw.WriteLine("trial#,pattern,playstamp,playendstamp");
        }

        internal string invokeLabel2
        {
            get { return debugLabel2.Content.ToString(); }
            set { Dispatcher.Invoke(new Action(() => { debugLabel2.Content = value; })); }
        }
        internal string invokeLabel3
        {
            get { return debugLabel3.Content.ToString(); }
            set { Dispatcher.Invoke(new Action(() => { debugLabel3.Content = value; })); }
        }

        public void workBackground(String text)
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerAsync(argument: text);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            String text = (String)e.Argument;
            patternGenerate(text);
            playendstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;

        }

        public ExpMain()
        {
            InitializeComponent();
           
            startTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            trial = 1;
            trialEnd = 40;
            trialLabel.Content = trial + " / " + trialEnd;
            patternAnswering = false;
            
            
            Random rnd = new Random();
            letterSet = letterSet.OrderBy(x => rnd.Next()).ToArray();
            letterSet = letterSet.OrderBy(x => rnd.Next()).ToArray();
            //TcpData tcp = new TcpData(this);
            //Thread t1 = new Thread(new ThreadStart(tcp.connect));

            //t1.Start();
            /*
            timer = new System.Timers.Timer();
            timer.Interval = 100;
            timer.Elapsed += new ElapsedEventHandler(timer_Tick);
            */
        }

        public void stimulation(int tactorNum)
        {

            string str = "";

            if (baseModality == 0)
                str = "ev";

            serialPort1.WriteLine(str + tactorNum.ToString());
            Thread.Sleep(duration);
            serialPort1.WriteLine(str.Remove(str.Length - 1) + "s" + tactorNum.ToString());
        }
        
        public void patternGenerate(String text)
        {
            edgeWritePattern(text);
            
            return;
        }
        
        public void edgeVibStimulation(int[] tactorNums)
        {
            int n = tactorNums.Length;
            int i;
            for (i = 0; i < n; i++)
            {
                stimulation(tactorNums[i]);
                /*
                if (i < n - 1)
                    Thread.Sleep(inLetterGap);
                 */

            }
        }

        public void edgeWritePattern(String character)
        {
            int[] arr = null;
            switch (character.ToUpper())
            {
                case "A":
                    arr = new int[] { 3, 2, 4 };
                    break;
                case "C":
                    arr = new int[] { 2, 3, 4 };
                    break;
                case "F":
                    arr = new int[] { 2, 1, 3 };
                    break;
                case "L":
                    arr = new int[] { 1, 3, 4 };
                    break;
                case "J":
                    arr = new int[] { 2, 4, 3 };
                    break;
                case "R":
                    arr = new int[] { 3, 1, 2 };
                    break;
                case "T":
                    arr = new int[] { 1, 2, 4 };
                    break;
                case "V":
                    arr = new int[] { 1, 3, 2 };
                    break;
            }

            edgeVibStimulation(arr);
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (patternAnswering == false)
            {
                
                answer1.Content = "";
                

                

                answer1.Content = letterSet[trial - 1];
                Thread.Sleep(400);
                workBackground(letterSet[trial-1]);

                //debugLabel1.Content = "playedLetters: " + playedLetters;
                
                playstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;
                patternAnswering = true;
                
                //debugLabel1.Content = "answer1.Content : " +answer1.Content.ToString() ;
            }
        }

        private void ButtonAnwer_Click(object sender, RoutedEventArgs e)
        {
            if (patternAnswering == true)
            {
                patternAnswering = false;
                
                
                String a = answer1.Content.ToString();
                
                confAnswered = false;

                int sum = 0;
                int i;
                for (i = 0; i < recentResults.Size(); i++)
                {
                    sum = sum + recentResults.Get(i);
                }
                recentAccuracy = (double)sum / recentResults.Size();

                

                enterstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;
                tw.WriteLine(trial.ToString() + "," + a + "," + playstamp.ToString() + "," + playendstamp.ToString());
                if (trial == trialEnd)
                    this.Close();

                trial++;
                trialLabel.Content = trial + " / " + trialEnd;
            }
        }

        /*
        public void breaktime()
        {
            ButtonPlay.Visibility = Visibility.Hidden;
            keyboardEvent = false;

            timer.Start();
            startTime = DateTime.Now;
        }

        public void timer_Tick(object sender, EventArgs e)
        {
            double elapsedSeconds = (double)(DateTime.Now - startTime).TotalMilliseconds;
            double remainingSeconds = secondsToWait - elapsedSeconds;

            //TimeSpan t1 = TimeSpan.FromSeconds(remainingSeconds);
            //string str = t1.ToString(@"mm\:ss");
            //Dispatcher.Invoke((Action)delegate () { clockLabel.Content = str; });

            if (remainingSeconds <= 0)
            {
                Dispatcher.Invoke((Action)delegate () { ButtonPlay.Visibility = Visibility.Visible; });
                keyboardEvent = true;
                //Dispatcher.Invoke((Action)delegate () { clockLabel.Content =""; });
                // run your function
                timer.Stop();
            }
        }
        */

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tw.Close();
        }
        
    }

    class TcpData
    {
        ExpMain target;
        StylusPointCollection pts = new StylusPointCollection();

        public TcpData(ExpMain window)
        {
            target = window;
        }

        public void connect()
        {
            byte[] buff = new byte[20];

            TcpListener listener = new TcpListener(IPAddress.Any, 5000); 
            listener.Start();
            TcpClient tc = listener.AcceptTcpClient();  //accept client request for connection and assign TcpClient object
            NetworkStream stream = tc.GetStream();  //Get networkstream from TcpClient object
            target.invokeLabel2 = String.Format("TCP Connection Succeed!");

            int m1 = 0;
            int m2 = 0;
            int m3 = 0;
            int mode = 0;

            Boolean doubleTapped = false;

            //Repeatedly read TCP data with stream.Read();
            int nbytes = 0;
            while (true)
            {
                if ((nbytes = stream.Read(buff, 0, buff.Length)) != 0)
                {
                    byte[] m1_bytes = { buff[0], buff[1], buff[2], buff[3] };
                    byte[] m2_bytes = { buff[4], buff[5], buff[6], buff[7] };
                    byte[] m3_bytes = { buff[8], buff[9], buff[10], buff[11] };
                    byte[] mode_bytes = { buff[12], buff[13], buff[14], buff[15] };
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(m1_bytes);
                        Array.Reverse(m2_bytes);
                        Array.Reverse(m3_bytes);
                        Array.Reverse(mode_bytes);
                    }
                    m1 = BitConverter.ToInt32(m1_bytes, 0);
                    m2 = BitConverter.ToInt32(m2_bytes, 0);
                    m3 = BitConverter.ToInt32(m3_bytes, 0);
                    mode = BitConverter.ToInt32(mode_bytes, 0);

                    target.invokeLabel3 = String.Format("m1 : {0}, m2 : {1}, m3: {2}, mode: {3}", m1, m2, m3, mode);

                    //target.invokeLabel7 = "timestamp : " + timestamp;
                    if (mode == 1)
                    {
                        Thread.Sleep(400);
                        int[] arr = new int[] { m1, m2, m3 };

                        target.edgeVibStimulation(arr);
                    }
                }
            }
            //stream.Close();
            //tc.Close();
        }
        
    }
}
