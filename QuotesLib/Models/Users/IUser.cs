namespace QuotesApi.Models.Users
{
    public interface IUser
    {
        string DiscordId { get; }
        string Username { get; }
        string Discriminator { get; }
        string ProfileUrl { get; }
    }
}