using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NATS.Client;
using QuotesLib.Models;
using QuotesLib.Nats;
using Serilog;

namespace QuotesLib.Services
{
    public class NatsService : IDisposable, ISingletonDiService
    {
        private readonly IConnection _connection;

        public NatsService(IConfiguration config)
        {
            ConnectionFactory cf = new ConnectionFactory();
            _connection = cf.CreateConnection($"nats://{config["Nats:Host"]}:{config["Nats:Port"]}");
            Log.Information("Connected to NATS.");
        }

        public Task<Msg> RequestAsync(INatsRequest request, int timeout = 2000)
        {
            return RequestAsync(request.NatsRequestName, request, timeout);
        }
        
        public Task<Msg> RequestAsync(string name, object body = null, int timeout = 2000)
        {
            return _connection.RequestAsync(name, body == null ? null : JsonSerializer.SerializeToUtf8Bytes(body), timeout);
        }

        public void Subscribe(string eventName, MethodInfo methodInfo, NatsResponder responder, Type parseType)
        {
            _connection.SubscribeAsync(eventName, async (sender, args) =>
            {
                var arg = JsonSerializer.Deserialize(args.Message.Data, parseType);
                Log.Debug("[Nats Request] {EventName}, {@data}", eventName, arg);
                var reply = methodInfo.Invoke(responder, new[] {arg});
                if ((methodInfo.ReturnParameter?.ParameterType.IsGenericType ?? false) && methodInfo.ReturnParameter?.ParameterType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    reply = ((dynamic) reply)?.Result;
                }
                else if (methodInfo.ReturnParameter?.ParameterType == typeof(Task))
                {
                    ((Task) reply)?.Wait();
                    reply = null;
                }
                Log.Debug("[Nats Response] {EventName}, {@data}", eventName, reply);

                args.Message.Respond(JsonSerializer.SerializeToUtf8Bytes(reply));
            });
        }

        public void Dispose()
        {
            _connection?.Drain();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
