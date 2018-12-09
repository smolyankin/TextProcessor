using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextProcessor.Context;
using TextProcessor.Models;
using System.IO;
using System.Data.Common;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace TextProcessor
{
    class Program
    {
        //[STAThread]
        static void Main(string[] args)
        {
            TcpListener server = null;
            try
            {
                var port = 5000;
                IPAddress ipAddress = IPAddress.Any;
                IPEndPoint piont = new IPEndPoint(ipAddress, port);
                server = new TcpListener(piont);
                server.Start();

                while (true)
                {
                    Console.WriteLine("Ожидание подключений... ");
                    TcpClient client = server.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(ReturnWords, client);
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

        static void ReturnWords(object obj)
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
                    // сообщение для отправки клиенту
                    var response = new StringBuilder(string.Empty);
                    search = search.Replace("\0", "");
                    var words = db.Words.Where(x => x.Title.StartsWith(search)).OrderByDescending(x => x.Count).Take(5)
                        .ToList();
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

        static async Task<string> GetWords(string search)
        {
            using (WordContext db = new WordContext())
            {
                search = search.Replace("\0", "");
                var words = db.Words.Where(x => x.Title.StartsWith(search)).OrderByDescending(x => x.Count).ThenBy(x => x.Title).Take(5).ToList();

                if (words.Any())
                {
                    StringBuilder response = new StringBuilder();
                    foreach (var word in words)
                    {
                        response.Append("\r\n");
                        response.Append(word.Title);
                    }

                    return response.ToString();
                }
                else
                    return "Слов не найдено. Попробуйте снова.";
            }
        }
    }
}
