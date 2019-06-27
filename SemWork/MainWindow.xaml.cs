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
        public static Game game;
        public bool isRunning;

        public string GameStatusBar { get { return statusBar.Text; } set { statusBar.Text = value; } }

        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            isRunning = false;
            game = new Game();
            DataContext = game;
        }


        private async void Connect_Click(object sender, RoutedEventArgs e) 
        {
            if (!isRunning)
            {
                isRunning = true;
                game.Connect(tb_address.Text, tb_nickname.Text);
                var progress = new Progress<string>(s => statusBar.Text = s);
                await Task.Factory.StartNew(() => Update(progress), TaskCreationOptions.LongRunning);
            }          
        }

        


        private void Update(IProgress<string> progress)
        {
            while (isRunning)
            {
                if (game.IsYourTurn == false)
                {
                    game.GetTurn(progress);
                }
            }
        }

        private void RollClick(object sender, RoutedEventArgs e)
        {
            if (game.IsYourTurn && game.CanRoll && isRunning) 
            {
                    int dice = game.Roll6();
                    GameStatusBar = "You rolled " + dice;
            }
                
        }

        private void Surrender_Click(object sender, RoutedEventArgs e)
        {
            if(game.IsYourTurn && isRunning)
            {
                game.SendSurrender();
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if(!isRunning)
            {
                var Client = new UdpClient();
                var RequestData = Encoding.ASCII.GetBytes("NeedServer");
                var ServerEp = new IPEndPoint(IPAddress.Any, 0);

                Client.EnableBroadcast = true;
                Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 8888));

                var ServerResponseData = Client.Receive(ref ServerEp);
                tb_address.Text = ServerEp.Address.ToString();
                Client.Close();
            }
        }
    }
}
