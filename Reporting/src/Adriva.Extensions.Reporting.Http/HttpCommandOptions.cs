using System.Net.Http;

namespace Adriva.Extensions.Reporting.Http
{
    public class HttpCommandOptions
    {
        public string DataElement { get; set; }

        public string Method { get; set; } = HttpMethod.Get.Method;
    }
}