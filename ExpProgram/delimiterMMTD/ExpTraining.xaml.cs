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

namespace delimiterMMTD
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
        String[] alphabetSet = { "i", "q",
                                 "a", "c", "f", "j", "l", "r", "t", "v",
                                 "b", "d", "e", "h", "n", "p", "s", "u", "x", "y", "z",
                                 "g", "k", "m", "o", "w",
                                 "i", "q",
                                 "a", "c", "f", "j", "l", "r", "t", "v",
                                 "b", "d", "e", "h", "n", "p", "s", "u", "x", "y", "z",
                                 "g", "k", "m", "o", "w" };
        String[] digitSet = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                              "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};

        String[] quickCalibrationSet = { "1234", "1243", "1342", "1324", "1423", "1432",
                                         "2134", "2143", "2341", "2314", "2413", "2431",
                                         "3124", "3142", "3241", "3214", "3412", "3421",
                                         "4123", "4132", "4213", "4231", "4312", "4321",
                                         "1234", "1243", "1342", "1324", "1423", "1432",
                                         "2134", "2143", "2341", "2314", "2413", "2431",
                                         "3124", "3142", "3241", "3214", "3412", "3421",
                                         "4123", "4132", "4213", "4231", "4312", "4321"};
        int quickCalibrationTrial;
        Boolean quickCalAnswering = false;
        Boolean quickCalPlayed = false;

        int clickedPoint_quickCal = 0;
        int firstPoint_quickCal = -1;
        int secondPoint_quickCal = -1;
        int thirdPoint_quickCal = -1;
        int fourthPoint_quickCal = -1;

        bool patternAnswering;
        int letterNum;

        System.IO.Ports.SerialPort serialPort1 = new SerialPort();
        String logID;
        
        int duration;   // ms
        int isiGap = 100;   // ms
        long startTimestamp;
        long playstamp;
        long playendstamp;
        long enterstamp;

        int group;
        int strategy;
        int armpose;
        String groupStr = "";   // 1(alphabetGroup) 2(digitGroup)
        String strategyStr = "";    // 1(baseline) 2(hetero)
        String armposeStr = "";    // 1(armFront) 2(armBody)

        System.Timers.Timer timer;
        private double secondsToWait;   // ms
        private DateTime startTime;

        bool keyboardEvent = true;
        bool enterButtonEnabled = false;
        
        public void setExpTraining(SerialPort port, String s1, int group_, int strategy_, int armpose_)
        {
            serialPort1 = port;
            logID = s1;
            group = group_;
            strategy = strategy_;
            armpose = armpose_;
            
            duration = 500;

            if (group == 0)
            {
                title.Content = title.Content.ToString() + ": 알파벳 그룹";
                groupStr = "alphabet";
                trialEnd = alphabetSet.Length;
            }
            else if (group == 1)
            {
                title.Content = title.Content.ToString() + ": 숫자 그룹";
                groupStr = "digitGroup";
                trialEnd = digitSet.Length;
            }
            trialLabel.Content = trial + " / " + trialEnd;

            if (strategy == 0)
            {
                title.Content = title.Content.ToString() + ", Baseline";
                strategyStr = "baseline";
                serialPort1.WriteLine("2bs000");
                serialPort1.WriteLine("4bs000");
                /*
                serialPort1.WriteLine("1bs000");
                serialPort1.WriteLine("3bs000");
                */
            }
            else if (strategy == 1)
            {
                title.Content = title.Content.ToString() + ", 2-Hetero";
                strategyStr = "hetero";
                serialPort1.WriteLine("2bv040");
                serialPort1.WriteLine("2bs040");
                serialPort1.WriteLine("4bv040");
                serialPort1.WriteLine("4bs040");
                /*
                serialPort1.WriteLine("1bv040");
                serialPort1.WriteLine("1bs040");
                serialPort1.WriteLine("3bv040");
                serialPort1.WriteLine("3bs040");
                */
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

            tw = new StreamWriter(logID + "_" + groupStr + "_" + strategyStr + "_" + armposeStr + "_training.csv", true);
            tw.WriteLine("id,group,strategy,armpose,trial#,realPattern,userAnswer,correct,playstamp,playendstamp,enterstamp");
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
            quickCalibrationTrial = 1;

            typingCount = 0;
            patternAnswering = false;

            timer = new System.Timers.Timer();
            timer.Interval = 100; // 
            timer.Elapsed += new ElapsedEventHandler(timer_Tick);

            Random rnd = new Random();
            alphabetSet = alphabetSet.OrderBy(x => rnd.Next()).ToArray();
            alphabetSet = alphabetSet.OrderBy(x => rnd.Next()).ToArray();
            digitSet = digitSet.OrderBy(x => rnd.Next()).ToArray();
            digitSet = digitSet.OrderBy(x => rnd.Next()).ToArray();
            quickCalibrationSet = quickCalibrationSet.OrderBy(x => rnd.Next()).ToArray();
            quickCalibrationSet = quickCalibrationSet.OrderBy(x => rnd.Next()).ToArray();

            lineActivate();

            ButtonQuickCalStart.Visibility = Visibility.Hidden;
            ButtonQuickCalEnter.Visibility = Visibility.Hidden;
            button1.Visibility = Visibility.Hidden;
            button2.Visibility = Visibility.Hidden;
            button3.Visibility = Visibility.Hidden;
            button4.Visibility = Visibility.Hidden;
            ButtonQuickCalClear.Visibility = Visibility.Hidden;
            ButtonQuickCalFinish.Visibility = Visibility.Hidden;
            armFrontImg.Visibility = Visibility.Hidden;
            armBodyImg.Visibility = Visibility.Hidden;
        }

        public void stimulation(int tactorNum)
        {
            serialPort1.WriteLine(tactorNum.ToString() + "v");
            Thread.Sleep(duration);
            serialPort1.WriteLine(tactorNum.ToString() + "s");
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
                if (i < n - 1)
                    Thread.Sleep(isiGap);
                 
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

            /*
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
            */
            edgeVibStimulation(arr);
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!quickCalAnswering)
            {
                int i;
                Label[] letters = { letter1, letter2, letter3, letter4 };
                Label[] answers = { answer1, answer2, answer3, answer4 };

                //if (patternAnswering == false)
                //{
                for (i = 0; i < 4; i++)
                {
                    answers[i].Content = "";
                    answers[i].Visibility = Visibility.Hidden;
                    typingCount = 0;
                }
                //if (typingCount == 0)
                //{
                for (i = 0; i < 4; i++)
                {
                    letters[i].Content = "";
                }
                // Random pattern generate

                if (group == 0)
                {
                    playedLetters = alphabetSet[trial - 1];
                    answer1.Content = alphabetSet[trial - 1];
                }
                else if (group == 1)
                {
                    playedLetters = digitSet[trial - 1];
                    answer1.Content = digitSet[trial - 1];
                }

                Thread.Sleep(400);
                workBackground(playedLetters);

                patternAnswering = true;
                //}
                // }
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

                tw.WriteLine(logID + "," + groupStr + "," + strategyStr + "," + armposeStr + "," + trial.ToString() + "," + a + "," + l + "," + correctStr + "," + playstamp.ToString() + "," + playendstamp.ToString() + "," + enterstamp.ToString());
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

                    if (trial % 20 == 0)
                    {
                        secondsToWait = 1000 * 30;
                    }
                    else
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
                {
                    quickCalAnswering = true;
                    Dispatcher.Invoke((Action)delegate ()
                    {
                        clockLabel.Content = "";
                        ButtonQuickCalStart.Visibility = Visibility.Visible;
                        ButtonQuickCalEnter.Visibility = Visibility.Visible;
                        button1.Visibility = Visibility.Visible;
                        button2.Visibility = Visibility.Visible;
                        button3.Visibility = Visibility.Visible;
                        button4.Visibility = Visibility.Visible;
                        ButtonQuickCalClear.Visibility = Visibility.Visible;
                        if (armpose == 0)    //armFront
                            armFrontImg.Visibility = Visibility.Visible;
                        else if (armpose == 1)
                            armBodyImg.Visibility = Visibility.Visible;
                        l1.Visibility = Visibility.Hidden;
                        answer1.Visibility = Visibility.Hidden;

                        enterButtonEnabled = false;
                        ButtonPlay.Visibility = Visibility.Hidden;
                        ButtonEnter.Visibility = Visibility.Hidden;
                    });
                }
                else
                {
                    keyboardEvent = true;
                }
                // run your function
                timer.Stop();
            }
        }
        

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (keyboardEvent)
            {
                Label[] letters = { letter1, letter2, letter3, letter4 };
                if (group == 0)
                {
                    if (e.Key == Key.A || e.Key == Key.B || e.Key == Key.C || e.Key == Key.D || e.Key == Key.E || e.Key == Key.F || e.Key == Key.G || e.Key == Key.H || e.Key == Key.I ||
                    e.Key == Key.J || e.Key == Key.K || e.Key == Key.L || e.Key == Key.M || e.Key == Key.N || e.Key == Key.O || e.Key == Key.P || e.Key == Key.Q || e.Key == Key.R ||
                    e.Key == Key.S || e.Key == Key.T || e.Key == Key.U || e.Key == Key.V || e.Key == Key.W || e.Key == Key.X || e.Key == Key.Y || e.Key == Key.Z)
                    {
                        String typedStr = e.Key.ToString();
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
                }
                else if (group == 1)
                {
                    if (e.Key == Key.D0 || e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3 || e.Key == Key.D4 ||
                        e.Key == Key.D5 || e.Key == Key.D6 || e.Key == Key.D7 || e.Key == Key.D8 || e.Key == Key.D9)
                    {
                        String typedStr = e.Key.ToString();
                        typedStr = typedStr[1].ToString();

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
                }

                if (e.Key == Key.Back)
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

        
        private void ButtonQuickCalStart_Click(object sender, RoutedEventArgs e)
        {
            //quickCalAnswering = true;
            if (quickCalAnswering)
            {
                correctlabel.Visibility = Visibility.Hidden;
                String quizLetter = quickCalibrationSet[quickCalibrationTrial - 1];

                int[] tactorNums = { int.Parse(quizLetter[0].ToString()),
                                 int.Parse(quizLetter[1].ToString()),
                                 int.Parse(quizLetter[2].ToString()),
                                 int.Parse(quizLetter[3].ToString())};

                edgeVibStimulation(tactorNums);
                quickCalPlayed = true;
            }
        }

        private void ButtonQuickCalEnter_Click(object sender, RoutedEventArgs e)
        {
            if (quickCalAnswering && quickCalPlayed)
            {
                String realPattern = quickCalibrationSet[quickCalibrationTrial - 1];
                String userAnswer = firstPoint_quickCal.ToString() + secondPoint_quickCal.ToString() + thirdPoint_quickCal.ToString() + fourthPoint_quickCal.ToString();
                if (armpose == 0)    // armFront
                {
                    int i;
                    int[] userAnswers = { -1, -1, -1, -1 };
                    for (i = 0; i < 4; i++)
                    {
                        if (int.Parse(userAnswer[i].ToString()) == 1)
                            userAnswers[i] = 2;
                        else if (int.Parse(userAnswer[i].ToString()) == 2)
                            userAnswers[i] = 4;
                        else if (int.Parse(userAnswer[i].ToString()) == 3)
                            userAnswers[i] = 1;
                        else if (int.Parse(userAnswer[i].ToString()) == 4)
                            userAnswers[i] = 3;

                    }
                    userAnswer = userAnswers[0].ToString() + userAnswers[1].ToString() + userAnswers[2].ToString() + userAnswers[3].ToString();
                }

                if (realPattern == userAnswer)
                {
                    correctlabel.Background = new SolidColorBrush(Color.FromRgb(0x66, 0xff, 0x66));
                    correctlabel.Content = "정답";
                    correctlabel.Visibility = Visibility.Visible;

                    ButtonQuickCalFinish.Visibility = Visibility.Visible;
                    quickCalibrationTrial++;
                    //quickCalPlayed = false;

                }
                else
                {
                    correctlabel.Background = new SolidColorBrush(Color.FromRgb(0xff, 0x66, 0x66));
                    correctlabel.Content = "오답";
                    correctlabel.Visibility = Visibility.Visible;
                }
                clearPoints();
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint_quickCal < 4)
            {
                if (clickedPoint_quickCal == 0)
                {
                    firstPoint_quickCal = 1;
                    button1.Content = "1";
                    Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                    x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                    clickedPoint_quickCal++;
                }
                else if (clickedPoint_quickCal == 1)
                {
                    if (firstPoint_quickCal != 1)
                    {
                        secondPoint_quickCal = 1;
                        button1.Content = "2";
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
                else if (clickedPoint_quickCal == 2)
                {
                    if (secondPoint_quickCal != 1)
                    {
                        thirdPoint_quickCal = 1;
                        button1.Content = "3";
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
                else if (clickedPoint_quickCal == 3)
                {
                    if (thirdPoint_quickCal != 1)
                    {
                        fourthPoint_quickCal = 1;
                        button1.Content = "4";
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint_quickCal < 4)
            {
                if (clickedPoint_quickCal == 0)
                {
                    firstPoint_quickCal = 2;
                    button2.Content = "1";
                    Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                    x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                    clickedPoint_quickCal++;
                }
                else if (clickedPoint_quickCal == 1)
                {
                    if (firstPoint_quickCal != 2)
                    {
                        secondPoint_quickCal = 2;
                        button2.Content = "2";
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
                else if (clickedPoint_quickCal == 2)
                {
                    if (secondPoint_quickCal != 2)
                    {
                        thirdPoint_quickCal = 2;
                        button2.Content = "3";
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
                else if (clickedPoint_quickCal == 3)
                {
                    if (thirdPoint_quickCal != 2)
                    {
                        fourthPoint_quickCal = 2;
                        button2.Content = "4";
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint_quickCal < 4)
            {
                if (clickedPoint_quickCal == 0)
                {
                    firstPoint_quickCal = 3;
                    button3.Content = "1";
                    Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                    x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                    clickedPoint_quickCal++;
                }
                else if (clickedPoint_quickCal == 1)
                {
                    if (firstPoint_quickCal != 3)
                    {
                        secondPoint_quickCal = 3;
                        button3.Content = "2";
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
                else if (clickedPoint_quickCal == 2)
                {
                    if (secondPoint_quickCal != 3)
                    {
                        thirdPoint_quickCal = 3;
                        button3.Content = "3";
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
                else if (clickedPoint_quickCal == 3)
                {
                    if (thirdPoint_quickCal != 3)
                    {
                        fourthPoint_quickCal = 3;
                        button3.Content = "4";
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
            }
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint_quickCal < 4)
            {
                if (clickedPoint_quickCal == 0)
                {
                    firstPoint_quickCal = 4;
                    button4.Content = "1";
                    Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                    x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                    clickedPoint_quickCal++;
                }
                else if (clickedPoint_quickCal == 1)
                {
                    if (firstPoint_quickCal != 4)
                    {
                        secondPoint_quickCal = 4;
                        button4.Content = "2";
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
                else if (clickedPoint_quickCal == 2)
                {
                    if (secondPoint_quickCal != 4)
                    {
                        thirdPoint_quickCal = 4;
                        button4.Content = "3";
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
                else if (clickedPoint_quickCal == 3)
                {
                    if (thirdPoint_quickCal != 4)
                    {
                        fourthPoint_quickCal = 4;
                        button4.Content = "4";
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint_quickCal++;
                    }
                }
            }
        }

        private void ButtonQuickCalClear_Click(object sender, RoutedEventArgs e)
        {
            clearPoints();
        }

        public void clearPoints()
        {
            Button[] buttons = { button1, button2, button3, button4 };
            int i;
            for (i = 0; i < 4; i++)
            {
                ((Ellipse)buttons[i].Template.FindName("ellipse", buttons[i])).Fill = System.Windows.Media.Brushes.White;
                buttons[i].Content = "";
            }
            clickedPoint_quickCal = 0;
        }

        private void ButtonQuickCalFinish_Click(object sender, RoutedEventArgs e)
        {

            ButtonQuickCalStart.Visibility = Visibility.Hidden;
            ButtonQuickCalEnter.Visibility = Visibility.Hidden;
            button1.Visibility = Visibility.Hidden;
            button2.Visibility = Visibility.Hidden;
            button3.Visibility = Visibility.Hidden;
            button4.Visibility = Visibility.Hidden;
            ButtonQuickCalClear.Visibility = Visibility.Hidden;
            ButtonQuickCalFinish.Visibility = Visibility.Hidden;
            if (armpose == 0)    //armFront
                armFrontImg.Visibility = Visibility.Hidden;
            else if (armpose == 1)
                armBodyImg.Visibility = Visibility.Hidden;
            correctlabel.Visibility = Visibility.Hidden;
            l1.Visibility = Visibility.Visible;

            quickCalAnswering = false;
            enterButtonEnabled = true;
            keyboardEvent = true;
            ButtonPlay.Visibility = Visibility.Visible;
            ButtonEnter.Visibility = Visibility.Visible;
            clearPoints();
        }
    }
}
