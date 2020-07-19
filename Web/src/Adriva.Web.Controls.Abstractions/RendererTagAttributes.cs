using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls.Abstractions
{
    public class RendererTagAttributes : ReadOnlyTagHelperAttributeList
    {
        public RendererTagAttributes() : base()
        {

        }

        public RendererTagAttributes(IEnumerable<TagHelperAttribute> attributes) : base(attributes.ToList())
        {

        }
    }
}
