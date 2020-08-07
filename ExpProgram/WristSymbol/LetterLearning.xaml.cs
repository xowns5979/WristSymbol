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

namespace WristSymbol
{
    /// <summary>
    /// ExpTraining.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LetterLearning : Window
    {
        TextWriter tw;
        
        int trial;
        int trialEnd;
        String quizLetter;
        String[] alphabetSet1 = { "i", "q",
                                 "a", "c", "f", "j", "L", "r", "t", "v",
                                 "b", "d", "e", "h", "n", "p", "s", "u", "x", "y", "z",
                                 "g", "k", "m", "o", "w"};

        String[] alphabetSet = { };
        String[] digitSet1 = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        String[] digitSet = { };

        int clickedPoint = 0;
        int firstPoint = -1;
        int secondPoint = -1;
        int thirdPoint = -1;
        int fourthPoint = -1;
        int fifthPoint = -1;
        int sixthPoint = -1;

        bool letterQuizAnswering;
        String logID;
        int group;

        long startTimestamp;
        long playstamp;
        long enterstamp;
        
        public void setLetterLearning(string id, int group_)
        {
            group = group_;
            logID = id;

            if (group == 0)  // 알파벳 그룹
            {
                trialEnd = alphabetSet.Length;
                trialLabel.Content = trial + " / " + trialEnd;
            }
            else if (group == 1)    // 숫자 그룹
            {
                trialEnd = digitSet.Length;
                trialLabel.Content = trial + " / " + trialEnd;
            }

            //tw = new StreamWriter(logID + "_LetterLearning.csv", true);
            //tw.WriteLine("id,trial#,realPattern,userAnswer,playstamp,enterstamp");
        }
        
        public LetterLearning()
        {
            InitializeComponent();
            startTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            trial = 1;
            trialLabel.Visibility = Visibility.Hidden;
            letterQuizAnswering = false;

            Random rnd = new Random();
            int i;
            for (i = 0; i < 20; i++)
            {
                alphabetSet1 = alphabetSet1.OrderBy(x => rnd.Next()).ToArray();
                alphabetSet = alphabetSet.Concat(alphabetSet1).ToArray();
                digitSet1 = digitSet1.OrderBy(x => rnd.Next()).ToArray();
                digitSet = digitSet.Concat(digitSet1).ToArray();
            }
        }


        public void letterQuizShow(String letter)
        {
            letterLabel.Content = letter;
            return;
        }

        public string edgeWritePattern(string character)
        {
            string str = "";
            switch (character.ToUpper())
            {
                case "A":
                    str = "324";
                    break;
                case "B":
                    str = "1343";
                    break;
                case "C":
                    str = "234";
                    break;
                case "D":
                    str = "2434";
                    break;
                case "E":
                    str = "2134";
                    break;
                case "F":
                    str = "213";
                    break;
                case "G":
                    str = "21343";
                    break;
                case "H":
                    str = "1324";
                    break;
                case "I":
                    str = "13";
                    break;
                case "J":
                    str = "243";
                    break;
                case "K":
                    str = "13234";
                    break;
                case "L":
                    str = "134";
                    break;
                case "M":
                    str = "31424";
                    break;
                case "N":
                    str = "3142";
                    break;
                case "O":
                    str = "21342";
                    break;
                case "P":
                    str = "3123";
                    break;
                case "Q":
                    str = "213424";
                    break;
                case "R":
                    str = "312";
                    break;
                case "S":
                    str = "2143";
                    break;
                case "T":
                    str = "124";
                    break;
                case "U":
                    str = "1342";
                    break;
                case "V":
                    str = "132";
                    break;
                case "W":
                    str = "13242";
                    break;
                case "X":
                    str = "1423";
                    break;
                case "Y":
                    str = "1424";
                    break;
                case "Z":
                    str = "1234";
                    break;
                case "0":
                    str = "213423";
                    break;
                case "1":
                    str = "24";
                    break;
                case "2":
                    str = "3234";
                    break;
                case "3":
                    str = "1243";
                    break;
                case "4":
                    str = "23424";
                    break;
                case "5":
                    str = "21243";
                    break;
                case "6":
                    str = "2343";
                    break;
                case "7":
                    str = "123";
                    break;
                case "8":
                    str = "21432";
                    break;
                case "9":
                    str = "2124";
                    break;
            } 

            return str;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (!letterQuizAnswering)
            {
                playstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;

                correctlabel.Visibility = Visibility.Hidden;
                letterImage.Visibility = Visibility.Hidden;                
                if (group == 0)
                    quizLetter = alphabetSet[trial - 1];
                else if (group == 1)
                    quizLetter = digitSet[trial - 1];
                letterLabel.Content = quizLetter;
                startLabel.Content = "패턴을 클릭하세요.";
                letterQuizAnswering = true;
            }
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            if (letterQuizAnswering)
            {
                startLabel.Content = "";
                String realPattern = edgeWritePattern(letterLabel.Content.ToString());
                String userAnswer = firstPoint.ToString() + secondPoint.ToString() + thirdPoint.ToString() + fourthPoint.ToString() + fifthPoint.ToString() + sixthPoint.ToString();
                userAnswer = userAnswer.Substring(0, clickedPoint);
                String correctStr = "";
                
                if (realPattern == userAnswer)
                {
                    correctlabel.Background = new SolidColorBrush(Color.FromRgb(0x66, 0xff, 0x66));
                    correctlabel.Content = "정답";
                    correctlabel.Visibility = Visibility.Visible;
                    correctStr = "1";
                }
                else
                {
                    correctlabel.Background = new SolidColorBrush(Color.FromRgb(0xff, 0x66, 0x66));
                    correctlabel.Content = "오답";
                    correctlabel.Visibility = Visibility.Visible;
                    correctStr = "0";

                    letterImage.Source = new BitmapImage(new Uri("./img/"+letterLabel.Content.ToString() + ".png", UriKind.Relative));
                    letterImage.Visibility = Visibility.Visible;
                }

                enterstamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTimestamp;
                //tw.WriteLine(logID + "," + trial.ToString() + "," + letterLabel.Content.ToString() + "," + userAnswer + "," + correctStr + "," + playstamp.ToString() + "," + enterstamp.ToString());
                
                
                if (trial == trialEnd)
                {
                    ButtonFinish.Visibility = Visibility.Visible;
                }
                else
                {
                    letterQuizAnswering = false;
                    clearPoints();
                    
                    trial++;
                    trialLabel.Content = trial + " / " + trialEnd;
                }
            }
        }
        
        private void ButtonFinish_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //tw.Close();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 6)
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
                    if (secondPoint != 1)
                    {
                        thirdPoint = 1;
                        button1.Content = "3";
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 3)
                {
                    if (thirdPoint != 1)
                    {
                        fourthPoint = 1;
                        button1.Content = "4";
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 4)
                {
                    if (fourthPoint != 1)
                    {
                        fifthPoint = 1;
                        button1.Content = "5";
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 5)
                {
                    if (fifthPoint != 1)
                    {
                        sixthPoint = 1;
                        button1.Content = "6";
                        Ellipse x1 = (Ellipse)button1.Template.FindName("ellipse", button1);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 6)
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
                    if (secondPoint != 2)
                    {
                        thirdPoint = 2;
                        button2.Content = "3";
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 3)
                {
                    if (thirdPoint != 2)
                    {
                        fourthPoint = 2;
                        button2.Content = "4";
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 4)
                {
                    if (fourthPoint != 1)
                    {
                        fifthPoint = 2;
                        button2.Content = "5";
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 5)
                {
                    if (fifthPoint != 1)
                    {
                        sixthPoint = 2;
                        button2.Content = "6";
                        Ellipse x1 = (Ellipse)button2.Template.FindName("ellipse", button2);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 6)
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
                    if (secondPoint != 3)
                    {
                        thirdPoint = 3;
                        button3.Content = "3";
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 3)
                {
                    if (thirdPoint != 3)
                    {
                        fourthPoint = 3;
                        button3.Content = "4";
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 4)
                {
                    if (fourthPoint != 1)
                    {
                        fifthPoint = 3;
                        button3.Content = "5";
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 5)
                {
                    if (fifthPoint != 1)
                    {
                        sixthPoint = 3;
                        button3.Content = "6";
                        Ellipse x1 = (Ellipse)button3.Template.FindName("ellipse", button3);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
            }
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (clickedPoint < 6)
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
                    if (secondPoint != 4)
                    {
                        thirdPoint = 4;
                        button4.Content = "3";
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 3)
                {
                    if (thirdPoint != 4)
                    {
                        fourthPoint = 4;
                        button4.Content = "4";
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 4)
                {
                    if (fourthPoint != 1)
                    {
                        fifthPoint = 4;
                        button4.Content = "5";
                        Ellipse x1 = (Ellipse)button4.Template.FindName("ellipse", button4);
                        x1.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                        clickedPoint++;
                    }
                }
                else if (clickedPoint == 5)
                {
                    if (fifthPoint != 4)
                    {
                        sixthPoint = 4;
                        button4.Content = "6";
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
            Button[] buttons = { button1, button2, button3, button4 };
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
