namespace QuotesApi.Models.Quotes
{
    public interface IQuote
    {
        string GuildId { get; }
        public string Title { get; }
        string Text { get; }
    }
}