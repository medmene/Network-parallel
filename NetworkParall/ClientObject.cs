using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;

namespace NetworkParall
{
    public class ClientObject
    {
        public TcpClient client;
        int num; //num of current client
        Form1 fr;
        string txtBox;
        DateTime curTime;

        public ClientObject(TcpClient tcpClient, int _num, Form1 f,string tbox) { client = tcpClient; num = _num; fr = f; txtBox = tbox; }
        
        string GetMsg(NetworkStream stream){
            byte[] data = new byte[2048]; // буфер для получаемых данных
            // получаем сообщение
            string msg = "";
            int bytes = 0;
            bytes = stream.Read(data, 0, data.Length);
            msg = Encoding.UTF8.GetString(data, 0, bytes);             
            return msg;
        }

        public void Process()
        {
            NetworkStream stream = null;
            try
            {
                curTime = DateTime.Now;
                //var Date
                stream = client.GetStream();
                // Буфер для входящих данных
                string data = "";
                int someInt = 0;

                while (true)
                {
                    data = GetMsg(stream);
                    //первое присоединение
                    if (data == "dvc_firstConn")
                    {
                        byte[] msg = Encoding.UTF8.GetBytes(num.ToString() + fr.UserSrcCode);
                        stream.Write(msg, 0, msg.Length);
                        Thread.Sleep(1000);
                    }
                    //отправлять результат
                    else if (data == "sndAnsw")
                    {
                        if (fr.checkBox1.Checked == true)
                        {
                            byte[] msg = Encoding.UTF8.GetBytes("sndRes");
                            stream.Write(msg, 0, msg.Length);
                        }
                        else
                        {
                            byte[] msg = Encoding.UTF8.GetBytes("DontSndRes");
                            stream.Write(msg, 0, msg.Length);
                        }
                        Thread.Sleep(1000);
                    }
                    //количество посылок
                    else if (data == "CountPrc")
                    {
                        if (txtBox != "" && Int32.TryParse(txtBox, out someInt))
                        {
                            byte[] msg = Encoding.UTF8.GetBytes(txtBox);
                            stream.Write(msg, 0, msg.Length);
                        }
                        else
                        {
                            byte[] msg = Encoding.UTF8.GetBytes("100000");
                            stream.Write(msg, 0, msg.Length);
                        }
                        Thread.Sleep(2000);
                    }
                    //процент выполнения
                    else
                    {
                        Thread.Sleep(1000);
                        string[] ss = data.Split('_');
                        if (Int32.TryParse(ss[0], out someInt))
                        {
                            if (ss[1] == "2") fr.progVal2 = someInt;                            
                            if (ss[1] == "3") fr.progVal3 = someInt;
                            if (ss[1] == "4") fr.progVal4 = someInt;  
                            byte[] msg = Encoding.UTF8.GetBytes("good");
                            stream.Write(msg, 0, msg.Length);
                        }
                        //отключение
                        else if (data.IndexOf("<TheEnd>") > -1)
                        {
                            fr.EndClnt(num, DateTime.Now.Subtract(curTime).ToString());
                            //fr.richTextBox1.Text += "Сервер завершил соединение с клиентом." + data[data.Length - 1];
                            break;
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                fr.numberClt--;
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
                fr.numberClt--;
            }
        }
#if false
            if (UserSrcCode != "")
            {

                // Устанавливаем для сокета локальную конечную точку
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);
        progressBar1.Enabled = true;
                progressBar2.Enabled = true;
                progressBar3.Enabled = true;

                // Создаем сокет Tcp/Ip
                sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
                try
                {
                    sListener.Bind(ipEndPoint);
                    sListener.Listen(10);
                    int someInt = 0; //
        int curUser = 1; //выделение номера пользователю
                    // Начинаем слушать соединения
                    while (true)
                    {
                        // Программа приостанавливается, ожидая входящее соединение
                        Socket handler = sListener.Accept();
        string data = null;

        // Буфер для входящих данных
        byte[] bytes = new byte[2048];

        // Мы дождались клиента, пытающегося с нами соединиться
        int bytesRec = handler.Receive(bytes);

        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        /////////////////////////////////////////////////////
                        /////////////////////////////////////////////////////
                        /////////////////////////////////////////////////////
                        //первое присоединение
                        if (data == "dvc_firstConn")
                        {
                            byte[] msg = Encoding.UTF8.GetBytes(curUser.ToString() + UserSrcCode);
        curUser++;
                            handler.Send(msg);
                        }
                        //отправлять результат
                        else if (data == "sndAnsw")
                        {
                            if (checkBox1.Checked == true)
                            {
                                byte[] msg = Encoding.UTF8.GetBytes("sndRes");
    handler.Send(msg);
                            }
                            else
                            {
                                byte[] msg = Encoding.UTF8.GetBytes("DontSndRes");
handler.Send(msg);
                            }
                        }
                        //количество посылок
                        else if (data == "CountPrc")
                        {
                            if (textBox1.Text != "" && Int32.TryParse(textBox1.Text, out someInt))
                            {
                                byte[] msg = Encoding.UTF8.GetBytes(textBox1.Text);
handler.Send(msg);
                            }
                            else
                            {
                                byte[] msg = Encoding.UTF8.GetBytes("100000");
handler.Send(msg);
                            }
                        }
                        //процент выполнения
                        else {
                            string[] ss = data.Split('_');
                            if (Int32.TryParse(ss[0], out someInt))
                            {
                                if (ss[1] == "2") progressBar1.Value = someInt;
                                if (ss[1] == "3") progressBar2.Value = someInt;
                                if (ss[1] == "4") progressBar3.Value = someInt;
                                byte[] msg = Encoding.UTF8.GetBytes("good");
handler.Send(msg);
                            }
                            //отключение
                            else if (data.IndexOf("<TheEnd>") > -1)
                            {
                                richTextBox1.Text += "Сервер завершил соединение с клиентом." + data[data.Length - 1];
                                break;
                            }
                        }

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }
                catch (Exception ex) { richTextBox1.Text += (ex.ToString()); }
            }
            else richTextBox1.Text += "User code is empty!\n";
#endif
    }
}
