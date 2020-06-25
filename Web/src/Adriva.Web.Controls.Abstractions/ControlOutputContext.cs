using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls.Abstractions
{
    internal class ControlOutputContext : IControlOutputContext
    {
        public string Id { get; private set; }

        public ControlTagHelper Control { get; set; }

        public IControlOutputContext Parent { get; set; }

        public TagHelperOutput Output { get; private set; }

        public IList<IControlOutputContext> Children { get; private set; }

        public ControlOutputContext(string id, TagHelperOutput output)
        {
            this.Id = id;
            this.Output = output;
            this.Children = new List<IControlOutputContext>();
        }

        public override string ToString() => $"{this.Control} [ControlOutputContext]";
    }
}