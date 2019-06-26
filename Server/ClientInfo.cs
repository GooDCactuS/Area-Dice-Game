using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ClientInfo
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
        public int Points { get; set; }
        public Socket ClientSocket { get; set; }
        public string NickName { get; set; }
        public string Color { get; set; }
        public bool Surrended { get; set; }


        public string SendMsg(string msg, int msgStatus)
        {
            string fullMsg = msgStatus + " " + msg + "<EOF>";
            byte[] bytes = Encoding.ASCII.GetBytes(fullMsg);
            ClientSocket.Send(bytes);
            return fullMsg;
        }

        public string GetMsg()
        {
            byte[] bytes = new Byte[1024];
            string data = null;
            while (true)
            {
                int bytesRec = ClientSocket.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (data.IndexOf("<EOF>") > -1)
                    break;
            }
            int index = data.IndexOf("<EOF>");
            data = data.Substring(0, index);
            return data;
        }

        public string GetNickName()
        {
            string data = GetMsg();
            NickName = data.Substring(3);
            return NickName;
        }

        public string SetColor(int n)
        {
            string playerColor = "White";
            switch (n)
            {
                case 0: playerColor = "Red"; break;
                case 1: playerColor = "Blue"; break;
                case 2: playerColor = "Green"; break;
                case 3: playerColor = "Yellow"; break;
            }
            Color = playerColor;
            string msg = SendMsg(playerColor, (int)MsgStatus.Color);
            return msg;
        }


        public string GetNewSquares()
        {
            string info = GetMsg();
            string status = info.Substring(0, 2);

            info = info.Substring(3);
            string retValue = info;

            if (Int32.Parse(status) == (int)MsgStatus.Chosen)
            {
                int x1, x2, y1, y2;

                int index = info.IndexOf(' ');
                x1 = Int32.Parse(info.Substring(0, index));
                info = info.Substring(index + 1);

                index = info.IndexOf(' ');
                y1 = Int32.Parse(info.Substring(0, index));
                info = info.Substring(index + 1);

                index = info.IndexOf(' ');
                x2 = Int32.Parse(info.Substring(0, index));
                info = info.Substring(index + 1);

                index = info.IndexOf(' ');
                y2 = Int32.Parse(info.Substring(0, index));

                int n = 0;
                switch (Color)
                {
                    case "Red": n = 0; break;
                    case "Blue": n = 1; break;
                    case "Green": n = 2; break;
                    case "Yellow": n = 3; break;
                }
                for (int ii = y1; ii <= y2; ii++)
                {
                    for (int jj = x1; jj <= x2; jj++)
                    {
                        Program.field[ii, jj] = n;
                    }
                }
                retValue = 0 + " " + retValue + n + " ";
            }
            else if(Int32.Parse(status) == (int)MsgStatus.Chosen1)
            {
                int x, y;
                int index = info.IndexOf(' ');
                x = Int32.Parse(info.Substring(0, index));
                info = info.Substring(index + 1);

                index = info.IndexOf(' ');
                y = Int32.Parse(info.Substring(0, index));

                int n = 0;
                switch (Color)
                {
                    case "Red": n = 0; break;
                    case "Blue": n = 1; break;
                    case "Green": n = 2; break;
                    case "Yellow": n = 3; break;
                }
                Program.field[y, x] = n;
                retValue = 1 + " " + retValue + n + " ";
            }
            else if(Int32.Parse(status) == (int)MsgStatus.Surrender)
            {
                Surrended = true;
            }

            return retValue;
        }

        public string SendNewSquaresInfo(string info)
        {
            SendMsg(info, (int)MsgStatus.SquareInfo);
            return info;
        }

        public string SendChoice()
        {
            string data = "Your choice";
            SendMsg(data, (int)MsgStatus.YourChoice);
            return data;
        }

        public string SendWon()
        {
            string data = "You won";
            SendMsg(data, (int)MsgStatus.YouWon);
            return data;
        }

        public string SendLost()
        {
            string data = "You lost";
            SendMsg(data, (int)MsgStatus.YouLost);
            return data;
        }
    }
}
