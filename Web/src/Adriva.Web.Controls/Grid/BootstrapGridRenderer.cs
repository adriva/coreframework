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
        public BootstrapGridRenderer(IServiceProvider serviceProvider, WebControlsRendererOptions rendererOptions, IControlRendererEvents events, IOptions<WebControlsOptions> optionsAccessor)
            : base(serviceProvider, rendererOptions, events, optionsAccessor)
        {

        }

        protected override IEnumerable<string> ResolveAssetPaths(IControlOutputContext context)
        {
            var viewContext = context.Control.ViewContext;
            return new[]{
                viewContext.Asset("jquery.js"),
                viewContext.Asset("bootstrap.bundle.js"),
                viewContext.Asset("bootstrap.table.js"),
                viewContext.Asset("bootstrap.min.css"),
                viewContext.Asset("bootstrap.table.css"),
                viewContext.Asset("font-awesome.css")
            };
        }

        protected override void RenderRootControl(Grid grid, IControlOutputContext context)
        {
            foreach (var column in grid.Columns)
            {
                if (!string.IsNullOrWhiteSpace(column.Formatter))
                {
                    var formatterScript = RendererUtilities.GenerateWrappedScriptCall(column.Formatter, 2, out string formatterFunctionName);
                    column.Formatter = formatterFunctionName;
                    context.Output.PostContent.AppendHtml($"<script>{formatterScript}</script>");
                }
            }

            TagBuilder tagBuilder = new TagBuilder("table");
            tagBuilder.TagRenderMode = TagRenderMode.Normal;
            tagBuilder.GenerateId(context.Id, "_");

            foreach (var attribute in context.Attributes)
            {
                tagBuilder.Attributes.TryAdd(attribute.Name, Convert.ToString(attribute.Value));
            }

            context.Output.Content.SetHtmlContent(tagBuilder);

            var contractResolver = new BootstrapGridRenderer.GridContractResolver();

            var mappingBuilder = contractResolver.AddTypeMapping<Grid>()
                .MapProperty(x => x.Columns, "columns")
                .MapProperty(x => x.Height, "height")
                .MapProperty(x => x.ShowDetails, "detailView")
                .MapProperty(x => x.ResponseFormatter, "responseHandler")
                .MapProperty(x => x.DataFieldName, "dataField", false)
                .MapProperty(x => x.TotalFieldName, "totalField", false)
                ;

            contractResolver.AddTypeMapping<GridColumn>()
                .MapProperty(x => x.Field, "field")
                .MapProperty(x => x.Title, "title")
                .MapProperty(x => x.Width, "width")
                .MapProperty(x => x.Formatter, "formatter")
                .MapProperty(x => x.Alignment, "align")
                .MapProperty(x => x.IsHidden, "visible", shouldNegate: true)
                ;


            if (grid.DataSource is string)
            {
                mappingBuilder.MapProperty(x => x.DataSource, "url");
            }
            else
            {
                mappingBuilder.MapProperty(x => x.DataSource, "data", new DataSourceConverter());
            }


            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = contractResolver
            };

            var json = Utilities.SafeSerialize(grid, jsonSerializerSettings);

            var postStartupFunction = RendererUtilities.GenerateWrappedScriptCall(grid.OnInitialized, new[] { "grid" }, out string postStartupFunctionName);

            context.Output.PostContent.AppendHtml(
                RendererUtilities.GenerateInitializerScript(
                    context,
                    this.OptimizationContextName,
                    $"var grid = $('#{context.Id}').bootstrapTable({json});",
                    string.IsNullOrWhiteSpace(grid.OnInitialized) ? null : $"{postStartupFunctionName}(grid);"
                )
            );

            if (!string.IsNullOrWhiteSpace(grid.OnInitialized))
            {
                context.Output.PreContent.AppendHtml($"<script>{postStartupFunction}</script>");
            }
        }
    }
}