using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {


        static Socket listener;
        static ClientInfo[] clients;
        public static int[,] field;

        static int currentPlayers = 4;
        static IPAddress ipAddress;
        static IPEndPoint localEndPoint;

        public static void StartListening()
        {

            field = new int[64, 64];
            for (int ii = 0; ii < 64; ii++)
                for (int jj = 0; jj < 64; jj++)
                    field[ii, jj] = -1;

            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);
                clients = new ClientInfo[4];
                Console.WriteLine("Server started at {0}", ipAddress);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private static void PlayersConnection()
        {

            try
            {
                for (int ii = 0; ii < currentPlayers; ii++)
                {
                    Console.WriteLine("Waiting for a connection...");
                    clients[ii] = new ClientInfo();
                    clients[ii].ClientSocket = listener.Accept();

                    clients[ii].GetNickName();
                    Console.WriteLine("{0} connected.", clients[ii].NickName);

                    clients[ii].SetColor(ii);
                    Console.WriteLine("ColorInfo sent: " + clients[ii].Color);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        private static void GameLoop()
        {
            try
            {
                bool continueGame = true;
                while (continueGame)
                {
                    for (int ii = 0; ii < currentPlayers; ii++)
                    {
                        if (!clients[ii].Surrended)
                        {
                            Console.WriteLine("Waiting for {0} choice...", clients[ii].NickName);
                            if (!(clients[ii].ClientSocket.Poll(0, SelectMode.SelectRead) && clients[ii].ClientSocket.Available == 0))
                            {
                                clients[ii].SendChoice();
                                string info = clients[ii].GetNewSquares();
                                if (!clients[ii].Surrended)
                                {
                                    for (int jj = 0; jj < currentPlayers; jj++)
                                    {
                                        if (ii != jj && !(clients[jj].ClientSocket.Poll(0, SelectMode.SelectRead) && clients[jj].ClientSocket.Available == 0))
                                            clients[jj].SendNewSquaresInfo(info);
                                    }
                                }
                            }

                        }
                    }
                    int surrendedPlayers = 0;
                    for (int ii = 0; ii < currentPlayers; ii++)
                    {
                        if (clients[ii].Surrended || (clients[ii].ClientSocket.Poll(0, SelectMode.SelectRead) && clients[ii].ClientSocket.Available == 0))
                            surrendedPlayers++;
                    }
                    if (surrendedPlayers == currentPlayers)
                    {
                        continueGame = false;
                        int winner = CalculatePoints();
                        for (int p = 0; p < currentPlayers; p++)
                        {
                            if (clients[p].ClientSocket.Poll(0, SelectMode.SelectRead) && clients[p].ClientSocket.Available == 0)
                                continue;
                            if (p != winner)
                                clients[p].SendLost();
                            else
                                clients[p].SendWon();

                            clients[p].ClientSocket.Shutdown(SocketShutdown.Both);
                            clients[p].ClientSocket.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static int CalculatePoints()
        {
            for (int ii = 0; ii < 64; ii++)
            {
                for (int jj = 0; jj < 64; jj++)
                {
                    for (int p = 0; p < currentPlayers; p++)
                    {
                        if (field[ii, jj] == p)
                            clients[p].Points++;
                    }
                }
            }
            int tmp = -1, pl = -1;
            for (int p = 0; p < currentPlayers; p++)
            {
                if (clients[p].Points > tmp)
                {
                    tmp = clients[p].Points;
                    pl = p;
                }
            }
            return pl;
        }

        private static void StartUdp()
        {
            Console.WriteLine("Udp started");
            var Server = new UdpClient(8888);
            var ResponseData = Encoding.ASCII.GetBytes(ipAddress.ToString());

            while (true)
            {
                var ClientEp = new IPEndPoint(IPAddress.Any, 0);
                var ClientRequestData = Server.Receive(ref ClientEp);
                var ClientRequest = Encoding.ASCII.GetString(ClientRequestData);

                Console.WriteLine("Received {0} from {1}, sending response", ClientRequest, ClientEp.Address.ToString());
                Server.Send(ResponseData, ResponseData.Length, ClientEp);
            }

        }

        static void SetIP()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            localEndPoint = new IPEndPoint(ipAddress, 11000);
        }

        static async void StartUdpServer()
        {
            await Task.Factory.StartNew(() => StartUdp(), TaskCreationOptions.LongRunning);
        }

        static void Main(string[] args)
        {
            SetIP();
            StartUdpServer();
            StartListening();
            PlayersConnection();
            GameLoop();
        }
    }
}
