using System.Net;

namespace QuotesApi.Exceptions
{
    public class BadRequestException : HttpException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public BadRequestException(string message) : base(message)
        {
        }
    }
}
