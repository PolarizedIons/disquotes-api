namespace QuotesApi.Models.Quotes
{
    public interface IQuote
    {
        string GuildId { get; }
        string Text { get; }
    }
}