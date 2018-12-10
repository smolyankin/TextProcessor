namespace TextProcessor.Models
{
    /// <summary>
    /// сущность слова
    /// </summary>
    public class Word
    {
        /// <summary>
        /// уникальный идентификатор
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// слово
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// количество повторений
        /// </summary>
        public long Count { get; set; }
    }
}
