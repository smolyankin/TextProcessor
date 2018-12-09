using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace TextProcessor.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 5000;
            string server = "127.0.0.1";
            if (args.Length >= 1)
            {
                try
                {
                    IPAddress.TryParse(args[0], out IPAddress ipAdress);
                    if (ipAdress != null)
                        server = ipAdress.ToString();
                    else
                    {
                        IPHostEntry host = Dns.GetHostEntry(args[0]);
                        if (host.AddressList.Any())
                        {
                            var newIpAdress = host.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                            server = newIpAdress.ToString();
                        }
                    }

                    if (args.Length == 2)
                    {
                        int.TryParse(args[1], out int newPort);
                        if (newPort > 0 && newPort <= 65535)
                            port = newPort;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            while (true)
            {
                try
                {
                    TcpClient client = new TcpClient();
                    client.Connect(server, port);

                    byte[] data = new byte[256];
                    StringBuilder response = new StringBuilder();
                    NetworkStream stream = client.GetStream();
                    Console.WriteLine("Введите запрос:");
                    var search = Console.ReadLine();
                    Byte[] sendBytes = Encoding.UTF8.GetBytes(search);
                    stream.Write(sendBytes, 0, sendBytes.Length);
                    do
                    {
                        int bytes = stream.Read(data, 0, data.Length);
                        response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    Console.WriteLine(response.ToString());

                    stream.Close();
                    client.Close();
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                }

                Console.WriteLine("Запрос завершен...");
            }
        }
    }
}
