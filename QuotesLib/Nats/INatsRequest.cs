namespace QuotesLib.Nats
{
    public interface INatsRequest
    {
        public string NatsRequestName => GetType().FullName;
    }
}
