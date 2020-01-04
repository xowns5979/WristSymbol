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
    public partial class ExpMain : Window
    {
        private BackgroundWorker worker;
        TextWriter tw;
        int trial;
        int trialEnd;
        String playedLetters;
        int confidenceLevel = -1;   // 1: Low, 2: Middle, 3: High

        String[] letterSet = { "f1","f3","r1","r3","t1","t3",
                               "f1","f3","r1","r3","t1","t3",
                               "f1","f3","r1","r3","t1","t3",
                               "f1","f3","r1","r3","t1","t3",
                               "f1","f3","r1","r3","t1","t3"};
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

        int clickedPoint = 0;
        int firstPoint = -1;
        int secondPoint = -1;
        int thirdPoint = -1;
        
        System.Timers.Timer timer;
        private double secondsToWait;   // ms
        private DateTime startTime;
        

        public void setExpMain(SerialPort port, String s1)
        {
            serialPort1 = port;
            logID = s1;
            baseModality = 0;
            duration = 500;
            
            tw = new StreamWriter(logID + "_exp" + ".csv", true);
            tw.WriteLine("id,trial#,pattern,firstPoint,secondPoint,thirdPoint,playstamp,playendstamp");
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
            funnelingPattern(text);
            //patternGenerate(text);
            playendstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;

        }

        public void fstimuli(int level)
        {
            stimulation(2);
            Thread.Sleep(100);
            stimulation2(1,3,level);
            Thread.Sleep(100);
            stimulation(3);
        }

        public void rstimuli(int level)
        {
            stimulation(3);
            Thread.Sleep(100);
            stimulation2(1, 3,level);
            Thread.Sleep(100);
            stimulation(2);
        }

        public void tstimuli(int level)
        {
            stimulation2(1, 3, level);
            Thread.Sleep(100);
            stimulation(2);
            Thread.Sleep(100);
            stimulation(4);
        }

        public void funnelingPattern(String text)
        {
            Dispatcher.Invoke((Action)delegate () { debugLabel1.Content = ""; });
            int[] arr = null;
            switch (text)
            {
                case "f1":
                    fstimuli(1);
                    break;
                    break;
                case "f3":
                    fstimuli(3);
                    break;
                case "r1":
                    rstimuli(1);
                    break;
                case "r3":
                    rstimuli(3);
                    break;
                case "t1":
                    tstimuli(1);
                    break;
                case "t3":
                    tstimuli(3);
                    break;

            }
           
        }

        public ExpMain()
        {
            InitializeComponent();
           
            startTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            trial = 1;
            trialEnd = 30;
            trialLabel.Content = trial + " / " + trialEnd;
            patternAnswering = false;
            
            Random rnd = new Random();
            letterSet = letterSet.OrderBy(x => rnd.Next()).ToArray();
            letterSet = letterSet.OrderBy(x => rnd.Next()).ToArray();


            
                
            //TcpData tcp = new TcpData(this);
            //Thread t1 = new Thread(new ThreadStart(tcp.connect));

            //t1.Start();
            
            timer = new System.Timers.Timer();
            timer.Interval = 100;
            timer.Elapsed += new ElapsedEventHandler(timer_Tick);
            
        }

        public void stimulation(int tactorNum)
        {
            serialPort1.WriteLine("i" + tactorNum + "255");
            //Dispatcher.Invoke((Action)delegate () { debugLabel1.Content = debugLabel1.Content + "i" + tactorNum + "255,"; });
            
            serialPort1.WriteLine("ev" + tactorNum.ToString());
            Thread.Sleep(duration);
            serialPort1.WriteLine("es" + tactorNum.ToString());
        }

        public void stimulation2(int tactorNum1, int tactorNum2, int level)
        {
            String intensity1 = "";
            String intensity2 = "";
            if (level == 1)
            {
                serialPort1.WriteLine("i" + tactorNum1 + "255");
                serialPort1.WriteLine("i" + tactorNum2 + "000");
                intensity1 = "255";
                intensity2 = "000";
            }
            else if (level == 2)
            {
                serialPort1.WriteLine("i" + tactorNum1 + "209");
                serialPort1.WriteLine("i" + tactorNum2 + "148");
                intensity1 = "209";
                intensity2 = "148";
            }
            else if (level == 3)
            {
                serialPort1.WriteLine("i" + tactorNum1 + "179");
                serialPort1.WriteLine("i" + tactorNum2 + "179");
                intensity1 = "179";
                intensity2 = "179";
            }
            else if (level == 4)
            {
                serialPort1.WriteLine("i" + tactorNum1 + "148");
                serialPort1.WriteLine("i" + tactorNum2 + "209");
                intensity1 = "148";
                intensity2 = "209";
            }

            //Dispatcher.Invoke((Action)delegate () { debugLabel1.Content = debugLabel1.Content + "(i" + tactorNum1 + intensity1+",i"+tactorNum2+intensity2+"),"; });


            serialPort1.WriteLine("ev" + tactorNum1.ToString());
            serialPort1.WriteLine("ev" + tactorNum2.ToString());
            Thread.Sleep(duration);
            serialPort1.WriteLine("es" + tactorNum1.ToString());
            serialPort1.WriteLine("es" + tactorNum2.ToString());
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
                //workBackground(letterSet[trial - 1]);
                workBackground("f2");

                /*
                stimulation(1);

                Thread.Sleep(100);
                stimulation2(1, 3);
                Thread.Sleep(100);
                stimulation(3);
                */

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
                
                confAnswered = false;

                int sum = 0;
                int i;
                for (i = 0; i < recentResults.Size(); i++)
                {
                    sum = sum + recentResults.Get(i);
                }
                recentAccuracy = (double)sum / recentResults.Size();

                

                enterstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;
                tw.WriteLine(logID+","+trial.ToString() + "," + a + ","+firstPoint.ToString()+","+secondPoint.ToString()+","+thirdPoint.ToString()+"," + playstamp.ToString() + "," + playendstamp.ToString());
                
                

                String filename = logID + "_" + trial + ".png";
                SaveClipboardImageToFile(CopyScreen(1255,465,0,0), filename);

                clearPoints();

                if (trial == trialEnd)
                    this.Close();
                

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

        private static BitmapSource CopyScreen(int sourceX, int sourceY, int destinationX, int destinationY)
        {
            
            using (var screenBmp = new Bitmap((int)SystemParameters.PrimaryScreenWidth-sourceX-385, (int)SystemParameters.PrimaryScreenHeight-sourceY-334, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            //using (var screenBmp = new Bitmap(right - left, bottom - top, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen(sourceX, sourceY, destinationX, destinationY, screenBmp.Size);
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
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

            TimeSpan t1 = TimeSpan.FromSeconds(remainingSeconds);
            string str = t1.ToString(@"mm\:ss");
            Dispatcher.Invoke((Action)delegate () { clockLabel.Content = str; });

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
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 1;
                    button1.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 1;
                    button1.Content = "3";
                }
                Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
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
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 2;
                    button2.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 2;
                    button2.Content = "3";
                }
                Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
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
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 3;
                    button3.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 3;
                    button3.Content = "3";
                }
                Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
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
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 4;
                    button4.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 4;
                    button4.Content = "3";
                }
                Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 5;
                    button5.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 5;
                    button5.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =5 ;
                    button5.Content = "3";
                }
                Ellipse x1 = (Ellipse)button5.Template.FindName("ellipse", button5);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 6;
                    button6.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =6 ;
                    button6.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =6 ;
                    button6.Content = "3";
                }
                Ellipse x1 = (Ellipse)button6.Template.FindName("ellipse", button6);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 7;
                    button7.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 7;
                    button7.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =7 ;
                    button7.Content = "3";
                }
                Ellipse x1 = (Ellipse)button7.Template.FindName("ellipse", button7);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 8;
                    button8.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 8;
                    button8.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =8 ;
                    button8.Content = "3";
                }
                Ellipse x1 = (Ellipse)button8.Template.FindName("ellipse", button8);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button9_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 9;
                    button9.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 9;
                    button9.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 9;
                    button9.Content = "3";
                }
                Ellipse x1 = (Ellipse)button9.Template.FindName("ellipse", button9);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button10_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 10;
                    button10.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =10 ;
                    button10.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =10 ;
                    button10.Content = "3";
                }
                Ellipse x1 = (Ellipse)button10.Template.FindName("ellipse", button10);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button11_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 11;
                    button11.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =11 ;
                    button11.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 11;
                    button11.Content = "3";
                }
                Ellipse x1 = (Ellipse)button11.Template.FindName("ellipse", button11);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button12_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 12;
                    button12.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 12;
                    button12.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 12;
                    button12.Content = "3";
                }
                Ellipse x1 = (Ellipse)button12.Template.FindName("ellipse", button12);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button13_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 13;
                    button13.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 13;
                    button13.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 13;
                    button13.Content = "3";
                }
                Ellipse x1 = (Ellipse)button13.Template.FindName("ellipse", button13);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button14_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =14 ;
                    button14.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 14;
                    button14.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 14;
                    button14.Content = "3";
                }
                Ellipse x1 = (Ellipse)button14.Template.FindName("ellipse", button14);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button15_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =15 ;
                    button15.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 15;
                    button15.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 15;
                    button15.Content = "3";
                }
                Ellipse x1 = (Ellipse)button15.Template.FindName("ellipse", button15);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button16_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 16;
                    button16.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 16;
                    button16.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 16;
                    button16.Content = "3";
                }
                Ellipse x1 = (Ellipse)button16.Template.FindName("ellipse", button16);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button17_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 17;
                    button17.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 17;
                    button17.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 17;
                    button17.Content = "3";
                }
                Ellipse x1 = (Ellipse)button17.Template.FindName("ellipse", button17);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button18_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 18;
                    button18.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 18;
                    button18.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 18;
                    button18.Content = "3";
                }
                Ellipse x1 = (Ellipse)button18.Template.FindName("ellipse", button18);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button19_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =19 ;
                    button19.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 19;
                    button19.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 19;
                    button19.Content = "3";
                }
                Ellipse x1 = (Ellipse)button19.Template.FindName("ellipse", button19);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button20_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =20 ;
                    button20.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =20 ;
                    button20.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =20 ;
                    button20.Content = "3";
                }
                Ellipse x1 = (Ellipse)button20.Template.FindName("ellipse", button20);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button21_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =21 ;
                    button21.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =21 ;
                    button21.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =21 ;
                    button21.Content = "3";
                }
                Ellipse x1 = (Ellipse)button21.Template.FindName("ellipse", button21);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button22_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 22;
                    button22.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 22;
                    button22.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 22;
                    button22.Content = "3";
                }
                Ellipse x1 = (Ellipse)button22.Template.FindName("ellipse", button22);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button23_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 23;
                    button23.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 23;
                    button23.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 23;
                    button23.Content = "3";
                }
                Ellipse x1 = (Ellipse)button23.Template.FindName("ellipse", button23);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button24_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 24;
                    button24.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 24;
                    button24.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 24;
                    button24.Content = "3";
                }
                Ellipse x1 = (Ellipse)button24.Template.FindName("ellipse", button24);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button25_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 25;
                    button25.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 25;
                    button25.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =25 ;
                    button25.Content = "3";
                }
                Ellipse x1 = (Ellipse)button25.Template.FindName("ellipse", button25);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button26_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 26;
                    button26.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 26;
                    button26.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 26;
                    button26.Content = "3";
                }
                Ellipse x1 = (Ellipse)button26.Template.FindName("ellipse", button26);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button27_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 27;
                    button27.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =27 ;
                    button27.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =27 ;
                    button27.Content = "3";
                }
                Ellipse x1 = (Ellipse)button27.Template.FindName("ellipse", button27);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button28_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 28;
                    button28.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 28;
                    button28.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 28;
                    button28.Content = "3";
                }
                Ellipse x1 = (Ellipse)button28.Template.FindName("ellipse", button28);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button29_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 29;
                    button29.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 29;
                    button29.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 29;
                    button29.Content = "3";
                }
                Ellipse x1 = (Ellipse)button29.Template.FindName("ellipse", button29);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button30_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 30;
                    button30.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 30;
                    button30.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 30;
                    button30.Content = "3";
                }
                Ellipse x1 = (Ellipse)button30.Template.FindName("ellipse", button30);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button31_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 31;
                    button31.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 31;
                    button31.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 31;
                    button31.Content = "3";
                }
                Ellipse x1 = (Ellipse)button31.Template.FindName("ellipse", button31);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button32_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 32;
                    button32.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 32;
                    button32.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 32;
                    button32.Content = "3";
                }
                Ellipse x1 = (Ellipse)button32.Template.FindName("ellipse", button32);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button33_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 33;
                    button33.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 33;
                    button33.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 33;
                    button33.Content = "3";
                }
                Ellipse x1 = (Ellipse)button33.Template.FindName("ellipse", button33);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button34_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =34 ;
                    button34.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 34;
                    button34.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 34;
                    button34.Content = "3";
                }
                Ellipse x1 = (Ellipse)button34.Template.FindName("ellipse", button34);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button35_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =35 ;
                    button35.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =35 ;
                    button35.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =35 ;
                    button35.Content = "3";
                }
                Ellipse x1 = (Ellipse)button35.Template.FindName("ellipse", button35);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button36_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 36;
                    button36.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =36 ;
                    button36.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 36;
                    button36.Content = "3";
                }
                Ellipse x1 = (Ellipse)button36.Template.FindName("ellipse", button36);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button37_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =37 ;
                    button37.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 37;
                    button37.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 37;
                    button37.Content = "3";
                }
                Ellipse x1 = (Ellipse)button37.Template.FindName("ellipse", button37);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button38_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 38;
                    button38.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 38;
                    button38.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 38;
                    button38.Content = "3";
                }
                Ellipse x1 = (Ellipse)button38.Template.FindName("ellipse", button38);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button39_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =39 ;
                    button39.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 39;
                    button39.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =39 ;
                    button39.Content = "3";
                }
                Ellipse x1 = (Ellipse)button39.Template.FindName("ellipse", button39);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button40_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 40;
                    button40.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 40;
                    button40.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 40;
                    button40.Content = "3";
                }
                Ellipse x1 = (Ellipse)button40.Template.FindName("ellipse", button40);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button41_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 41;
                    button41.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =41 ;
                    button41.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =41 ;
                    button41.Content = "3";
                }
                Ellipse x1 = (Ellipse)button41.Template.FindName("ellipse", button41);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button42_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 42;
                    button42.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 42;
                    button42.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 42;
                    button42.Content = "3";
                }
                Ellipse x1 = (Ellipse)button42.Template.FindName("ellipse", button42);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button43_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 43;
                    button43.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 43;
                    button43.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 43;
                    button43.Content = "3";
                }
                Ellipse x1 = (Ellipse)button43.Template.FindName("ellipse", button43);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button44_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =44 ;
                    button44.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 44;
                    button44.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =44 ;
                    button44.Content = "3";
                }
                Ellipse x1 = (Ellipse)button44.Template.FindName("ellipse", button44);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button45_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 45;
                    button45.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 45;
                    button45.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 45;
                    button45.Content = "3";
                }
                Ellipse x1 = (Ellipse)button45.Template.FindName("ellipse", button45);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button46_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 46;
                    button46.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 46;
                    button46.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 46;
                    button46.Content = "3";
                }
                Ellipse x1 = (Ellipse)button46.Template.FindName("ellipse", button46);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button47_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 47;
                    button47.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 47;
                    button47.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 47;
                    button47.Content = "3";
                }
                Ellipse x1 = (Ellipse)button47.Template.FindName("ellipse", button47);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button48_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 48;
                    button48.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =48 ;
                    button48.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =48 ;
                    button48.Content = "3";
                }
                Ellipse x1 = (Ellipse)button48.Template.FindName("ellipse", button48);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button49_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =49 ;
                    button49.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 49;
                    button49.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =49 ;
                    button49.Content = "3";
                }
                Ellipse x1 = (Ellipse)button49.Template.FindName("ellipse", button49);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button50_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =50 ;
                    button50.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =50 ;
                    button50.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 50;
                    button50.Content = "3";
                }
                Ellipse x1 = (Ellipse)button50.Template.FindName("ellipse", button50);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button51_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =51 ;
                    button51.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =51 ;
                    button51.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 51;
                    button51.Content = "3";
                }
                Ellipse x1 = (Ellipse)button51.Template.FindName("ellipse", button51);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button52_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 52;
                    button52.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 52;
                    button52.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 52;
                    button52.Content = "3";
                }
                Ellipse x1 = (Ellipse)button52.Template.FindName("ellipse", button52);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button53_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 53;
                    button53.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 53;
                    button53.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =53 ;
                    button53.Content = "3";
                }
                Ellipse x1 = (Ellipse)button53.Template.FindName("ellipse", button53);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button54_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 54;
                    button54.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =54 ;
                    button54.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =54 ;
                    button54.Content = "3";
                }
                Ellipse x1 = (Ellipse)button54.Template.FindName("ellipse", button54);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button55_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 55;
                    button55.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 55;
                    button55.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =55 ;
                    button55.Content = "3";
                }
                Ellipse x1 = (Ellipse)button55.Template.FindName("ellipse", button55);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button56_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =56 ;
                    button56.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =56 ;
                    button56.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =56 ;
                    button56.Content = "3";
                }
                Ellipse x1 = (Ellipse)button56.Template.FindName("ellipse", button56);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button57_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 57;
                    button57.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =57 ;
                    button57.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 57;
                    button57.Content = "3";
                }
                Ellipse x1 = (Ellipse)button57.Template.FindName("ellipse", button57);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button58_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =58 ;
                    button58.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =58 ;
                    button58.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 58;
                    button58.Content = "3";
                }
                Ellipse x1 = (Ellipse)button58.Template.FindName("ellipse", button58);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button59_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 59;
                    button59.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =59 ;
                    button59.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 59;
                    button59.Content = "3";
                }
                Ellipse x1 = (Ellipse)button59.Template.FindName("ellipse", button59);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button60_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 60;
                    button60.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 60;
                    button60.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 60;
                    button60.Content = "3";
                }
                Ellipse x1 = (Ellipse)button60.Template.FindName("ellipse", button60);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button61_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =61 ;
                    button61.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =61 ;
                    button61.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =61 ;
                    button61.Content = "3";
                }
                Ellipse x1 = (Ellipse)button61.Template.FindName("ellipse", button61);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button62_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 62;
                    button62.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 62;
                    button62.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 62;
                    button62.Content = "3";
                }
                Ellipse x1 = (Ellipse)button62.Template.FindName("ellipse", button62);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button63_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 63;
                    button63.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =63 ;
                    button63.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =63 ;
                    button63.Content = "3";
                }
                Ellipse x1 = (Ellipse)button63.Template.FindName("ellipse", button63);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button64_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 64;
                    button64.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 64;
                    button64.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 64;
                    button64.Content = "3";
                }
                Ellipse x1 = (Ellipse)button64.Template.FindName("ellipse", button64);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button65_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 65;
                    button65.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 65;
                    button65.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 65;
                    button65.Content = "3";
                }
                Ellipse x1 = (Ellipse)button65.Template.FindName("ellipse", button65);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button66_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 66;
                    button66.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 66;
                    button66.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 66;
                    button66.Content = "3";
                }
                Ellipse x1 = (Ellipse)button66.Template.FindName("ellipse", button66);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button67_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 67;
                    button67.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 67;
                    button67.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 67;
                    button67.Content = "3";
                }
                Ellipse x1 = (Ellipse)button67.Template.FindName("ellipse", button67);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button68_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 68;
                    button68.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 68;
                    button68.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 68;
                    button68.Content = "3";
                }
                Ellipse x1 = (Ellipse)button68.Template.FindName("ellipse", button68);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button69_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 69;
                    button69.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 69;
                    button69.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 69;
                    button69.Content = "3";
                }
                Ellipse x1 = (Ellipse)button69.Template.FindName("ellipse", button69);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button70_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 70;
                    button70.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 70;
                    button70.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =70 ;
                    button70.Content = "3";
                }
                Ellipse x1 = (Ellipse)button70.Template.FindName("ellipse", button70);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button71_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 71;
                    button71.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 71;
                    button71.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 71;
                    button71.Content = "3";
                }
                Ellipse x1 = (Ellipse)button71.Template.FindName("ellipse", button71);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button72_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 72;
                    button72.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 72;
                    button72.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =72 ;
                    button72.Content = "3";
                }
                Ellipse x1 = (Ellipse)button72.Template.FindName("ellipse", button72);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button73_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 73;
                    button73.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 73;
                    button73.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 73;
                    button73.Content = "3";
                }
                Ellipse x1 = (Ellipse)button73.Template.FindName("ellipse", button73);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button74_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 74;
                    button74.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 74;
                    button74.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint =74 ;
                    button74.Content = "3";
                }
                Ellipse x1 = (Ellipse)button74.Template.FindName("ellipse", button74);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button75_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 75;
                    button75.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 75;
                    button75.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 75;
                    button75.Content = "3";
                }
                Ellipse x1 = (Ellipse)button75.Template.FindName("ellipse", button75);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button76_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 76;
                    button76.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 76;
                    button76.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 76;
                    button76.Content = "3";
                }
                Ellipse x1 = (Ellipse)button76.Template.FindName("ellipse", button76);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button77_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 77;
                    button77.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 77;
                    button77.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 77;
                    button77.Content = "3";
                }
                Ellipse x1 = (Ellipse)button77.Template.FindName("ellipse", button77);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button78_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 78;
                    button78.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =78 ;
                    button78.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 78;
                    button78.Content = "3";
                }
                Ellipse x1 = (Ellipse)button78.Template.FindName("ellipse", button78);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button79_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 79;
                    button79.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint = 79;
                    button79.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 79;
                    button79.Content = "3";
                }
                Ellipse x1 = (Ellipse)button79.Template.FindName("ellipse", button79);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button80_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint =80 ;
                    button80.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =80 ;
                    button80.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 80;
                    button80.Content = "3";
                }
                Ellipse x1 = (Ellipse)button80.Template.FindName("ellipse", button80);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }
        private void Button81_Click(object sender, RoutedEventArgs e)
        {
            
            if (clickedPoint < 3)
            {
                if (clickedPoint == 0)
                {
                    firstPoint = 81;
                    button81.Content = "1";
                }
                else if (clickedPoint == 1)
                {
                    secondPoint =81 ;
                    button81.Content = "2";
                }
                else if (clickedPoint == 2)
                {
                    thirdPoint = 81;
                    button81.Content = "3";
                }
                Ellipse x1 = (Ellipse)button81.Template.FindName("ellipse", button81);
                x1.Fill = System.Windows.Media.Brushes.Gray;
                clickedPoint++;
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            clearPoints();
        }

        public void clearPoints()
        {
            Button[] buttons = { button1,button2,button3,button4,button5,button6,button7,button8,button9,button10,
                                 button11,button12,button13,button14,button15,button16,button17,button18,button19,button20,
                                 button21,button22,button23,button24,button25,button26,button27,button28,button29,button30,
                                 button31,button32,button33,button34,button35,button36,button37,button38,button39,button40,
                                 button41,button42,button43,button44,button45,button46,button47,button48,button49,button50,
                                 button51,button52,button53,button54,button55,button56,button57,button58,button59,button60,
                                 button61,button62,button63,button64,button65,button66,button67,button68,button69,button70,
                                 button71,button72,button73,button74,button75,button76,button77,button78,button79,button80,
                                 button81};
            int i;
            for (i = 0; i < 81; i++)
            {
                ((Ellipse)buttons[i].Template.FindName("ellipse", buttons[i])).Fill = System.Windows.Media.Brushes.White;
                buttons[i].Content = "";
            }
            clickedPoint = 0;
        }
    }
    
}
