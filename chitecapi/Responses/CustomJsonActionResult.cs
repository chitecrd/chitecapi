using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace chitecapi.Responses
{
    public class CustomJsonActionResult : IHttpActionResult
    {
        private readonly string jsonString;
        private readonly HttpStatusCode httpStatusCode;

        public CustomJsonActionResult(HttpStatusCode httpStatusCode, JsonErrorResponse jsonResponse)
        {
            jsonString = jsonResponse.JsonString;
            this.httpStatusCode = httpStatusCode;
        }

        public CustomJsonActionResult(HttpStatusCode httpStatusCode, JsonDataResponse jsonResponse)
        {
            jsonString = jsonResponse.JsonString;
            this.httpStatusCode = httpStatusCode;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var content = new StringContent(jsonString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = new HttpResponseMessage(httpStatusCode) { Content = content };
            return Task.FromResult(response);
        }
    }
}
