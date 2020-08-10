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
using System.Threading;
using System.ComponentModel;
using System.Timers;
using System.IO;

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
        int correctCount;
        int trialEnd;
        int typingCount;
        String playedLetters;
        /*
        String[] letterSet = { "a", "c", "f", "j", "l", "r", "t", "v", "7",
                               "a", "c", "f", "j", "l", "r", "t", "v", "7"};
        */
        String[] letterSet = { "b", "d", "e", "h", "n", "p", "s", "u", "x", "y", "z",
                               "b", "d", "e", "h", "n", "p", "s", "u", "x", "y", "z",
                               "b", "d", "e", "h", "n", "p", "s", "u", "x", "y", "z"};
        bool patternAnswering;
        int letterNum;

        System.IO.Ports.SerialPort serialPort1 = new SerialPort();
        String logID;
        
        int duration;   // ms
        //int oneLetterTime;

        long startTimestamp;
        long playstamp;
        long playendstamp;
        long enterstamp;

        int orientation;
        int armpose;
        String orientationStr = "";    // 1(handNorth) 2(watchNorth)
        String armposeStr = "";    // 1(armFront) 2(armBody) 3(armDown)
        //int interletterinterval;

        System.Timers.Timer timer;
        private double secondsToWait;   // ms
        private DateTime startTime;

        bool keyboardEvent = true;
        bool enterButtonEnabled = false;
        //bool fixedLength = false;
        
        public void setExpTraining(SerialPort port, String s1, int orientation_, int armpose_)
        {
            serialPort1 = port;
            logID = s1;
            orientation = orientation_;
            armpose = armpose_;
            
            //oneLetterTime = 2000;
            duration = 500;
            //interletterinterval = 2000;

            if (orientation == 1)
            {
                title.Content = title.Content.ToString() + ": 방위 기준 1";
                orientationStr = "handNorth";
            }
            else if (orientation == 2)
            {
                title.Content = title.Content.ToString() + ": 방위 기준 2";
                orientationStr = "watchNorth";
            }

            if (armpose == 0)
            {
                title.Content = title.Content.ToString() + ", 팔 앞";
                armposeStr = "armFront";
            }
            else if (armpose == 1)
            {
                title.Content = title.Content.ToString() + ", 팔 몸";
                armposeStr = "armBody";
            }
            else if (armpose == 2)
            {
                title.Content = title.Content.ToString() + ", 팔 아래";
                armposeStr = "armDown";
            }

            tw = new StreamWriter(logID + "_" + orientationStr + "_" + armposeStr + "_training.csv", true);
            tw.WriteLine("id,orientation,armpose,trial#,realPattern,userAnswer,correct,playstamp,playendstamp,enterstamp");
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
            playstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;
            patternGenerate(text);
            playendstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;
        }

        public void workBackgroundStim(int tactorNum)
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_stimulation);
            worker.RunWorkerAsync(argument: tactorNum);
        }

        void worker_stimulation(object sender, DoWorkEventArgs e)
        {
            int tactorNum = (int)e.Argument;
            stimulation(tactorNum);
        }
        
        private void lineActivate()
        {
            int i;
            letterNum = 1;

            Line[] lines = { l1, l2, l3, l4 };
            Label[] letters = { letter1, letter2, letter3, letter4 };
            typingCount = 0;
            for (i = 0; i < letterNum; i++)
            {
                lines[i].Visibility = Visibility.Hidden;
                letters[i].Content = "";
            }
            int start = 960 - (100 * letterNum + 20 * (letterNum - 1)) / 2;
            for (i = 0; i < letterNum; i++)
            {
                lines[i].X1 = start;
                lines[i].X2 = start + 100;
                lines[i].Visibility = Visibility.Visible;
                letters[i].Margin = new Thickness(start, 0, 0, 540);
                start = start + 120;
            }
        }

        public ExpTraining()
        {
            InitializeComponent();
            startTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            correctCount = 0;
            trial = 1;
            trialEnd = letterSet.Length;
            trialLabel.Content = trial + " / " + trialEnd;

            typingCount = 0;
            patternAnswering = false;

            timer = new System.Timers.Timer();
            timer.Interval = 100; // 
            timer.Elapsed += new ElapsedEventHandler(timer_Tick);

            Random rnd = new Random();
            letterSet = letterSet.OrderBy(x => rnd.Next()).ToArray();
            letterSet = letterSet.OrderBy(x => rnd.Next()).ToArray();
            
            lineActivate();
        }

        public void stimulation(int tactorNum)
        {
            serialPort1.WriteLine("ev" + tactorNum.ToString());
            Thread.Sleep(duration);
            serialPort1.WriteLine("es" + tactorNum.ToString());
        }
        
        public void patternGenerate(String text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                edgeVibPattern(text[i].ToString());
                /*
                if (text.Length > i + 1)
                    Thread.Sleep(interletterinterval);
                */
            }
            return;
        }

        public void edgeVibStimulation(int[] tactorNums)
        {
            
            int n = tactorNums.Length;
            /*
            if (fixedLength)
            {
                duration = oneLetterTime / n;
            }
            */
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
                    arr = new int[] { 3, 2, 4 };
                    break;
                case "B":
                    arr = new int[] { 1, 3, 4, 3 };
                    break;
                case "C":
                    arr = new int[] { 2, 3, 4 };
                    break;
                case "D":
                    arr = new int[] { 2, 4, 3, 4 };
                    break;
                case "E":
                    arr = new int[] { 2, 1, 3, 4 };
                    break;
                case "F":
                    arr = new int[] { 2, 1, 3 };
                    break;
                case "G":
                    arr = new int[] { 2, 1, 3, 4, 3 };
                    break;
                case "H":
                    arr = new int[] { 1, 3, 2, 4 };
                    break;
                case "I":
                    arr = new int[] { 1, 3 };
                    break;
                case "J":
                    arr = new int[] { 2, 4, 3 };
                    break;
                case "K":
                    arr = new int[] { 1, 3, 2, 3, 4 };
                    break;
                case "L":
                    arr = new int[] { 1, 3, 4 };
                    break;
                case "M":
                    arr = new int[] { 3, 1, 4, 2, 4 };
                    break;
                case "N":
                    arr = new int[] { 3, 1, 4, 2 };
                    break;
                case "O":
                    arr = new int[] { 2, 1, 3, 4, 2 };
                    break;
                case "P":
                    arr = new int[] { 3, 1, 2, 3 };
                    break;
                case "Q":
                    arr = new int[] { 2, 1, 3, 4, 2, 4 };
                    break;
                case "R":
                    arr = new int[] { 3, 1, 2 };
                    break;
                case "S":
                    arr = new int[] { 2, 1, 4, 3 };
                    break;
                case "T":
                    arr = new int[] { 1, 2, 4 };
                    break;
                case "U":
                    arr = new int[] { 1, 3, 4, 2 };
                    break;
                case "V":
                    arr = new int[] { 1, 3, 2 };
                    break;
                case "W":
                    arr = new int[] { 1, 3, 2, 4, 2 };
                    break;
                case "X":
                    arr = new int[] { 1, 4, 2, 3 };
                    break;
                case "Y":
                    arr = new int[] { 1, 4, 2, 4 };
                    break;
                case "Z":
                    arr = new int[] { 1, 2, 3, 4 };
                    break;
                case "0":
                    arr = new int[] { 2, 1, 3, 4, 2, 3 };
                    break;
                case "1":
                    arr = new int[] { 2, 4 };
                    break;
                case "2":
                    arr = new int[] { 3, 2, 3, 4 };
                    break;
                case "3":
                    arr = new int[] { 1, 2, 4, 3 };
                    break;
                case "4":
                    arr = new int[] { 2, 3, 4, 2, 4 };
                    break;
                case "5":
                    arr = new int[] { 2, 1, 2, 4, 3 };
                    break;
                case "6":
                    arr = new int[] { 2, 3, 4, 3 };
                    break;
                case "7":
                    arr = new int[] { 1, 2, 3 };
                    break;
                case "8":
                    arr = new int[] { 2, 1, 4, 3, 2 };
                    break;
                case "9":
                    arr = new int[] { 2, 1, 2, 4 };
                    break;
            }
            if (orientation == 2)
            {
                int i;
                for (i = 0; i < arr.Length; i++)
                {
                    if (arr[i] == 1)
                    {
                        arr[i] = 3;
                    }
                    else if (arr[i] == 2)
                    {
                        arr[i] = 1;
                    }
                    else if (arr[i] == 3)
                    {
                        arr[i] = 4;
                    }
                    else if (arr[i] == 4)
                    {
                        arr[i] = 2;
                    }
                }
            }
            
            edgeVibStimulation(arr);
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            int i;
            Label[] letters = { letter1, letter2, letter3, letter4 };
            Label[] answers = { answer1, answer2, answer3, answer4 };

            if (patternAnswering == false)
            {
                for (i = 0; i < 4; i++)
                {
                    answers[i].Content = "";
                    answers[i].Visibility = Visibility.Hidden;
                }
                if (typingCount == 0)
                {
                    for (i = 0; i < 4; i++)
                    {
                        letters[i].Content = "";
                    }
                    // Random pattern generate
                    playedLetters = "";
                    playedLetters = playedLetters + letterSet[trial - 1];
                    answer1.Content = letterSet[trial - 1];
                    
                    Thread.Sleep(400);
                    workBackground(playedLetters);

                    patternAnswering = true;
                }
            }
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            //Label[] letters = { letter1, letter2, letter3, letter4 };
            //Label[] answers = { answer1, answer2, answer3, answer4 };

            if (patternAnswering == true)
            {
                int i;
                //+ Logging add
                String l = letter1.Content.ToString();
                String a = answer1.Content.ToString();

                //String a = answer1.Content.ToString() + answer2.Content.ToString() + answer3.Content.ToString() + answer4.Content.ToString();
                String correctStr = "";
                if (l == a)
                {
                    correctStr = "1";
                    correctCount = correctCount + 1;
                    answer1.Background = new SolidColorBrush(Color.FromRgb(0x66, 0xff, 0x66));
                    answer1.Visibility = Visibility.Visible;
                }
                else
                {
                    correctStr = "0";
                    answer1.Background = new SolidColorBrush(Color.FromRgb(0xff, 0x66, 0x66));
                    answer1.Visibility = Visibility.Visible;
                }
                
                enterstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;

                tw.WriteLine(logID + "," + orientationStr + "," + armposeStr + "," + trial.ToString() + "," + a + "," + l + "," + correctStr + "," + playstamp.ToString() + "," + playendstamp.ToString() + "," + enterstamp.ToString());
                typingCount = 0;
                letter1.Content = "";

                if (trial == trialEnd)
                {
                    //scoreLabel.Content = "점수: " + (correctCount * 100 / letterSet.Length) + "점";
                    ButtonPlay.Visibility = Visibility.Hidden;
                    ButtonFinish.Visibility = Visibility.Visible;
                }
                else
                {
                    patternAnswering = false;

                    secondsToWait = 1000;
                    enterButtonEnabled = false;
                    ButtonEnter.Visibility = Visibility.Hidden;
                    breaktime();

                    trial++;
                    trialLabel.Content = trial + " / " + trialEnd;
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

            if (secondsToWait == 1000 * 30)
            {
                TimeSpan t1 = TimeSpan.FromMilliseconds(remainingSeconds);
                string str = t1.ToString(@"mm\:ss");
                Dispatcher.Invoke((Action)delegate () { clockLabel.Content = str;/* update UI */ });
            }

            if (remainingSeconds <= 0)
            {
                Dispatcher.Invoke((Action)delegate () { ButtonPlay.Visibility = Visibility.Visible; /* update UI */ });
                if (secondsToWait == 1000 * 30)
                    Dispatcher.Invoke((Action)delegate () { clockLabel.Content = "";/* update UI */ });
                keyboardEvent = true;
                // run your function
                timer.Stop();
            }
        }


        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (keyboardEvent)
            {
                Label[] letters = { letter1, letter2, letter3, letter4 };
                //if(e.Key == Key.A || e.Key == Key.C || e.Key == Key.F || e.Key == Key.J || e.Key == Key.L || e.Key == Key.R || e.Key == Key.T || e.Key == Key.V || e.Key == Key.D7)
                if (e.Key == Key.B || e.Key == Key.D || e.Key == Key.E || e.Key == Key.H || e.Key == Key.N || e.Key == Key.P || e.Key == Key.S || e.Key == Key.U || e.Key == Key.X || e.Key == Key.Y || e.Key == Key.Z)
                {
                    String typedStr = e.Key.ToString();
                    //if (typedStr == "D7")
                    if (typedStr == "D2" || typedStr == "D3" || typedStr == "D6" || typedStr == "D9")
                        typedStr = typedStr[1].ToString();
                    else
                        typedStr = typedStr.ToLower();
                    
                    if (patternAnswering == false)
                    {
                        workBackground(typedStr);
                    }
                    else
                    {
                        if (typingCount < letterNum)
                        {
                            letters[typingCount].Content = typedStr;
                            typingCount++;
                            if (typingCount == letterNum)
                            {
                                enterButtonEnabled = true;
                                ButtonEnter.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
                else if (e.Key == Key.Back)
                {
                    if (typingCount > 0)
                    {
                        letters[typingCount - 1].Content = "";
                        typingCount--;
                        if (typingCount == letterNum - 1)
                        {
                            enterButtonEnabled = false;
                            ButtonEnter.Visibility = Visibility.Hidden;
                        }
                    }
                }
                else if (e.Key == Key.Space)
                {
                    ButtonPlay_Click(sender, e);
                }
                else if (e.Key == Key.Enter && enterButtonEnabled)
                {
                    ButtonEnter_Click(sender, e);
                }
            }
        }

        private void ButtonFinish_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tw.Close();
        }
    }
}
