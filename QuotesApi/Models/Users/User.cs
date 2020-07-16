namespace QuotesApi.Models.Users
{
    public class User : DbEntity, IUser
    {
        public ulong DiscordId { get; set; }
        public string Username { get; set; }
        public int Discriminator { get; set; }
        public string ProfileUrl { get; set; }
    }
}
