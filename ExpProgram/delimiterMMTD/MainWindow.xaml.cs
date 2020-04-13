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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;

namespace delimiterMMTD
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        System.IO.Ports.SerialPort serialPort1 = new SerialPort();
        
        public MainWindow()
        {
            InitializeComponent();

            ComboboxMode.Items.Add("A, 1 Letter 리마인드");
            ComboboxMode.Items.Add("A, 2 Letter 연습");
            ComboboxMode.Items.Add("A, 2 Letter 간격 Long");
            ComboboxMode.Items.Add("A, 2 Letter 간격 Middle");
            ComboboxMode.Items.Add("A, 2 Letter 간격 Short");

            ComboboxMode.Items.Add("B, 1 Letter 리마인드");
            ComboboxMode.Items.Add("B, 2 Letter 연습");
            ComboboxMode.Items.Add("B, 2 Letter 간격 Long");
            ComboboxMode.Items.Add("B, 2 Letter 간격 Middle");
            ComboboxMode.Items.Add("B, 2 Letter 간격 Short");
           
            ComboboxMode.SelectedIndex = 0;
            
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            String[] ports = SerialPort.GetPortNames();
            ComboboxSerials.Items.Clear();
            for (int i = 0; i < ports.Length; i++)
            {
                ComboboxSerials.Items.Add(ports[i]);
            }
            if (ports.Length > 0)
            {
                ComboboxSerials.SelectedIndex = ComboboxSerials.Items.Count - 1;
                serialPort1.BaudRate = 115200;
                serialPort1.DtrEnable = true;
                serialPort1.RtsEnable = true;
            }
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                serialPort1.PortName = (String)ComboboxSerials.Items[ComboboxSerials.SelectedIndex];
                serialPort1.Open();
                string line = serialPort1.ReadExisting();
                Console.WriteLine("Start");
            }
            else
            {
                serialPort1.Close();
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            int expMode = ComboboxMode.SelectedIndex;
            if (expMode == 0 || expMode == 5)
            {
                OneLetterRemind program = new OneLetterRemind();
                program.setOneLetterRemind(serialPort1, logID.Text, 
                                        ComboboxMode.SelectedIndex);
                program.Show();
            }
            else if (expMode == 1 || expMode == 6)   // Training
            {
                ExpTraining program = new ExpTraining();
                program.setExpTraining(serialPort1, logID.Text,
                                        ComboboxMode.SelectedIndex);
                program.Show();
            }
            else if (expMode == 2 || expMode == 3 || expMode == 4 || expMode == 7 || expMode == 8 || expMode == 9)      // Main
            {
                ExpMain program = new ExpMain();
                program.setExpMain(serialPort1, logID.Text,
                                        ComboboxMode.SelectedIndex);
                program.Show();
            }
        }
        

        
    }
}
