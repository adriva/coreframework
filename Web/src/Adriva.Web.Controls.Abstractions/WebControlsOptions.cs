using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Adriva.Web.Controls.Abstractions
{
    public class WebControlsOptions
    {
        public string OptimizationContextName { get; set; } = Options.DefaultName;

        public PathString DefaultAssetRootPath { get; } = "/webcontrols/assets";

        public PathString AssetsRootPath { get; set; } = "/webcontrols/assets";

        internal IList<Assembly> ControlLibraries { get; } = new List<Assembly>();
    }
}
