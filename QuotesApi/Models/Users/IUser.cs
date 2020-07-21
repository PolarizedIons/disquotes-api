namespace QuotesApi.Models.Users
{
    public interface IUser
    {
        string DiscordId { get; }
        string Username { get; }
        int Discriminator { get; }
        string ProfileUrl { get; }
    }
}