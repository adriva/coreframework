using System;
using System.Collections.Generic;
using Adriva.Common.Core;
using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Adriva.Web.Controls
{
    [DefaultName("bootstrap-grid")]
    public partial class BootstrapGridRenderer : DefaultControlRenderer<Grid>
    {
        public BootstrapGridRenderer(IServiceProvider serviceProvider, IOptions<WebControlsRendererOptions> rendererOptionsAccessor, IOptions<WebControlsOptions> optionsAccessor)
            : base(serviceProvider, rendererOptionsAccessor, optionsAccessor)
        {

        }

        protected override IEnumerable<string> ResolveAssetPaths(IControlOutputContext context)
        {
            return new[]{
                "https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css",
                "https://use.fontawesome.com/releases/v5.6.3/css/all.css",
                "https://unpkg.com/bootstrap-table@1.18.0/dist/bootstrap-table.min.css",
                "https://cdnjs.cloudflare.com/ajax/libs/jquery/3.5.1/jquery.min.js",
                "https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js",
                "https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js",
                "https://unpkg.com/bootstrap-table@1.18.0/dist/bootstrap-table.min.js"
            };
        }

        protected override void RenderRootControl(Grid grid, IControlOutputContext context)
        {
            foreach (var c in grid.Columns)
            {
                if (!string.IsNullOrWhiteSpace(c.Formatter))
                {
                    var s = base.GenerateWrappedScriptCall(c.Formatter, 2, out string fn);
                    c.Formatter = fn;
                    context.Output.PostContent.AppendHtml($"<script>{s}</script>");
                }
            }

            TagBuilder tagBuilder = new TagBuilder("table");
            tagBuilder.TagRenderMode = TagRenderMode.Normal;
            tagBuilder.GenerateId(context.Id, "_");

            context.Output.Content.SetHtmlContent(tagBuilder);

            var contractResolver = new BootstrapGridRenderer.GridContractResolver();

            contractResolver.AddTypeMapping<Grid>()
                .MapProperty(x => x.DataSource, "url")
                .MapProperty(x => x.Columns, "columns")
                .MapProperty(x => x.Height, "height");

            contractResolver.AddTypeMapping<GridColumn>()
                .MapProperty(x => x.Field, "field")
                .MapProperty(x => x.Title, "title")
                .MapProperty(x => x.Width, "width")
                .MapProperty(x => x.Formatter, "formatter")
                .MapProperty(x => x.Alignment, "align")
                .MapProperty(x => x.IsHidden, "visible", shouldNegate: true);

            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = contractResolver
            };

            var json = Utilities.SafeSerialize(grid, jsonSerializerSettings);

            context.Output.PostContent.AppendHtml(base.GenerateInitializerScript(context, $"$('#{context.Id}').bootstrapTable({json});"));
        }
    }
}