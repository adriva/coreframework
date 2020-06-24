using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{
    internal class ControlOutputContext : IControlOutputContext
    {
        public ControlTagHelper Control { get; set; }

        public IControlOutputContext Parent { get; set; }

        public TagHelperOutput Output { get; private set; }

        public IList<IControlOutputContext> Children { get; private set; }

        public ControlOutputContext(TagHelperOutput output)
        {
            this.Output = output;
            this.Children = new List<IControlOutputContext>();
        }

        public override string ToString() => $"{this.Control} [ControlOutputContext]";
    }
}