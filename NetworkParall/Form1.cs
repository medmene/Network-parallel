#define Multithread
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
using System.Threading;
using System.Diagnostics;

namespace NetworkParall
{
    public partial class Form1 : Form
    {
        string filepath = "";
        public string UserSrcCode = "";
        public int numberClt = 0;
        public int progVal1 = 0;
        public int progVal2 = 0;
        public int progVal3 = 0;
        public int progVal4 = 0;
        bool work = true;
        bool prtRes0 = false; string time0 = "";
        bool prtRes1 = false; string time1 = "";
        bool prtRes3 = false; string time3 = "";
        bool prtRes2 = false; string time2 = "";


        public Form1() { InitializeComponent(); timer1.Enabled = true; }

        public void EndClnt(int num,string tim)
        {
            switch (num)
            {
                case 1: prtRes0 = true; time0 = tim; break;
                case 2: prtRes1 = true; time1 = tim; break;
                case 4: prtRes3 = true; time3 = tim; break;
                case 3: prtRes2 = true; time2 = tim; break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
#if Multithread
            if (UserSrcCode != "")
            {
                Thread t = new Thread(new ThreadStart(GetClients));
                t.Start();
                Thread host = new Thread(new ThreadStart(HostFunc));
                host.Start();
                button1.Enabled = false;
#if false
                const int port = 8888;
                TcpListener listener = null;
                try
                {
                    listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                    listener.Start();
                    while (true)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        numberClt++;
                        ClientObject clientObject = new ClientObject(client, numberClt+1, this, textBox1.Text);
                        // создаем новый поток для обслуживания нового клиента
                        Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                        clientThread.Start();
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                finally { if (listener != null) listener.Stop(); }
#endif
            }
            else { MessageBox.Show("UserCode is empty!"); }          
#else
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

        void GetClients()
        {
            const int port = 8879;
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Parse("192.168.0.83"/*"127.0.0.1"*/), port);
                listener.Start();

                while (work)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    numberClt++;
                    ClientObject clientObject = new ClientObject(client, numberClt + 1, this, textBox1.Text);

                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                    if (numberClt == 2000) { work = false; }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            finally { if (listener != null) listener.Stop(); }
        }

        void HostFunc()
        {
            DateTime curTime= DateTime.Now;
            /////////////////////////////////////////////
            ////////////GENERATE execute FILE////////////
            /////////////////////////////////////////////
            Dictionary<string, string> providerOptions = new Dictionary<string, string>
                {
                    {"CompilerVersion", "v3.5"}
                };
            CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);

            //создание директории,если её нет
            if (!Directory.Exists("C:\\11")) { Directory.CreateDirectory("C:\\11"); }

            CompilerParameters compilerParams = new CompilerParameters
            { OutputAssembly = "C:\\11\\Foo.EXE", GenerateExecutable = true };
            compilerParams.ReferencedAssemblies.Add("System.Core.Dll");

            // Компиляция 
            CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, UserSrcCode);

            // Выводим информацию об ошибках 
            using (StreamWriter outputFile = new StreamWriter("CompileLog.txt"))
            {
                outputFile.WriteLine("Number of Errors: {0}", results.Errors.Count);
                foreach (CompilerError err in results.Errors)
                {
                    //richTextBox1.Text += "ERROR " + err.ErrorText + "\n";
                    outputFile.WriteLine("ERROR {0}", err.ErrorText);
                }
            }
            /////////////////////////////////////////////
            ////////////GENERATE execute FILE////////////
            /////////////////////////////////////////////

            Process execProg = new Process();
            ProcessStartInfo msi = new ProcessStartInfo("C:\\11\\Foo.EXE")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = "250000",
                WorkingDirectory = Path.GetDirectoryName("C:\\11\\Foo.EXE"),
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            //run de process
            execProg.StartInfo = msi;
            execProg.Start();

            while (true)
            {
                string output = execProg.StandardOutput.ReadLine();
                if (output == "Done")
                {
                    // преобразуем сообщение в массив байтов
                    byte[] data = Encoding.UTF8.GetBytes("<TheEnd>");
                    // отправка сообщения
                    //stream.Write(data, 0, data.Length);
                    break;
                }
                if (output == null) break;
                progVal1 = Convert.ToInt32(output);
            }
            EndClnt(1, DateTime.Now.Subtract(curTime).ToString());
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
            if (Data.Value != ""){
                UserSrcCode = Data.Value;
                richTextBox1.Text += "Source code loaded!\n";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Value = progVal1;
            progressBar2.Value = progVal2;
            progressBar3.Value = progVal3;
            progressBar4.Value = progVal4;
            if (prtRes0) { richTextBox1.Text += "Time Host: 0 is: " + time0 + "\n"; prtRes0 = false; }
            if (prtRes1) { richTextBox1.Text += "Time client: 1 is:" + time1 + "\n"; prtRes1 = false; }
            if (prtRes2) { richTextBox1.Text += "Time client: 2 is:" + time2 + "\n"; prtRes2 = false; }
            if (prtRes3) { richTextBox1.Text += "Time client: 3 is:" + time3 + "\n"; prtRes3 = false; }
            if (progressBar2.Value==100) { button1.Enabled = true; }
        }
    }
}
