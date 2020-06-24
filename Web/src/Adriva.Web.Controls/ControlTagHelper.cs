using System;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{

    public abstract class ControlTagHelper : TagHelper
    {
        private static readonly string RandomContextKey;

        private IControlRenderer ControlRenderer;

        static ControlTagHelper()
        {
            ControlTagHelper.RandomContextKey = $"controlContext_{Guid.NewGuid()}";
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        protected TagHelperContext TagHelperContext { get; private set; }

        protected static string GetOrGenerateControlId(TagHelperOutput output, string prefix = "ctrl_")
        {
            string controlId = $"{prefix}{Utilities.GetRandomId(6)}";

            if (output.Attributes.TryGetAttribute("id", out TagHelperAttribute idAttribute))
            {
                controlId = Convert.ToString(idAttribute.Value);
            }
            else
            {
                output.Attributes.Add("id", controlId);
            }

            return controlId;
        }

        public override void Init(TagHelperContext context)
        {
            this.ControlRenderer = new DefaultControlRenderer();
            base.Init(context);
            this.TagHelperContext = context;
        }

        private ControlOutputContext GetContext(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            ControlOutputContext currentContext = new ControlOutputContext(output) { Control = this };

            if (tagHelperContext.Items.TryGetValue(ControlTagHelper.RandomContextKey, out object obj))
            {
                ControlOutputContext parentContext = obj as ControlOutputContext;
                currentContext.Parent = parentContext;
                parentContext.Children.Add(currentContext);
            }

            return currentContext;
        }

        public sealed override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ControlOutputContext controlOutputContext = this.GetContext(context, output);
            this.Process(controlOutputContext);

            context.Items[ControlTagHelper.RandomContextKey] = controlOutputContext;
        }

        public sealed override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ControlOutputContext currentContext = this.GetContext(context, output);

            await this.ProcessAsync(currentContext);

            context.Items[ControlTagHelper.RandomContextKey] = currentContext;

            _ = await output.GetChildContentAsync();

            if (null == currentContext.Parent) this.ControlRenderer.Render(currentContext);
        }

        protected virtual void Process(IControlOutputContext context) { }

        protected virtual Task ProcessAsync(IControlOutputContext context) => Task.CompletedTask;
    }
}