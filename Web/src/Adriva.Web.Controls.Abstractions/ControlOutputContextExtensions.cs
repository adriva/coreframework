using Microsoft.AspNetCore.Http;

namespace Adriva.Web.Controls.Abstractions
{
    public static class ControlOutputContextExtensions
    {
        public static HttpContext GetHttpContext(this IControlOutputContext context)
        {
            return context?.Control?.ViewContext?.HttpContext;
        }
    }
}