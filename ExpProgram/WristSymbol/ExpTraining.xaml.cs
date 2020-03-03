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

        String[] letterSet = { "12","14","13","24","23","21","31","32","34","43","41","42"};
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
        
        System.Timers.Timer timer;
        private double secondsToWait;   // ms
        private DateTime startTime;

        int expCond = -1;
        String condStr = "";

        public void setExpTraining(SerialPort port, String s1, int cond)
        {
            serialPort1 = port;
            logID = s1;
            duration = 500;
            expCond = cond;

            if (cond == 0)
            {
                condStr = "A(Baseline1)";
                title.Content = title.Content + ": A";
            }
            else if (cond == 1)
            {
                condStr = "B(Approach)";
                title.Content = title.Content + ": B";
            }
            else if (cond == 2)
            {
                condStr = "C(Baseline2)";
                title.Content = title.Content + ": C";
            }

            tw = new StreamWriter(logID + "_" + condStr + "_training"+ ".csv", true);
            tw.WriteLine("id,cond,trial#,realPattern,userAnswer,correct,c1,c2,playstamp,playendstamp,enterstamp,reactionTime");
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
            //funnelingPattern(text);
            patternGenerate(text);
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

        public ExpTraining()
        {
            InitializeComponent();
           
            startTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            trial = 1;
            trialEnd = 6;
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
            //int[] arr = { (int)Char.GetNumericValue(text[0]), (int)Char.GetNumericValue(text[1]), (int)Char.GetNumericValue(text[2]) };
            int[] arr = { (int)Char.GetNumericValue(text[0]), (int)Char.GetNumericValue(text[1])};
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
                //workBackground("f2");

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
            if (patternAnswering == true && clickedPoint == 2)
            {
                
                patternAnswering = false;
                
                
                String a = answer1.Content.ToString();
                
                enterstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;

                String correctStr = "";
                //String userAnswer = firstPoint.ToString() + secondPoint.ToString() + thirdPoint.ToString();
                String userAnswer = firstPoint.ToString() + secondPoint.ToString();
                if (a == userAnswer)
                    correctStr = "1";
                else
                    correctStr = "0";


                string c1Str = "";
                string c2Str = "";

                if (a[0] == userAnswer[0])
                    c1Str = "1";
                else
                    c1Str = "0";

                if (a[1] == userAnswer[1])
                    c2Str = "1";
                else
                    c2Str = "0";


                long rt = enterstamp - playendstamp;

                tw.WriteLine(logID+","+ condStr + "," + trial.ToString() + "," + a + ","+userAnswer+"," + correctStr +","+ c1Str+","+c2Str+","+ playstamp.ToString() + "," + playendstamp.ToString() + "," + enterstamp.ToString() +","+rt.ToString());
                
                

                //String filename = logID + "_" + trial + ".png";
                //SaveClipboardImageToFile(CopyScreen(1255,465,0,0), filename);

                clearPoints();

                if (trial == trialEnd)
                    this.Close();
                else if (trial % 24 == 0)
                {
                    secondsToWait = 20000;
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
            if (clickedPoint < 2)
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
                
            }

        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 2)
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
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 2)
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
            }
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 2)
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
