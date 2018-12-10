﻿using System;
using System.Linq;

namespace TextProcessor.Admin
{
    class Program
    {
        private static string message = string.Empty; //сообщение пользователю

        [STAThread]
        static void Main(string[] args)
        {
            Service service = new Service();
            
            if (args.Length == 1)
            {
                int.TryParse(args.FirstOrDefault(), out int arg);
                switch (arg)
                {
                    case 0:
                        Console.WriteLine("Загрузите текстовый файл для создания словаря...");
                        message = service.CreateDictionary().GetAwaiter().GetResult();
                        break;
                    case 1:
                        Console.WriteLine("Загрузите текстовый файл для дополнения словаря...");
                        message = service.UpdateDictionary().GetAwaiter().GetResult();
                        break;
                    case 2:
                        Console.WriteLine("Очистить словарь? (Y)es / No");
                        if (Console.ReadKey().Key == ConsoleKey.Y)
                            message = service.ClearDictionary().GetAwaiter().GetResult();
                        else
                            message = "Словарь не очищен. Нажмите любую кнопку для выхода";
                        break;
                }
            }
            else
            {
                NoArgs(service);
            }
            Console.WriteLine(message);
            Console.ReadKey();
        }

        /// <summary>
        /// работа со словарем без аргументов
        /// </summary>
        /// <param name="service">сервис</param>
        private static void NoArgs(Service service)
        {
            Console.WriteLine(@"Вы можете запустить приложение с аргументами: 0 - создание словаря, 1 - обновление словаря, 2 - очистка словаря
1 Создать словарь
2 Обновить словарь
3 Очистить словарь
Вы можете продолжить выбрав один из пунктов либо выйти из программы.");
            var input = Console.ReadKey();

            switch (input.Key)
            {
                case ConsoleKey.D1:
                    message = service.CreateDictionary().GetAwaiter().GetResult();
                    break;
                case ConsoleKey.D2:
                    message = service.UpdateDictionary().GetAwaiter().GetResult();
                    break;
                case ConsoleKey.D3:
                    Console.WriteLine("\r\nОчистить словарь? (Y)es / No");
                    if (Console.ReadKey().Key == ConsoleKey.Y)
                        message = service.ClearDictionary().GetAwaiter().GetResult();
                    else
                        message = "Словарь не очищен. Нажмите любую кнопку для выхода";
                    break;
            }
        }
    }
}
