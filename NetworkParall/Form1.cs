using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
////////////////////////////
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace NetworkParall
{
    public partial class Form1 : Form
    {
        string filepath = "";
        string UserSrcCode = "";
        Socket sListener;
        Socket sender;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (UserSrcCode != ""){
                // Устанавливаем для сокета локальную конечную точку
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

                // Создаем сокет Tcp/Ip
                sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                
#if false
                // Соединяем сокет с удаленной точкой
                sListener.Connect(ipEndPoint);

                richTextBox1.Text += "Сокет соединяется с "+ sListener.RemoteEndPoint.ToString();
                byte[] msg = Encoding.UTF8.GetBytes(UserSrcCode);
                // Отправляем данные через сокет
                int bytesSent = sListener.Send(msg);

                // Получаем ответ от сервера
                int bytesRec = sListener.Receive(bytes);

                richTextBox1.Text += "\nОтвет от сервера: "+Encoding.UTF8.GetString(bytes, 0, bytesRec) + "\n\n";
#endif
            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
                {
                    sListener.Bind(ipEndPoint);
                    sListener.Listen(10);

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
                        if(data == "dvc_firstConn")
                        {
                            // Отправляем ответ клиенту
                            byte[] msg = Encoding.UTF8.GetBytes(UserSrcCode);
                            handler.Send(msg);
                        }

                        if (data.IndexOf("<TheEnd>") > -1)
                        {
                            richTextBox1.Text += "Сервер завершил соединение с клиентом.";
                            break;
                        }

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }
                catch (Exception ex) {richTextBox1.Text += (ex.ToString());}

            }
        }

        private void button2_Click(object sender, EventArgs e){
            OpenFileDialog od = new OpenFileDialog();
            if (od.ShowDialog() == DialogResult.OK){
                filepath = od.FileName;
                using (StreamReader sr = new StreamReader(filepath)){
                    UserSrcCode= sr.ReadToEnd();
                }
                richTextBox1.Text += "Source code loaded!\n";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            UserCode uc = new UserCode();
            Data.Value = "";
            uc.ShowDialog();
            UserSrcCode = Data.Value;
            richTextBox1.Text += "Source code loaded!\n";
        }
    }
}
