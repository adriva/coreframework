using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{
    public interface IControlOutputContext
    {
        TagHelperOutput Output { get; }

        ControlTagHelper Control { get; }

        IControlOutputContext Parent { get; }

        IList<IControlOutputContext> Children { get; }
    }
}