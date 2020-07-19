using Microsoft.AspNetCore.Mvc.Razor;
using System.IO;
using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;

namespace Adriva.Web.Controls.Abstractions
{
    internal static class Extensions
    {
        public static void WriteTo(this IHtmlContent htmlContent, RazorPageBase razorPageBase)
        {
            using (StringWriter writer = new StringWriter())
            {
                htmlContent.WriteTo(writer, HtmlEncoder.Default);
                var buffer = writer.GetStringBuilder();
                razorPageBase.WriteLiteral(buffer);
            }
        }
    }
}