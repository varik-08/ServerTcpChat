using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace ChatServer
{
    public class ServerObj
    {
        static TcpListener tcpListener; // сервер для прослушивания
        List<ClientObj> clients = new List<ClientObj>(); // все подключения

        string path = @"text.txt";
        protected void writeText(string text)
        {
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
            {
                sw.Write(text + "\r\n");
            }
        }

        protected internal void AddConnection(ClientObj ClientObj)
        {
            clients.Add(ClientObj);
        }
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObj client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }
        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();

                writeText("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObj ClientObj = new ClientObj(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(ClientObj.Process));
                    Thread clientBan = new Thread(new ThreadStart(banUser));
                    clientBan.Start();
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                writeText(ex.Message);

                Disconnect();
            }
        }

        protected internal void banUser()
        {
            while (true)
            {
                string path = @"banUser.txt";
                string name = "";
                using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
                {
                    name = sr.ReadToEnd();
                }
                using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
                {
                    sw.Write("");
                }

                if (name != "")
                {
                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (clients[i].userName == name)
                        {
                            clients[i].ban();
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        // трансляция сообщения подключенным клиентам
        protected internal void BroadcastMessage(string message, string nameRoom)
        {
            if (message == "\r\nАктивные пользователи:\r\n")
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    if (clients[i].nameRoom == nameRoom)
                    {
                        message += clients[i].userName + "\r\n";
                    }
                }
                message += "\r\n";
                writeText(message);
            }
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].nameRoom == nameRoom)
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                }
            }
        }
        // отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }
    }
}