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
using System.Threading;
using System.ComponentModel;
using System.Timers;
using System.Windows.Threading;

namespace delimiterMMTD
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

        String[] letterSet = { "a", "b", "c", "d", "e", "f",
                                "g", "h", "i", "j", "k", "l"};
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
            tw.WriteLine("trial#,pattern,answer,confidence,playstamp,playendstamp,enterstamp");
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
            trialEnd = 108;
            trialLabel.Content = trial + " / " + trialEnd;
            patternAnswering = false;
            
            Random rnd = new Random();
            playSet = playSet.OrderBy(x => rnd.Next()).ToArray();

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
            int firstTactor = -1;
            int secondTactor = -1;
            int rotateAngle_calibrated = -1;
            // Amplitude, Frequency setting
            if (text[0].ToString() == "a")
            {
                firstTactor = 1;
                secondTactor = 2;
            }
            else if (text[0].ToString() == "b")
            {
                firstTactor = 2;
                secondTactor = 1;
            }

            int rotateAngle = -1;
            if (text.Length == 2)
            {
                rotateAngle = (int)Char.GetNumericValue(text[1]) * 10;
            }
            else if (text.Length == 3)
            {
                rotateAngle = (int)Char.GetNumericValue(text[1]) * 100 + (int)Char.GetNumericValue(text[2]) * 10;
            }

            if (rotateAngle == 0)
                rotateAngle_calibrated = 5;
            else if (rotateAngle >= 10 && rotateAngle <= 90)
                rotateAngle_calibrated = 5 + ((85 - 5) * rotateAngle) / 90;
            else if (rotateAngle >= 100)
                rotateAngle_calibrated = 85 + ((170 - 85) * (rotateAngle - 90)) / 90;
            
            String rotateAngle_str = "";
            if (rotateAngle_calibrated.ToString().Length == 1)
                rotateAngle_str = rotateAngle_str + "00" + rotateAngle_calibrated.ToString();
            else if (rotateAngle_calibrated.ToString().Length == 2)
                rotateAngle_str = rotateAngle_str + "0" + rotateAngle_calibrated.ToString();
            else if (rotateAngle_calibrated.ToString().Length == 3)
                rotateAngle_str = rotateAngle_str + rotateAngle_calibrated.ToString();

            
            /*
            Dispatcher.Invoke((Action)delegate () {
                debugLabel1.Content = debugLabel1.Content + ", rotation: " + rotateAngle_str;
            });
            */

            serialPort1.WriteLine("s" + height_up_str);
            serialPort1.WriteLine("m" + rotateAngle_str);
            Thread.Sleep(500);
            serialPort1.WriteLine("s" + height_down_str);
            Thread.Sleep(1000);

            stimulation(firstTactor);
            stimulation(secondTactor);

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

        public void edgeVibPattern(string character)
        {
            int[] arr = null;
            switch (character.ToUpper())
            {
                case "A":
                    arr = new int[] { 1, 2 };
                    break;
                case "B":
                    arr = new int[] { 1, 4 };
                    break;
                case "C":
                    arr = new int[] { 1, 3 };
                    break;
                case "D":
                    arr = new int[] { 2, 4 };
                    break;
                case "E":
                    arr = new int[] { 2, 3 };
                    break;
                case "F":
                    arr = new int[] { 2, 1 };
                    break;
                case "G":
                    arr = new int[] { 4, 3 };
                    break;
                case "H":
                    arr = new int[] { 4, 1 };
                    break;
                case "I":
                    arr = new int[] { 4, 2 };
                    break;
                case "J":
                    arr = new int[] { 3, 1 };
                    break;
                case "K":
                    arr = new int[] { 3, 2 };
                    break;
                case "L":
                    arr = new int[] { 3, 4 };
                    break;
            }
                edgeVibStimulation(arr);
        }
        
        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (patternAnswering == false)
            {

                answer1.Content = "";
                playedLetters = "";

                playedLetters = playedLetters + playSet[trial-1];
                answer1.Content = playSet[trial-1];
                Thread.Sleep(400);
                workBackground(playedLetters);

                //debugLabel1.Content = "playedLetters: " + playedLetters;

                playstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;
                patternAnswering = true;

                //debugLabel1.Content = "answer1.Content : " +answer1.Content.ToString() ;
            }
        }

        public void clickAnswer(int answer)
        {
            if (patternAnswering == true && confAnswered == true)
            {
                String a = answer1.Content.ToString();
                String correctStr = "";
                

                
                /*
                if (l == a)
                {
                    answer1.Background = new SolidColorBrush(Color.FromRgb(0x66, 0xff, 0x66));
                    answer1.Visibility = Visibility.Visible;
                    correctStr = "1";
                    recentResults.Enqueue(1);
                }
                else
                {
                    answer1.Background = new SolidColorBrush(Color.FromRgb(0xff, 0x66, 0x66));
                    answer1.Visibility = Visibility.Visible;
                    correctStr = "0";
                    recentResults.Enqueue(0);
                }
                */
                patternAnswering = false;
                confAnswered = false;

                int sum = 0;
                int i;
                for (i = 0; i < recentResults.Size(); i++)
                {
                    sum = sum + recentResults.Get(i);
                }
                recentAccuracy = (double)sum / recentResults.Size();

                String confStr = "";
                if (confidenceLevel == 1)
                    confStr = "1";
                else if (confidenceLevel == 2)
                    confStr = "2";
                else if (confidenceLevel == 3)
                    confStr = "3";


                enterstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;
                tw.WriteLine(trial.ToString() + "," + a + "," + answer.ToString() + "," + confStr + "," + playstamp.ToString() + "," + playendstamp.ToString() + "," + enterstamp.ToString());
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
        

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            clickAnswer((int)pattern.top_left);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            clickAnswer((int)pattern.top);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            clickAnswer((int)pattern.top_right);
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            clickAnswer((int)pattern.right);
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            clickAnswer((int)pattern.bottom_right);
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            clickAnswer((int)pattern.bottom);
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            clickAnswer((int)pattern.bottom_left);
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            clickAnswer((int)pattern.left);
        }


        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (keyboardEvent)
            {
                if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
                {
                    if (e.Key != Key.NumPad5)
                    {
                        if (e.Key == Key.NumPad1)
                            clickAnswer((int)pattern.bottom_left);
                        else if(e.Key == Key.NumPad2)
                            clickAnswer((int)pattern.bottom);
                        else if (e.Key == Key.NumPad3)
                            clickAnswer((int)pattern.bottom_right);
                        else if (e.Key == Key.NumPad4)
                            clickAnswer((int)pattern.left);
                        else if (e.Key == Key.NumPad6)
                            clickAnswer((int)pattern.right);
                        else if (e.Key == Key.NumPad7)
                            clickAnswer((int)pattern.top_left);
                        else if (e.Key == Key.NumPad8)
                            clickAnswer((int)pattern.top);
                        else if (e.Key == Key.NumPad9)
                            clickAnswer((int)pattern.top_right);
                    }
                }
                else if (e.Key == Key.Space)
                {
                    ButtonPlay_Click(sender, e);
                }
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tw.Close();
        }

        private void Button_high_click(object sender, RoutedEventArgs e)
        {
            confidenceLevel = 3;
            confAnswered = true;
        }

        private void Button_middle_click(object sender, RoutedEventArgs e)
        {
            confidenceLevel = 2;
            confAnswered = true;
        }

        private void Button_low_click(object sender, RoutedEventArgs e)
        {
            confidenceLevel = 1;
            confAnswered = true;
        }
    }
}
