using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;

namespace Adriva.Web.Controls.Abstractions
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ControlTagHelper : TagHelper
    {
        private static readonly string RandomContextKey;

        [HtmlAttributeNotBound]
        protected virtual bool RequiresRenderer { get => false; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        protected TagHelperContext TagHelperContext { get; private set; }

        static ControlTagHelper()
        {
            ControlTagHelper.RandomContextKey = $"controlContext_{Guid.NewGuid()}";
        }

        private static string GetOrGenerateControlId(TagHelperOutput output, string prefix = "ctrl_")
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
            base.Init(context);
            this.TagHelperContext = context;
        }

        private ControlOutputContext GetContext(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            string id = ControlTagHelper.GetOrGenerateControlId(output);

            ControlOutputContext currentContext = new ControlOutputContext(id, output) { Control = this };

            if (tagHelperContext.Items.TryGetValue(ControlTagHelper.RandomContextKey, out object obj))
            {
                ControlOutputContext parentContext = obj as ControlOutputContext;
                currentContext.Parent = parentContext;
                parentContext.Children.Add(currentContext);
            }

            return currentContext;
        }

        private bool EnsureRenderer(IControlOutputContext context)
        {
            if (this.RequiresRenderer && !(context.Parent?.Control is RendererTagHelper))
            {
                context.Output.Content.Clear();
                context.Output.SuppressOutput();
                context.Output.Content.SetHtmlContent($"<div>'{this.GetType().FullName}' requires a parent &lt;renderer&gt; since it doesn't have any default rendering logic.</div>");
                return false;
            }

            return true;
        }

        public sealed override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ControlOutputContext controlOutputContext = this.GetContext(context, output);

            if (this.EnsureRenderer(controlOutputContext))
            {
                this.Process(controlOutputContext);
                context.Items[ControlTagHelper.RandomContextKey] = controlOutputContext;
            }
        }

        public sealed override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ControlOutputContext currentContext = this.GetContext(context, output);

            if (this.EnsureRenderer(currentContext))
            {
                context.Items[ControlTagHelper.RandomContextKey] = currentContext;

                _ = await output.GetChildContentAsync();
                await this.ProcessAsync(currentContext);
            }
        }

        protected virtual void Process(IControlOutputContext context) { }

        protected virtual Task ProcessAsync(IControlOutputContext context)
        {
            this.Process(context);
            return Task.CompletedTask;
        }
    }
}