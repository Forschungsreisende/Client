using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets; //Для работы с сокетами
using System.IO; //Для работы с файлами
using System.Threading;

namespace Client
{
    public partial class Form1 : Form
    {
        static private Socket Client;
        private IPAddress ip = null;
        private int port = 0;
        private Thread th; //Создаём поток
        public Form1()
        {
            InitializeComponent();
            //Блокируем окна и кнопку "Отправить"
            richTextBox1.Enabled = false;
            richTextBox2.Enabled = false;
            button1.Enabled = false;
            //Считывание из файла настройки и дальнейшее их использование
            try //обработчик ошибок
            {
                var sr = new StreamReader(@"Client_info/data_info.txt");
                string buffer = sr.ReadToEnd();
                sr.Close();
                //парсим файл
                string[] connect_info = buffer.Split(':');//Делим на две части по знаку ":"
                ip = IPAddress.Parse(connect_info[0]);
                port = int.Parse(connect_info[1]);
                //Выводим сообщение о том, что всё получилось
                label4.ForeColor = Color.Green;
                label4.Text = "настройки: \n IP сервера: " + connect_info[0] + "\n Порт сервера: " + connect_info[1];
            }
            catch (Exception ex)
            {
                //Кидаем пользователя в настройки
                label4.ForeColor = Color.Red;
                label4.Text = "Отсутствуют настройки!";
                Form2 form = new Form2();
                form.Show();
            }
        }
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            form.Show();
        }
        void SendMessage(string message)
        {
            if (message != " " && message != "")
            {
                byte[] buffer = new byte[1024];
                buffer = Encoding.UTF8.GetBytes(message);
                Client.Send(buffer);
            }
        }
        void RecvMessage()
        {
            byte[] buffer = new byte[1024];
            for (int i = 0; i < buffer.Length; i++)
            {
                //Чистим буфер
                buffer[i] = 0;
            }
            for (; ; )
            {
                try
                {
                    Client.Receive(buffer);
                    string message = Encoding.UTF8.GetString(buffer);
                    int count = message.IndexOf(";;;5"); //Конец сообщения
                    if (count == -1) //Проверяем
                    {
                        continue;
                    }
                    string Clear_Message = "";
                    for (int i = 0; i < count; i++)
                    {
                        Clear_Message += message[i];
                    }
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = 0;
                    }
                    this.Invoke((MethodInvoker)delegate()
                    {
                        richTextBox1.AppendText(Clear_Message);
                    });
                }
                catch (Exception ex) {}
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox1.Text != "")
            {
                button1.Enabled = true;
                richTextBox2.Enabled = true;
                Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (ip != null)
                {
                    Client.Connect(ip, port);//подлючение к серверу
                    th = new Thread(delegate()
                        {
                            RecvMessage();
                        });
                    th.Start(); //Запускаем поток, который будет принимать сообщения от сервера
                }
            }            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendMessage("\n" + textBox1.Text + ": " + richTextBox2.Text + ";;;5");
            richTextBox2.Clear();
        }

        private void выходToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if(th != null) th.Abort(); //Закрываем поток
            Application.Exit();
        }
    }
}
