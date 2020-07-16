namespace QuotesApi.Models.Users
{
    public interface IUser
    {
        ulong DiscordId { get; }
        string Username { get; }
        int Discriminator { get; }
        string ProfileUrl { get; }
    }
}