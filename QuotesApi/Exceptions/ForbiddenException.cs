using System.Net;

namespace QuotesApi.Exceptions
{
    public class ForbiddenException : HttpException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;

        public ForbiddenException(string message) : base(message)
        {
        }
    }
}
