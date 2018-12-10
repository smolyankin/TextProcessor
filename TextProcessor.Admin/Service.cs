using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TextProcessor.Models;
using TextProcessor.Context;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TextProcessor.Admin
{
    /// <summary>
    /// сервис словаря
    /// </summary>
    public class Service
    {
        /// <summary>
        /// создание словаря
        /// </summary>
        /// <returns></returns>
        public async Task<string> CreateDictionary()
        {
            using (WordContext db = new WordContext())
            {
                Console.WriteLine("\r\n");
                var exist = db.Words.ToList();
                if (exist != null && exist.Any())
                    return "Словарь не пуст. Для создания нового словаря сперва очистите старый.";
                var words = await WordsFromFile();
                if (!words.Any())
                    return "Файл не был выбран или в файле не было подходящих слов";
                var group = words.GroupBy(g => g);
                long i = 0;
                var list = new List<Word>();
                foreach (var word in group.Where(w => w.LongCount() >= 3))
                {
                    list.Add(new Word
                    {
                        Title = word.FirstOrDefault(),
                        Count = word.LongCount()
                    });
                    var l = Console.CursorLeft;
                    var t = Console.CursorTop;
                    Console.Write(i * 100 / group.Where(w => w.LongCount() >= 3).Count() + " %");
                    Console.SetCursorPosition(l, t);
                    i++;
                }
                db.Words.AddRange(list);
                await db.SaveChangesAsync();
                return "Словарь создан.";
            }
        }

        /// <summary>
        /// дополнение словаря
        /// </summary>
        /// <returns></returns>
        public async Task<string> UpdateDictionary()
        {
            using (WordContext db = new WordContext())
            {
                Console.WriteLine("\r\n");
                var exist = db.Words.ToList();
                var words = await WordsFromFile();
                if (!words.Any())
                    return "Файл не был выбран или в файле не было подходящих слов";
                var group = words.GroupBy(g => g);
                long i = 0;
                var list = new List<Word>();
                var count = group.Where(w => w.LongCount() >= 3).Count();
                foreach (var word in group.Where(w => w.LongCount() >= 3))
                {
                    var wordTitle = word.FirstOrDefault().ToLowerInvariant();
                    if (exist.Exists(x => x.Title == wordTitle))
                    {
                        var existWord = db.Words.FirstOrDefault(x => x.Title == wordTitle);
                        existWord.Count += word.LongCount();
                    }
                    else
                    {
                        list.Add(new Word
                        {
                            Title = wordTitle,
                            Count = word.LongCount()
                        });
                    }
                    var l = Console.CursorLeft;
                    var t = Console.CursorTop;
                    Console.Write(i * 100 / count + " %");
                    Console.SetCursorPosition(l, t);
                    i++;
                }
                db.Words.AddRange(list);
                await db.SaveChangesAsync();
                return "Словарь дополнен.";
            }
        }

        /// <summary>
        /// очистка словаря
        /// </summary>
        /// <returns></returns>
        public async Task<string> ClearDictionary()
        {
            using (WordContext db = new WordContext())
            {
                var exist = db.Words;
                db.Words.RemoveRange(exist);
                await db.SaveChangesAsync();
                return "\r\nСловарь очищен.";
            }
        }

        /// <summary>
        /// получение списка слов из файла
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> WordsFromFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "TXT files (*.txt)|*.txt";
            dialog.ShowDialog();
            if (string.IsNullOrEmpty(dialog.FileName))
                return new List<string>();
            var text = File.ReadAllText(dialog.FileName);
            text = Regex.Replace(text, "[\u0020-\u007E\u00A0-\u00FF\\r\\n\\t]", " ");
            var textArray = text.Split(' ').ToList();
            textArray.RemoveAll(x => string.IsNullOrEmpty(x));
            var words = textArray.Where(x => x.Length <= 15 && x.Length >= 3).ToList();
            words = words.ConvertAll(w => w.ToLower());
            return words;
        }
    }
}
