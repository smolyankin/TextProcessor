using System.Data.Entity;
using TextProcessor.Models;

namespace TextProcessor.Context
{
    public class WordContext : DbContext
    {
        public WordContext() : base("DBConnection")
        {
            //Database.SetInitializer<WordContext>(null);
        }

        public DbSet<Word> Words { get; set; }
    }
}
