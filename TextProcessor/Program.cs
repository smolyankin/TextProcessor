using System;
using System.Linq;
using System.Text;
using TextProcessor.Context;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TextProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = null;
            var port = 5000; //порт по умолчанию

            try
            {
                if (args.Length >= 1)
                {
                    try
                    {
                        int.TryParse(args[0], out int newPort);
                        if (newPort > 0 && newPort <= 65535)
                            port = newPort;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
                server = new TcpListener(IPAddress.Any, port);
                server.Start();

                while (true)
                {
                    Console.WriteLine("Ожидание подключений... ");
                    TcpClient client = server.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(SearchEngine, client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (server != null)
                    server.Stop();
            }
        }
        
        /// <summary>
        /// обработчик запросов
        /// </summary>
        /// <param name="obj">tcp client</param>
        static void SearchEngine(object obj)
        {
            try
            {
                var client = (TcpClient)obj;
                using (WordContext db = new WordContext())
                {
                    Console.WriteLine("Подключен клиент. Выполнение запроса...");
                    NetworkStream stream = client.GetStream();
                    byte[] bytes = new byte[client.ReceiveBufferSize];
                    stream.Read(bytes, 0, (int)client.ReceiveBufferSize);
                    var search = Encoding.UTF8.GetString(bytes);
                    var response = new StringBuilder(string.Empty);
                    search = search.Replace("\0", "");
                    var words = db.Words.Where(x => x.Title.StartsWith(search)).OrderByDescending(x => x.Count).Take(5).ToList();
                    if (words.Any())
                    {
                        foreach (var word in words)
                        {
                            response.Append(word.Title);
                            response.Append("\r\n");
                        }

                        response.ToString();
                    }
                    else
                        response.Append("Слов не найдено. Измените запрос...\r\n");

                    byte[] data = Encoding.UTF8.GetBytes(response.ToString());
                    stream.Write(data, 0, data.Length);

                    Console.WriteLine("Отправлено сообщение: \r\n{0}", response);

                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
