using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextProcessor.Models;
using TextProcessor.Context;
using System.IO;
using System.Data.Common;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TextProcessor.Admin
{
    public class Service
    {
        public async Task<string> CreateDictionary()
        {
            using (WordContext db = new WordContext())
            {
                Console.WriteLine("\r\n");
                var exist = db.Words.ToList();
                if (exist != null && exist.Any())
                    return "Словарь не пуст. Для создания нового словаря сперва очистите старый";
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.ShowDialog();
                var text = File.ReadAllText(dialog.FileName);
                //записать данные в бд
                text = Regex.Replace(text, "[\u0020-\u007E\u00A0-\u00FF\\r\\n\\t]", " ");
                var textArray = text.Split(' ').ToList();
                textArray.RemoveAll(x => string.IsNullOrEmpty(x));
                var words = textArray.Where(x => x.Length <= 15 && x.Length >= 3).ToList();
                var mylist = new List<string>();
                foreach (var word in words)
                    if (!String.IsNullOrEmpty(word) || !String.IsNullOrWhiteSpace(word))
                        mylist.Add(word.ToLowerInvariant());
                var group = mylist.GroupBy(g => g);
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

        public void GetWords()
        {
            using (WordContext db = new WordContext())
            {
                var exist = db.Words.Where(w => w.Count > 10).ToList();
                exist = exist.OrderByDescending(x => x.Count).ToList();
                foreach (var word in exist)
                    Console.WriteLine(word.Title + " - " + word.Count);
            }
        }

        public async Task<string> UpdateDictionary()
        {
            using (WordContext db = new WordContext())
            {
                Console.WriteLine("\r\n");
                var exist = db.Words.ToList();
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.ShowDialog();
                var text = File.ReadAllText(dialog.FileName);
                //записать данные в бд
                text = Regex.Replace(text, "[\u0020-\u007E\u00A0-\u00FF\\r\\n\\t]", " ");
                var textArray = text.Split(' ').ToList();
                textArray.RemoveAll(x => string.IsNullOrEmpty(x));
                var words = textArray.Where(x => x.Length <= 15 && x.Length >= 3).ToList();
                var mylist = new List<string>();
                foreach (var word in words)
                    if (!String.IsNullOrEmpty(word) || !String.IsNullOrWhiteSpace(word))
                        mylist.Add(word.ToLowerInvariant());
                var group = mylist.GroupBy(g => g);
                long i = 0;
                var list = new List<Word>();
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
                    Console.Write(i * 100 / group.Where(w => w.LongCount() >= 3).Count() + " %");
                    Console.SetCursorPosition(l, t);
                    i++;
                }
                db.Words.AddRange(list);
                await db.SaveChangesAsync();
                return "Словарь обновлен.";
            }
        }

        public async Task<string> ClearDictionary()
        {
            using (WordContext db = new WordContext())
            {
                var exist = db.Words;
                db.Words.RemoveRange(exist);
                await db.SaveChangesAsync();
                return "\r\nСловарь очищен";
            }
        }
    }
}
