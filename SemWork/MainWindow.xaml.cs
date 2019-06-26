using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SemWork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;
        public string GameStatusBar { get { return statusBar.Text; } set { statusBar.Text = value; } }

        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            DataContext = new Game();
        }


        private async void Connect_Click(object sender, RoutedEventArgs e) 
        {
            if (!(DataContext as Game).IsStarted)
            {
                (DataContext as Game).Connect(tb_address.Text, tb_nickname.Text);
                var progress = new Progress<string>(s => statusBar.Text = s);
                await Task.Factory.StartNew(() => Update(progress), TaskCreationOptions.LongRunning);
            }          
        }

        


        private void Update(IProgress<string> progress)
        {
            while (true)
            {
                (DataContext as Game).GetTurn();
            }
        }

        private void RollClick(object sender, RoutedEventArgs e)
        {
            if((DataContext as Game).IsStarted)
            {
                if((DataContext as Game).CanRoll)
                {
                    int dice = (DataContext as Game).Roll6();
                    GameStatusBar = "You rolled " + dice;
                }
                
            }
                
        }
    }
}
