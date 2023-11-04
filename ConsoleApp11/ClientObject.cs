using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ConsoleApp11
{
    class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        string userName;
        TcpClient client;
        ServerObject server;
        String hello= " поприветствуем нового пользователя \U0001F600";
            // объект сервера

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            string Name;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                Console.WriteLine("::::");
                // получаем имя пользователя
                string message =  GetMessage();
                Console.WriteLine(message);
                string[] login = message.Split("><");
                String mode= login[0];
                userName = login[1];
                String pin = login[2];
                Console.WriteLine(pin);
                StreamReader sr = new StreamReader("база.txt");
                String[] basepolsovatel = sr.ReadToEnd().Split('|');
                sr.Close();
                StreamReader parol = new StreamReader("база паролей.txt");
                String[] baseparol = parol.ReadToEnd().Split('|');
                parol.Close();
                for (int i = 0;i<basepolsovatel.Length;i++) {
                    Console.WriteLine(basepolsovatel[i]);
                
                }
                if (mode.Equals("e"))
                {
                    if (basepolsovatel.Length != 0)
                    {
                        bool falg = true;
                        for (int i = 0; i < basepolsovatel.Length; i++)
                        {
                            if (userName == basepolsovatel[i] || basepolsovatel[i] == userName)
                            {
                                falg = false;
                                hello = " вошел в чат 😁";

                                if (baseparol[i] != pin.ToString())
                                {
                                    Stream.Write(UTF8Encoding.UTF8.GetBytes("YOURAPPLE"), 0, UTF8Encoding.UTF8.GetBytes("YOURAPPLE").Length);
                                    Stream.Flush();
                                   

                                    break;
                                }
                                else
                                {

                                    Stream.Write(UTF8Encoding.UTF8.GetBytes("YOUAReAPPLE"), 0, UTF8Encoding.UTF8.GetBytes("YOUAReAPPLE").Length);
                                    Stream.Flush(); Console.WriteLine("+-");
                                }
                                break;
                            }
                        }
                        if (falg) {
                            Stream.Flush();
                            Stream.Close();
                        }
                    }
                   
                }
                if (mode.Equals("r"))
                {
                    bool register=false;
                    for (int i = 0; i < basepolsovatel.Length; i++)
                    {
                        if (userName == basepolsovatel[i] || basepolsovatel[i] == userName)
                        {
                            register = true;
                            Stream.Write(UTF8Encoding.UTF8.GetBytes("loginzanyat"), 0, UTF8Encoding.UTF8.GetBytes("loginzanyat").Length);
                            Stream.Flush();
                            
                        }
                     }
                    if (!register)
                    {
                        using (StreamWriter fileStream = File.AppendText("база.txt"))
                        {
                            fileStream.Write(userName + "|");
                            fileStream.Close();
                        }
                        using (StreamWriter fileStreamparol = File.AppendText("база паролей.txt"))
                        {
                            fileStreamparol.Write(pin + "|");
                            fileStreamparol.Close();
                        }
                        Stream.Write(UTF8Encoding.UTF8.GetBytes("успешно"), 0, UTF8Encoding.UTF8.GetBytes("успешно").Length);
                        Stream.Flush();
                        hello = " поприветствуем нового пользователя \U0001F600";
                    }
                       
                    }
                 
                    Console.Beep();


                message = userName + hello;
                // посылаем сообщение о входе в чат всем подключенным пользователям
                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message);
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = String.Format("{0}: {1}", userName, message);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат 😥", userName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
            byte[] data = new byte[10000]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                if (bytes == null ) {
                    server.RemoveConnection(this.Id);
                    Stream.Close();
                    client.Close();
                    Close();
                }
                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                if (builder == null||builder.ToString()== "") {
                    server.RemoveConnection(this.Id);
                    Stream.Close();
                    client.Close();
                    Close();
                }
            }
            while (Stream.DataAvailable);
            if (builder.ToString().Contains("ClOsE")) {
                server.RemoveConnection(this.Id);
                Stream.Close();
                client.Close();
                Close();
            }
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
