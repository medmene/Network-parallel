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
        public int numReady = 0;
        public bool runHost = false;
        public bool run = false;
        bool prtRes0 = false; string time0 = "";
        bool prtRes1 = false; string time1 = "";
        bool prtRes3 = false; string time3 = "";
        bool prtRes2 = false; string time2 = "";
        public string logMsg="";
        TcpListener listener = null;

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
            if (UserSrcCode != "")
            {
                Thread t = new Thread(new ThreadStart(GetClients));
                t.Start();
                button1.Enabled = false;
            }
            else { MessageBox.Show("UserCode is empty!"); } 
        }

        void GetClients()
        {
            const int port = 8888;
            try
            {
                listener = new TcpListener(IPAddress.Parse(/*"192.168.0.83"*/"127.0.0.1"), port);
                listener.Start();

                while (work)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    numberClt++;
                    ClientObject clientObject = new ClientObject(client, numberClt + 1, this, textBox1.Text);

                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                    if (numberClt == 3) { work = false; }
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
            if (numReady == 3) button4.Enabled = true;
            if (runHost)
            {
                Thread host = new Thread(new ThreadStart(HostFunc));
                host.Start();
                runHost = false;
            }
            if (logMsg != "") { richTextBox1.Text += logMsg; logMsg = ""; }
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

        private void button4_Click(object sender, EventArgs e)
        {
            run = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener.Stop();
        }
    }
}
