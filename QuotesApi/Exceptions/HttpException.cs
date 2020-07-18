using System.Net;
using System.Net.Http;
using QuotesApi.Extentions;

namespace QuotesApi.Exceptions
{
    public abstract class HttpException : HttpRequestException
    {
        private readonly string _message;

        public abstract HttpStatusCode StatusCode { get; }

        public new string Message => StatusCode.ToString().SplitTitleCase() + (string.IsNullOrEmpty(_message) ? "" : $": {_message}");

        public HttpException(string message)
        {
            _message = message;
        }
    }
}
