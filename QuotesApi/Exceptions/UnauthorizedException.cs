using System.Net;

namespace QuotesApi.Exceptions
{
    public class UnauthorizedException : HttpException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;

        public UnauthorizedException(string message) : base(message)
        {
        }
    }
}
