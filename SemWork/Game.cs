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
            Chosen1,
            SquareInfo,
            Surrender,
            YouWon,
            YouLost
        }

        Socket sender = null;
        int dice = 0;

        public List<SquareData> Squares { get; private set; }

        public Command<SquareData> SquareClickCommand { get; private set; }

        public bool IsSurrended { get; set; }
        public bool IsYourTurn { get; set; }
        public bool CanRoll { get; set; }
        public string UserColor { get; set; }
        SquareData selSquare;

        public Game()
        {
            IsYourTurn = false;
            Squares = new List<SquareData>();
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
            
            if(IsYourTurn && !CanRoll && square.Color=="Wheat")
            {
                if (selSquare == null) 
                {
                    if ((square.Row == 0 || square.Row == 63) && (square.Column == 0 || square.Column == 63)) 
                    {
                        if (dice != 1)
                        {
                            square.Color = "Black";
                            selSquare = new SquareData() { Column = square.Column, Color = square.Color, Row = square.Row };
                        }
                        else if (dice == 1)
                        {
                            string msg = Recolor1Square(square, UserColor);
                            SendMsg(msg, (int)MsgStatus.Chosen1);
                            IsYourTurn = false;
                        }
                    }
                    else
                    {
                        foreach (SquareData item in Squares)
                        {
                            if (item.Color == UserColor)
                            {
                                if ((Math.Abs(item.Column - square.Column) == 1 && Math.Abs(item.Row - square.Row) == 0) || (Math.Abs(item.Column - square.Column) == 0 && Math.Abs(item.Row - square.Row) == 1))
                                {
                                    if(dice!=1)
                                    {

                                        square.Color = "Black";
                                        selSquare = new SquareData() { Column = square.Column, Color = square.Color, Row = square.Row };
                                    }
                                    else if (dice == 1)
                                    {
                                        string msg = Recolor1Square(square, UserColor);
                                        SendMsg(msg, (int)MsgStatus.Chosen1);
                                        IsYourTurn = false;
                                    }
                                    break;
                                }                               
                            }
                        }
                    }
                    
                }
                else
                {
                    if (Math.Abs(square.Row - selSquare.Row) == dice-1 && Math.Abs(square.Column - selSquare.Column) == dice-1) 
                    {
                        if(CheckInnerSquare(square))
                        {
                            string msg = RecolorSquares(square, selSquare, UserColor);
                            SendMsg(msg, (int)MsgStatus.Chosen);
                            selSquare = null;
                            IsYourTurn = false;
                        }
                    }
                }
                
            }
        }

        private string Recolor1Square(SquareData s, string color)
        {
            foreach(SquareData item in Squares)
                if(item.Row == s.Row && item.Column == s.Column)
                {
                    item.Color = color;
                    return s.Column + " " + s.Row + " ";
                }
            return s.Column + " " + s.Row + " ";
        }

        private string RecolorSquares(SquareData s1, SquareData s2, string color)
        {
            int x1, y1, x2, y2;
            x1 = Math.Min(s1.Column, s2.Column);
            x2 = Math.Max(s1.Column, s2.Column);
            y1 = Math.Min(s1.Row, s2.Row);
            y2 = Math.Max(s1.Row, s2.Row);
            SquareData s_lu = new SquareData() { Row = y1, Column = x1 };
            SquareData s_rd = new SquareData() { Row = y2, Column = x2 };
            foreach (SquareData item in Squares)
            {
                if (item.IsInSquare(s_lu, s_rd))
                {
                    item.Color = color;
                }
            }
            string msg = x1 + " " + y1 + " " + x2 + " " + y2 + " ";
            return msg;
        }

        private bool CheckInnerSquare(SquareData square)
        {
            int x1, y1, x2, y2;
            x1 = Math.Min(square.Column, selSquare.Column);
            x2 = Math.Max(square.Column, selSquare.Column);
            y1 = Math.Min(square.Row, selSquare.Row);
            y2 = Math.Max(square.Row, selSquare.Row);
            SquareData s1 = new SquareData() { Row = y1, Column = x1 };
            SquareData s2 = new SquareData() { Row = y2, Column = x2 };
            foreach(SquareData item in Squares)
            {
                if(item.IsInSquare(s1, s2))
                {
                    if (item.Color == "Wheat" || item.Color == "Black")
                    { }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public int Roll6()
        {
            CanRoll = false;
            Random random = new Random();
            dice = random.Next(1, 6);
            return dice;
        }

        public void Connect(string address, string nickname)
        {
            byte[] bytes = new byte[1024];
            foreach (var item in Squares)
            {
                item.Color = "Wheat";
            }
            try
            {
                IPAddress iPAddress = IPAddress.Parse(address);
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
            try
            {
                string fullMsg = msgStatus + " " + msg + "<EOF>";
                byte[] bytes = Encoding.ASCII.GetBytes(fullMsg);
                sender.Send(bytes);
                return fullMsg;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return "";
            }
            
        }

        private string GetMsg()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return "";
            }
            
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

        public void GetTurn(IProgress<string> progress)
        {
            string data = GetMsg();
            string tmp = data.Substring(0, 2);
            if(Int32.Parse(tmp) == (int)MsgStatus.YourChoice)
            {
                IsYourTurn = true;
                CanRoll = true;
                progress.Report("Your turn!");
            }
            else if(Int32.Parse(tmp) == (int)MsgStatus.SquareInfo)
            {
                tmp = data.Substring(3, 1);
                if (Int32.Parse(tmp) == 1)
                    GetInfo1(data);
                else GetInfo(data);
            }
            else if(Int32.Parse(tmp) == (int)MsgStatus.YouWon)
            {
                MessageBox.Show("Congratulations! You won!");
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                MainWindow.mainWindow.isRunning = false;
            }
            else if(Int32.Parse(tmp) == (int)MsgStatus.YouLost)
            {
                MessageBox.Show("NOOB! You lost!");
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                MainWindow.mainWindow.isRunning = false;
            }
            
        }

        public void GetInfo1(string data)
        {
            data = data.Substring(5);
            int x, y, color;

            int index = data.IndexOf(' ');
            x = Int32.Parse(data.Substring(0, index));
            data = data.Substring(index+1);

            index = data.IndexOf(' ');
            y = Int32.Parse(data.Substring(0, index));
            data = data.Substring(index + 1);

            color = Int32.Parse(data.Substring(0, 1));
            string strColor = "Wheat";
            switch(color)
            {
                case 0: strColor = "Red"; break;
                case 1: strColor = "Blue"; break;
                case 2: strColor = "Green"; break;
                case 3: strColor = "Yellow"; break;
            }
            SquareData s = new SquareData() { Row = y, Column = x };
            Recolor1Square(s, strColor);
        }

        public void GetInfo(string data)
        {
            data = data.Substring(5);
            int x1, x2, y1, y2, color;

            int index = data.IndexOf(' ');
            x1 = Int32.Parse(data.Substring(0, index));
            data = data.Substring(index + 1);

            index = data.IndexOf(' ');
            y1 = Int32.Parse(data.Substring(0, index));
            data = data.Substring(index + 1);

            index = data.IndexOf(' ');
            x2 = Int32.Parse(data.Substring(0, index));
            data = data.Substring(index + 1);

            index = data.IndexOf(' ');
            y2 = Int32.Parse(data.Substring(0, index));
            data = data.Substring(index + 1);

            color = Int32.Parse(data.Substring(0, 1));
            string strColor = "Wheat";
            switch (color)
            {
                case 0: strColor = "Red"; break;
                case 1: strColor = "Blue"; break;
                case 2: strColor = "Green"; break;
                case 3: strColor = "Yellow"; break;
            }
            SquareData s1 = new SquareData() { Row = y1, Column = x1 };
            SquareData s2 = new SquareData() { Row = y2, Column = x2 };
            RecolorSquares(s1, s2, strColor);
        }

        public void SendSurrender()
        {
            SendMsg("Surrender", (int)MsgStatus.Surrender);
            MainWindow.mainWindow.GameStatusBar = "You surrended";
            IsYourTurn = false;
            CanRoll = false;
        }
    }
}
