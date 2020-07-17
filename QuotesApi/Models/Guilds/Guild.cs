namespace QuotesApi.Models.Guilds
{
    public class Guild
    {
        public ulong Id { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public ulong? SystemChannelId { get; set; }
    }
}