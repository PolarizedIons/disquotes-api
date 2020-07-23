using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace QuotesApi.RouteConstraints
{
    public class ULongConstraint : IRouteConstraint
    {
        public const string Name = "ulong";

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out object value))
            {
                return ulong.TryParse(value.ToString(), out _);
            }

            return false;
        }
    }
}
