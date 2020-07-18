namespace QuotesApi.Models.Guilds
{
    public class Guild
    {
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string? SystemChannelId { get; set; }

        public bool IsOwner { get; set; }
    }
}
