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


using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Interop;


namespace WristSymbol
{
    /// <summary>
    /// ExpTraining.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ExpTraining : Window
    {
        private BackgroundWorker worker;
        TextWriter tw;
        int trial;
        int trialEnd;
        String playedLetters;

        String[] letterSet = { "124","123","142","143","134","132",
                               "243","241","234","231","214","213",
                               "312","314","324","321","342","341",
                               "431","432","413","412","421","423"};
        enum pattern { top_left, top, top_right, right, bottom_right, bottom, bottom_left, left };
        bool patternAnswering;

        System.IO.Ports.SerialPort serialPort1 = new SerialPort();
        String logID;

        int duration;   // ms
        
        long startTimestamp;
        long playstamp;
        long playendstamp;
        long enterstamp;

        bool keyboardEvent = true;

        int clickedPoint = 0;
        int firstPoint = -1;
        int secondPoint = -1;
        int thirdPoint = -1;
        
        System.Timers.Timer timer;
        private double secondsToWait;   // ms
        private DateTime startTime;
        String condStr = "";

        int expCond = -1;

        public void setExpTraining(SerialPort port, String s1, int cond)
        {
            serialPort1 = port;
            logID = s1;
            duration = 500;
            expCond = cond;

            if (cond == 0)
            {
                condStr = "armFront";
                title.Content = title.Content + ": 팔 앞";
            }
            else if (cond == 1)
            {
                condStr = "armBody";
                title.Content = title.Content + ": 팔 몸";
            }

            tw = new StreamWriter(logID + "_" + condStr + "_training"+ ".csv", true);
            tw.WriteLine("id,cond,trial#,realPattern,userAnswer,correct,c1,c2,c3,playstamp,playendstamp,enterstamp,reactionTime");
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

        public ExpTraining()
        {
            InitializeComponent();
           
            startTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            trial = 1;
            trialEnd = 12;
            trialLabel.Content = trial + " / " + trialEnd;
            patternAnswering = false;
            
            Random rnd = new Random();
            letterSet = letterSet.OrderBy(x => rnd.Next()).ToArray();
            letterSet = letterSet.OrderBy(x => rnd.Next()).ToArray();
            
            timer = new System.Timers.Timer();
            timer.Interval = 100;
            timer.Elapsed += new ElapsedEventHandler(timer_Tick);
        }

        public void stimulation(int tactorNum)
        {            
            serialPort1.WriteLine("ev" + tactorNum.ToString());
            Thread.Sleep(duration);
            serialPort1.WriteLine("es" + tactorNum.ToString());
        }
        
        public void patternGenerate(String text)
        {
            int[] arr = { (int)Char.GetNumericValue(text[0]), (int)Char.GetNumericValue(text[1]), (int)Char.GetNumericValue(text[2]) };
            edgeVibStimulation(arr);
            //edgeWritePattern(text);
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
                workBackground(letterSet[trial - 1]);
                //debugLabel1.Content = "playedLetters: " + playedLetters;

                playstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;
                patternAnswering = true;
                //debugLabel1.Content = "answer1.Content : " +answer1.Content.ToString() ;
            }
        }

        private void ButtonAnwer_Click(object sender, RoutedEventArgs e)
        {
            if (patternAnswering == true && clickedPoint == 3)
            {
                patternAnswering = false;
                                
                String a = answer1.Content.ToString();
                if (expCond == 1)
                {
                    String modified_a1 = "";
                    String modified_a2 = "";
                    String modified_a3 = "";

                    if (a[0] == '1')
                        modified_a1 = "2";
                    else if (a[0] == '2')
                        modified_a1 = "4";
                    else if (a[0] == '3')
                        modified_a1 = "1";
                    else if (a[0] == '4')
                        modified_a1 = "3";

                    if (a[1] == '1')
                        modified_a2 = "2";
                    else if (a[1] == '2')
                        modified_a2 = "4";
                    else if (a[1] == '3')
                        modified_a2 = "1";
                    else if (a[1] == '4')
                        modified_a2 = "3";

                    if (a[2] == '1')
                        modified_a3 = "2";
                    else if (a[2] == '2')
                        modified_a3 = "4";
                    else if (a[2] == '3')
                        modified_a3 = "1";
                    else if (a[2] == '4')
                        modified_a3 = "3";

                    a = modified_a1 + modified_a2 + modified_a3;
                }

                enterstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;

                String correctStr = "";
                String userAnswer = firstPoint.ToString() + secondPoint.ToString() + thirdPoint.ToString();
                if (a == userAnswer)
                    correctStr = "1";
                else
                    correctStr = "0";

                string c1Str = "";
                string c2Str = "";
                string c3Str = "";

                if (a[0] == userAnswer[0])
                    c1Str = "1";
                else
                    c1Str = "0";

                if (a[1] == userAnswer[1])
                    c2Str = "1";
                else
                    c2Str = "0";

                if (a[2] == userAnswer[2])
                    c3Str = "1";
                else
                    c3Str = "0";

                long rt = enterstamp - playendstamp;
                tw.WriteLine(logID + "," + condStr + "," + trial.ToString() + "," + a + "," + userAnswer + "," + correctStr + "," + c1Str + "," + c2Str + "," + c3Str + "," + playstamp.ToString() + "," + playendstamp.ToString() + "," + enterstamp.ToString() + "," + rt.ToString());

                clearPoints();
                if (trial == trialEnd)
                    this.Close();
                else if (trial % 20 == 0)
                {
                    secondsToWait = 20000;
                    breaktime();
                }

                trial++;
                trialLabel.Content = trial + " / " + trialEnd;
            }
        }
        
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

            if (secondsToWait == 1000 * 20)
            {
                TimeSpan t1 = TimeSpan.FromMilliseconds(remainingSeconds);
                string str = t1.ToString(@"mm\:ss");
                Dispatcher.Invoke((Action)delegate () { clockLabel.Content = str;/* update UI */ });
            }

            if (remainingSeconds <= 0)
            {
                Dispatcher.Invoke((Action)delegate () { ButtonPlay.Visibility = Visibility.Visible; });
                keyboardEvent = true;
                Dispatcher.Invoke((Action)delegate () { clockLabel.Content =""; });
                // run your function
                timer.Stop();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tw.Close();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 1;
                    button1.Content = "1";
                    Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                    x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                    clickedPoint++;
                }
                else if (clickedPoint == 1)
                {
                    if (firstPoint != 1)
                    {
                        secondPoint = 1;
                        button1.Content = "2";
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 2)
                {
                    if (firstPoint != 1 && secondPoint != 1)
                    {
                        thirdPoint = 1;
                        button1.Content = "3";
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 2;
                    button2.Content = "1";
                    Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                    x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                    clickedPoint++;
                }
                else if (clickedPoint == 1)
                {
                    if (firstPoint != 2)
                    {
                        secondPoint = 2;
                        button2.Content = "2";
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 2)
                {
                    if (firstPoint != 2 && secondPoint != 2)
                    {
                        thirdPoint = 2;
                        button2.Content = "3";
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 3;
                    button3.Content = "1";
                    Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                    x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                    clickedPoint++;
                }
                else if (clickedPoint == 1)
                {
                    if (firstPoint != 3)
                    {
                        secondPoint = 3;
                        button3.Content = "2";
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 2)
                {
                    if (firstPoint != 3 && secondPoint != 3)
                    {
                        thirdPoint = 3;
                        button3.Content = "3";
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
            }
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 4;
                    button4.Content = "1";
                    Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                    x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                    clickedPoint++;
                }
                else if (clickedPoint == 1)
                {
                    if (firstPoint != 4)
                    {
                        secondPoint = 4;
                        button4.Content = "2";
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 2)
                {
                    if (firstPoint != 4 && secondPoint != 4)
                    {
                        thirdPoint = 4;
                        button4.Content = "3";
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            clearPoints();
        }

        public void clearPoints()
        {
            Button[] buttons = { button1,button2,button3,button4};
            int i;
            for (i = 0; i < 4; i++)
            {
                ((Ellipse)buttons[i].Template.FindName("ellipse", buttons[i])).Fill = System.Windows.Media.Brushes.White;
                buttons[i].Content = "";
            }
            clickedPoint = 0;
        }
    }
}
