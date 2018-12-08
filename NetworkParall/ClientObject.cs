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
                        fr.logMsg = "Device: " + (num-1).ToString() + " conneced\n";
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
                    }
                    else if (data == "ProgramAssembled")
                    {
                        fr.logMsg = "Programm on: " + (num-1).ToString() + " assembled\n";
                        byte[] msg = Encoding.UTF8.GetBytes("good");
                        fr.numReady++;
                        while (!fr.run) { }
                        if (fr.runHost == false) fr.runHost = true;
                        stream.Write(msg, 0, msg.Length);
                        curTime = DateTime.Now;
                    }
                    //процент выполнения
                    else
                    {
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
    }
}
