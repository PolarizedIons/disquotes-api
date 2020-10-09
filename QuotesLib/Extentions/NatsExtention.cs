using System.Text.Json;
using NATS.Client;

namespace QuotesLib.Extentions
{
    public static class NatsExtention
    {
        public static T GetData<T>(this Msg msg)
        {
            return JsonSerializer.Deserialize<T>(msg.Data);
        }
    }
}
