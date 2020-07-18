using System.Net;

namespace QuotesApi.Exceptions
{
    public class NotFoundException : HttpException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
        
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
