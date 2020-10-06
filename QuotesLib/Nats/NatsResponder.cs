using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using QuotesLib.Services;

namespace QuotesLib.Nats
{
    public class NatsResponder
    {
        protected readonly NatsService _natsService;

        protected NatsResponder(NatsService natsService)
        {
            _natsService = natsService;

            var methods = GetType().GetMethods();
            foreach (var methodInfo in methods)
            {
                if (!methodInfo.IsPublic || methodInfo.IsStatic)
                {
                    continue;
                }

                var methodParams = methodInfo.GetParameters();
                if (methodParams.Length == 1 && typeof(INatsRequest).IsAssignableFrom(methodParams.First().ParameterType))
                {
                    var eventName = methodParams.First().ParameterType.FullName;
                    _natsService.Subscribe(eventName, methodInfo, this, methodParams.First().ParameterType);
                }
            }
        }

        public static IEnumerable<NatsResponder> ActivateAll(IServiceProvider serviceProvider)
        {
            return Assembly.GetEntryAssembly()?.GetExportedTypes()
                .Where(x => typeof(NatsResponder).IsAssignableFrom(x))
                .Select(x => (NatsResponder)serviceProvider.GetRequiredService(x));
        }
    }
}
