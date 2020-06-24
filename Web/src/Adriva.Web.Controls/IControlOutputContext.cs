using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{
    public interface IControlOutputContext
    {
        string Id { get; }

        TagHelperOutput Output { get; }

        ControlTagHelper Control { get; }

        IControlOutputContext Parent { get; }

        IList<IControlOutputContext> Children { get; }
    }
}