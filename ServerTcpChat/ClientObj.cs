using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ChatServer
{
    public class ClientObj
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        public string userName;
        TcpClient client;
        ServerObj server; // объект сервера
        public string nameRoom;

        string path = @"text.txt";
        protected void writeText(string text)
        {
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
            {
                sw.Write(text + "\r\n");
            }
        }

        public ClientObj(TcpClient tcpClient, ServerObj ServerObj)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = ServerObj;
            ServerObj.AddConnection(this);
            nameRoom = "Общая";
        }

        internal void ban()
        {
            string ex = String.Format("{0}: покинул чат", userName);
            writeText(ex);
            server.RemoveConnection(this.Id);
            server.BroadcastMessage(ex, nameRoom);
            string message = "\r\nАктивные пользователи:\r\n";
            server.BroadcastMessage(message, nameRoom);
            Close();
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                // получаем имя пользователя
                string message = GetMessage();
                
                Regex regex = new Regex(@"/room:(.*)/-");
                var matches = regex.Match(message);
                nameRoom = matches.Groups[1].Value;
                message = message.Replace(@"/room:" + nameRoom + @"/-", "");
                userName = message;

                message = userName + " вошел в чат";
                // посылаем сообщение о входе в чат всем подключенным пользователям
                server.BroadcastMessage(message, nameRoom);

                writeText(message);

                message = "\r\nАктивные пользователи:\r\n";
                server.BroadcastMessage(message, nameRoom);

                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = GetMessage();

                        matches = regex.Match(message);
                        nameRoom = matches.Groups[1].Value;
                        message = message.Replace(@"/room:" + nameRoom + @"/-", "");

                        if(message == "/quit/")
                        {
                            string ex = String.Format("{0}: покинул чат", userName);
                            writeText(ex);
                            server.RemoveConnection(this.Id);
                            server.BroadcastMessage(ex, nameRoom);
                            message = "\r\nАктивные пользователи:\r\n";
                            server.BroadcastMessage(message, nameRoom);
                            Close();
                            break;
                        }
                        else
                        {
                            message = String.Format("{0}: {1}", userName, message);
                        }

                        writeText(message);
                        server.BroadcastMessage(message, nameRoom);
                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", userName);

                        writeText(message);
                        server.RemoveConnection(this.Id);
                        server.BroadcastMessage(message, nameRoom);

                        message = "\r\nАктивные пользователи:\r\n";
                        server.BroadcastMessage(message, nameRoom);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                writeText(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}