﻿using System;
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

namespace WristSymbol
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

            ComboboxCond.Items.Add("팔 앞쪽으로");
            ComboboxCond.Items.Add("팔 몸쪽으로");
            ComboboxCond.SelectedIndex = 0;

            ComboboxMode.Items.Add("연습");
            ComboboxMode.Items.Add("본 실험");
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
            int cond = ComboboxCond.SelectedIndex;
            int mode = ComboboxMode.SelectedIndex;

            if (mode == 0)
            {
                ExpTraining program = new ExpTraining();
                program.setExpTraining(serialPort1, logID.Text, cond);
                program.Show();
            }
            else if (mode == 1)
            {
                ExpMain program = new ExpMain();
                program.setExpMain(serialPort1, logID.Text, cond);
                program.Show();
            }
        }
    }
}
