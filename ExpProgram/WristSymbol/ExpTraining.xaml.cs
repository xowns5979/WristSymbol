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
        int confidenceLevel = -1;   // 1: Low, 2: Middle, 3: High

        String[] letterSet = { "124","123","142","143","134","132","243","241","234","231","214","213",
                               "312","314","324","321","342","341","431","432","413","412","421","423",
                               "124","123","142","143","134","132","243","241","234","231","214","213",
                               "312","314","324","321","342","341","431","432","413","412","421","423"};

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

        int expCond = -1;
        String condStr = "";
        String vibTypeStr = "";
        String blockNumStr = "";
        public void setExpTraining(SerialPort port, String s1, int cond, int vibtype, int blocknum)
        {
            serialPort1 = port;
            logID = s1;
            duration = 500;
            expCond = cond;

            if (cond == 0)
            {
                condStr = "armfront";
                title.Content = title.Content + ": 팔 앞쪽";
            }
            else if (cond == 1)
            {
                condStr = "armbody";
                title.Content = title.Content + ": 팔 몸쪽";
            }

            string powerLowStr = "a030";
            string freqHighStr = "f300";
            string modulateOnTimeStr = "br040";
            string modulateOffTimeStr = "bs040";

            if (vibtype == 0)   // Baseline -> 1,2,3,4번 모터 파워 감소
            {
                vibTypeStr = "baseline";
                serialPort1.WriteLine("1" + powerLowStr);
                serialPort1.WriteLine("2" + powerLowStr);
                serialPort1.WriteLine("3" + powerLowStr);
                serialPort1.WriteLine("4" + powerLowStr);
            }
            else if (vibtype == 1)  // 2 Color -> 1,2,3,4번 모터 파워 감소. 2번, 4번 모터에 모듈레이션 추가
            {
                vibTypeStr = "2color";
                serialPort1.WriteLine("1" + powerLowStr);
                serialPort1.WriteLine("2" + powerLowStr);
                serialPort1.WriteLine("3" + powerLowStr);
                serialPort1.WriteLine("4" + powerLowStr);
                serialPort1.WriteLine("2" + modulateOnTimeStr);
                serialPort1.WriteLine("2" + modulateOffTimeStr);
                serialPort1.WriteLine("4" + modulateOnTimeStr);
                serialPort1.WriteLine("4" + modulateOffTimeStr);
            }
            else if (vibtype == 2)  // 4 Color -> 1번: 고주파수, 2번: 고주파수 + 모듈레이션, 3번: 파워 감소, 4번: 모듈레이션 + 파워 감소
            {
                vibTypeStr = "4color";
                serialPort1.WriteLine("1" + freqHighStr);
                serialPort1.WriteLine("2" + freqHighStr);
                serialPort1.WriteLine("2" + modulateOnTimeStr);
                serialPort1.WriteLine("2" + modulateOffTimeStr);
                serialPort1.WriteLine("3" + powerLowStr);
                serialPort1.WriteLine("4" + powerLowStr);
                serialPort1.WriteLine("4" + modulateOnTimeStr);
                serialPort1.WriteLine("4" + modulateOffTimeStr);
            }
            
            blockNumStr = (blocknum+1).ToString();

            title.Content = title.Content + ", " + vibTypeStr + ", block " + blockNumStr;

            tw = new StreamWriter(logID + "_" + condStr + "_" + vibTypeStr + "_training_"+ blockNumStr +".csv", true);
            tw.WriteLine("id,cond,vibtype,blocknum,trial#,realPattern,userAnswer,correct,c1,c2,c3,playstamp,playendstamp,enterstamp");
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
            trialEnd = letterSet.Length;
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
            serialPort1.WriteLine(tactorNum.ToString() + "v");
            Thread.Sleep(duration);
            serialPort1.WriteLine(tactorNum.ToString() + "s");
        }

        public void patternGenerate(String text)
        {
            int[] arr = { (int)Char.GetNumericValue(text[0]), (int)Char.GetNumericValue(text[1]), (int)Char.GetNumericValue(text[2])};
            edgeVibStimulation(arr);
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
        
        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (patternAnswering == false)
            {
                // 정답 피드백 주기
                clearPoints();
                answer1.Content = "";
                answer1.Content = letterSet[trial - 1];
                Thread.Sleep(400);

                playstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;
                workBackground(letterSet[trial - 1]);

                patternAnswering = true;
            }
        }

        private void ButtonAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (patternAnswering == true && clickedPoint == 3)
            {
                enterstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;

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
                
                String correctStr = "";
                String userAnswer = firstPoint.ToString() + secondPoint.ToString() + thirdPoint.ToString();
                
                clearPoints();

                // 정답 피드백 주기
                if (a == userAnswer)
                {
                    if (a[0] == '1')
                    {
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button1.Content = "1";
                    }
                    else if (a[0] == '2')
                    {
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button2.Content = "1";
                    }
                    else if (a[0] == '3')
                    {
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button3.Content = "1";
                    }
                    else if (a[0] == '4')
                    {
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button4.Content = "1";
                    }

                    if (a[1] == '1')
                    {
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button1.Content = "2";
                    }
                    else if (a[1] == '2')
                    {
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button2.Content = "2";
                    }
                    else if (a[1] == '3')
                    {
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button3.Content = "2";
                    }
                    else if (a[1] == '4')
                    {
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button4.Content = "2";
                    }

                    if (a[2] == '1')
                    {
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button1.Content = "3";
                    }
                    else if (a[2] == '2')
                    {
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button2.Content = "3";
                    }
                    else if (a[2] == '3')
                    {
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button3.Content = "3";
                    }
                    else if (a[2] == '4')
                    {
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xff, 0x66));
                        button4.Content = "3";
                    }
                }
                else
                {
                    if (a[0] == '1')
                    {
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button1.Content = "1";
                    }
                    else if (a[0] == '2')
                    {
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button2.Content = "1";
                    }
                    else if (a[0] == '3')
                    {
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button3.Content = "1";
                    }
                    else if (a[0] == '4')
                    {
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button4.Content = "1";
                    }

                    if (a[1] == '1')
                    {
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button1.Content = "2";
                    }
                    else if (a[1] == '2')
                    {
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button2.Content = "2";
                    }
                    else if (a[1] == '3')
                    {
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button3.Content = "2";
                    }
                    else if (a[1] == '4')
                    {
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button4.Content = "2";
                    }

                    if (a[2] == '1')
                    {
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button1.Content = "3";
                    }
                    else if (a[2] == '2')
                    {
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button2.Content = "3";
                    }
                    else if (a[2] == '3')
                    {
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button3.Content = "3";
                    }
                    else if (a[2] == '4')
                    {
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x66, 0x66));
                        button4.Content = "3";
                    }
                }

                /*
                if (expCond == 1)
                {
                    String modified_answer1 = "";
                    String modified_answer2 = "";
                    if (userAnswer[0] == '1')
                        modified_answer1 = "3";
                    else if (userAnswer[0] == '2')
                        modified_answer1 = "1";
                    else if (userAnswer[0] == '3')
                        modified_answer1 = "4";
                    else if (userAnswer[0] == '4')
                        modified_answer1 = "2";
                    if (userAnswer[1] == '1')
                        modified_answer2 = "3";
                    else if (userAnswer[1] == '2')
                        modified_answer2 = "1";
                    else if (userAnswer[1] == '3')
                        modified_answer2 = "4";
                    else if (userAnswer[1] == '4')
                        modified_answer2 = "2";

                    String modified_a1 = "";
                    String modified_a2 = "";
                    if (a[0] == '1')
                        modified_a1 = "3";
                    else if (a[0] == '2')
                        modified_a1 = "1";
                    else if (a[0] == '3')
                        modified_a1 = "4";
                    else if (a[0] == '4')
                        modified_a1 = "2";
                    if (a[1] == '1')
                        modified_a2 = "3";
                    else if (a[1] == '2')
                        modified_a2 = "1";
                    else if (a[1] == '3')
                        modified_a2 = "4";
                    else if (a[1] == '4')
                        modified_a2 = "2";

                    userAnswer = modified_answer1 + modified_answer2;
                    a = modified_a1 + modified_a2;
                }
                */


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

                tw.WriteLine(logID + "," + condStr + "," + vibTypeStr + "," + blockNumStr + "," + trial.ToString() + "," + a + "," + userAnswer + "," + correctStr + "," + c1Str + "," + c2Str + "," + c3Str + "," + playstamp.ToString() + "," + playendstamp.ToString() + "," + enterstamp.ToString());

                if (trial == trialEnd)
                    this.Close();
                else if (trial % 20 == 0)
                {
                    secondsToWait = 20 * 1000;
                    breaktime();
                }

                trial++;
                trialLabel.Content = trial + " / " + trialEnd;
            }
        }

        public static void SaveClipboardImageToFile(BitmapSource img, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(fileStream);
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
        

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {

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

        public String modifyString(String str)
        {
            String modified_s1 = "";
            String modified_s2 = "";
            String modified_s3 = "";
            if (str[0] == '1')
                modified_s1 = "2";
            else if (str[0] == '2')
                modified_s1 = "4";
            else if (str[0] == '3')
                modified_s1 = "1";
            else if (str[0] == '4')
                modified_s1 = "3";

            if (str[1] == '1')
                modified_s2 = "2";
            else if (str[1] == '2')
                modified_s2 = "4";
            else if (str[1] == '3')
                modified_s2 = "1";
            else if (str[1] == '4')
                modified_s2 = "3";

            if (str[2] == '1')
                modified_s3 = "2";
            else if (str[2] == '2')
                modified_s3 = "4";
            else if (str[2] == '3')
                modified_s3 = "1";
            else if (str[2] == '4')
                modified_s3 = "3";

            return (modified_s1 + modified_s2 + modified_s3);
        }

        private void Pattern1_Click(object sender, RoutedEventArgs e)
        {
            String str = "124";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);

            workBackground(str);

        }

        private void Pattern2_Click(object sender, RoutedEventArgs e)
        {
            String str = "123";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern3_Click(object sender, RoutedEventArgs e)
        {
            String str = "142";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern4_Click(object sender, RoutedEventArgs e)
        {
            String str = "143";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern5_Click(object sender, RoutedEventArgs e)
        {
            String str = "134";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern6_Click(object sender, RoutedEventArgs e)
        {
            String str = "132";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern7_Click(object sender, RoutedEventArgs e)
        {
            String str = "243";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern8_Click(object sender, RoutedEventArgs e)
        {
            String str = "241";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern9_Click(object sender, RoutedEventArgs e)
        {
            String str = "234";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern10_Click(object sender, RoutedEventArgs e)
        {
            String str = "231";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern11_Click(object sender, RoutedEventArgs e)
        {
            String str = "213";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern12_Click(object sender, RoutedEventArgs e)
        {
            String str = "214";
            //Thread.Sleep(400);
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern13_Click(object sender, RoutedEventArgs e)
        {

            String str = "312";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern14_Click(object sender, RoutedEventArgs e)
        {

            String str = "314";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);
        }

        private void Pattern15_Click(object sender, RoutedEventArgs e)
        {
            String str = "324";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);

        }

        private void Pattern16_Click(object sender, RoutedEventArgs e)
        {
            String str = "321";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);

        }

        private void Pattern17_Click(object sender, RoutedEventArgs e)
        {
            String str = "342";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);

        }

        private void Pattern18_Click(object sender, RoutedEventArgs e)
        {
            String str = "341";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);

        }

        private void Pattern19_Click(object sender, RoutedEventArgs e)
        {
            String str = "431";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);

        }

        private void Pattern20_Click(object sender, RoutedEventArgs e)
        {
            String str = "432";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);

        }

        private void Pattern21_Click(object sender, RoutedEventArgs e)
        {
            String str = "413";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);

        }

        private void Pattern22_Click(object sender, RoutedEventArgs e)
        {
            String str = "412";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);

        }

        private void Pattern23_Click(object sender, RoutedEventArgs e)
        {
            String str = "421";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);

        }

        private void Pattern24_Click(object sender, RoutedEventArgs e)
        {
            String str = "423";
            if (expCond == 0)
                str = modifyString(str);
            workBackground(str);

        }
    }
    
}
