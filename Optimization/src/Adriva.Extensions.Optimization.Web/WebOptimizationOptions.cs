using System;
using System.Collections.Generic;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Adriva.Extensions.Optimization.Web
{
    public class WebOptimizationOptions : OptimizationOptions
    {
        public PathString StaticAssetsPath { get; set; } = "/assets";

        public bool BundleStylesheets { get; set; }

        public bool BundleJavascripts { get; set; }

        public bool MinifyStylesheets { get; set; }

        public bool MinifyJavascripts { get; set; }

        public bool MinifyHtml { get; set; }
    }
}
