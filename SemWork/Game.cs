using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SemWork
{
    public class Game
    {

        enum MsgStatus
        {
            Connect = 10,
            Color,
            YourChoice,
            Chosen,
            OutOfMoves,
            YouWon,
            YouLose
        }

        Socket sender = null;
        int cube = 0;

        public List<SquareData> Squares { get; private set; }

        public Command<SquareData> SquareClickCommand { get; private set; }

        public bool IsStarted { get; set; }
        public bool CanRoll { get; set; }
        public string UserColor { get; set; }
        SquareData[] selectedSquares;

        public Game()
        {
            IsStarted = false;
            Squares = new List<SquareData>();
            selectedSquares = new SquareData[2];
            for (int ii = 0; ii < 64; ii++) 
            {
                for (int jj = 0; jj < 64; jj++)
                {
                    Squares.Add(new SquareData() { Row = ii, Column = jj, Color = "Wheat" });
                }
            }

            SquareClickCommand = new Command<SquareData>(OnSquareClick);
        }

        private void OnSquareClick(SquareData square)
        {
            if(IsStarted && !CanRoll && square.Color=="Wheat")
            {
                if(selectedSquares[0]==null)
                {
                    if((square.Row==0||square.Row==63)&&(square.Column==0||square.Column==63))
                    {
                        square.Color = "Black";
                        selectedSquares[0] = new SquareData() { Column = square.Column, Color = square.Color, Row = square.Row};
                    }
                    foreach (SquareData item in Squares)
                    {
                        if (item.Color == UserColor && (Math.Abs(item.Column - square.Column) == 1 || Math.Abs(item.Row - square.Row) == 1))
                        {
                            square.Color = "Black";
                            selectedSquares[0] = new SquareData() { Column = square.Column, Color = square.Color, Row = square.Row };
                        }
                    }
                }
                else
                {

                }
                
            }
        }

        public int Roll6()
        {
            CanRoll = false;
            Random random = new Random();
            cube = random.Next(1, 6);
            return cube;
        }

        public void Connect(string address, string nickname)
        {
            byte[] bytes = new byte[1024];

            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress iPAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(address), 11000);

                sender = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    sender.Connect(remoteEP);

                    SendNickname(nickname);
                    GetColor();
                   
                    MainWindow.mainWindow.GameStatusBar = "Socket connected to " + sender.RemoteEndPoint.ToString();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullEx: {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException: {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception: {0}", e.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private string SendMsg(string msg, int msgStatus)
        {
            string fullMsg = msgStatus + " " + msg + "<EOF>";
            byte[] bytes = Encoding.ASCII.GetBytes(fullMsg);
            sender.Send(bytes);
            return fullMsg;
        }

        private string GetMsg()
        {
            byte[] bytes = new Byte[1024];
            string data = null;
            while (true)
            {
                int bytesRec = sender.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (data.IndexOf("<EOF>") > -1)
                    break;
            }
            int index = data.IndexOf("<EOF>");
            data = data.Substring(0, index);

            return data;
        }

        public string SendNickname(string nickname)
        {
            string data = SendMsg(nickname, (int)MsgStatus.Connect);
            return data;
        }

        public string GetColor()
        {
            string data = GetMsg();
            UserColor = data.Substring(3);
            return data;
        }

        public void GetTurn()
        {
            string data = GetMsg();
            string tmp = data.Substring(0, 2);
            if(Int32.Parse(tmp) == (int)MsgStatus.YourChoice)
            {
                IsStarted = true;
                CanRoll = true;
            }

        }
    }
}
