using System.Net;

namespace imt_wankeyun_client.Entities
{
    public class HttpMessage
    {
        public HttpStatusCode statusCode { get; set; }
        public object data { get; set; }
    }
}
