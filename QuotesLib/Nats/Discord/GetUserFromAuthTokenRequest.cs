namespace QuotesLib.Nats.Discord
{
    public class GetUserFromAuthTokenRequest : INatsRequest
    {
        public string Token { get; set; }
    }
}
