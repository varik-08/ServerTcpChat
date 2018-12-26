using ChatServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerTcpChat
{
    public partial class Form1 : Form
    {
        string path = @"text.txt";
        static ServerObj server; // сервер
        static Thread listenThread; // потока для прослушивания


        public Form1()
        {

            using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
            {
                sw.Write("");
            }

            InitializeComponent();

            try
            {
                server = new ServerObj();
                listenThread = new Thread(new ThreadStart(server.Listen));

                listenThread.Start(); //старт потока

                timer1.Start();

            }
            catch (Exception ex)
            {
                server.Disconnect();
                textBox1.Text += ex.Message + "\n";
            }
        }

        public void print()
        {
            try
            {
                using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
                {
                    textBox1.Text = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            server.Disconnect();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            print();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                string path = @"banUser.txt";

                using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
                {
                    sw.Write(textBox1.Text);
                }
            }
        }
    }
}
