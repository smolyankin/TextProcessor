using System.Data.Entity;
using TextProcessor.Models;

namespace TextProcessor.Context
{
    /// <summary>
    /// контекст для работы с бд
    /// </summary>
    public class WordContext : DbContext
    {
        public WordContext() : base("DBConnection")
        {
            
        }

        /// <summary>
        /// таблица слов
        /// </summary>
        public DbSet<Word> Words { get; set; }
    }
}
